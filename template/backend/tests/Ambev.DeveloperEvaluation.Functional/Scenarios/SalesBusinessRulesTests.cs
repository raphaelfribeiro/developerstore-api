using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Scenarios;

/// <summary>
/// Functional tests that verify sales business rules from the user's perspective.
/// Each test represents a complete business scenario — not just an HTTP contract check.
/// Tests run against a real PostgreSQL database via Testcontainers.
/// </summary>
[Collection("Functional")]
public class SalesBusinessRulesTests : BaseFunctionalTest
{
    public SalesBusinessRulesTests(FunctionalTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "Scenario: Discount tiers are correctly enforced across all pricing brackets")]
    public async Task DiscountTiers_AllBrackets_CorrectDiscountApplied()
    {
        await AuthenticateClientAsync(
            $"func.tiers{Guid.NewGuid():N}@test.com",
            "ValidPassword@123",
            $"functiers{Guid.NewGuid():N}");

        // No discount — below minimum threshold (3 items)
        var noDiscountResponse = await Client.PostAsJsonAsync("/api/sales", BuildSaleRequest(quantity: 3, unitPrice: 100m));
        noDiscountResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var noDiscountItem = ParseFirstItem(await noDiscountResponse.Content.ReadAsStringAsync());
        noDiscountItem.GetProperty("discount").GetDecimal().Should().Be(0m,
            because: "fewer than 4 identical items receive no discount");
        noDiscountItem.GetProperty("totalAmount").GetDecimal().Should().Be(300m,
            because: "3 × $100 with no discount = $300");

        // 10% discount — first tier (5 items, range 4–9)
        var tenPctResponse = await Client.PostAsJsonAsync("/api/sales", BuildSaleRequest(quantity: 5, unitPrice: 100m));
        tenPctResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenPctItem = ParseFirstItem(await tenPctResponse.Content.ReadAsStringAsync());
        tenPctItem.GetProperty("discount").GetDecimal().Should().Be(0.10m,
            because: "4–9 identical items receive a 10% discount");
        tenPctItem.GetProperty("totalAmount").GetDecimal().Should().Be(450m,
            because: "5 × $100 × 0.90 = $450");

        // 20% discount — second tier (15 items, range 10–20)
        var twentyPctResponse = await Client.PostAsJsonAsync("/api/sales", BuildSaleRequest(quantity: 15, unitPrice: 100m));
        twentyPctResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var twentyPctItem = ParseFirstItem(await twentyPctResponse.Content.ReadAsStringAsync());
        twentyPctItem.GetProperty("discount").GetDecimal().Should().Be(0.20m,
            because: "10–20 identical items receive a 20% discount");
        twentyPctItem.GetProperty("totalAmount").GetDecimal().Should().Be(1200m,
            because: "15 × $100 × 0.80 = $1200");
    }

    [Fact(DisplayName = "Scenario: Sale is rejected when a line item exceeds the 20-item maximum")]
    public async Task CreateSale_LineItemExceedsMaximumQuantity_IsRejected()
    {
        await AuthenticateClientAsync(
            $"func.maxqty{Guid.NewGuid():N}@test.com",
            "ValidPassword@123",
            $"funcmaxqty{Guid.NewGuid():N}");

        var response = await Client.PostAsJsonAsync("/api/sales", BuildSaleRequest(quantity: 21, unitPrice: 50m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "the business rule prohibits selling more than 20 identical items per line");
    }

    [Fact(DisplayName = "Scenario: Cancelling a sale item marks it as cancelled while the sale remains active")]
    public async Task CancelSaleItem_SaleRemainsActive_ItemMarkedCancelled()
    {
        await AuthenticateClientAsync(
            $"func.cancel{Guid.NewGuid():N}@test.com",
            "ValidPassword@123",
            $"funccancel{Guid.NewGuid():N}");

        // Create a sale with one item
        var createResponse = await Client.PostAsJsonAsync("/api/sales", BuildSaleRequest(quantity: 5, unitPrice: 100m));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var saleData = createJson.RootElement.GetProperty("data");
        var saleId = Guid.Parse(saleData.GetProperty("id").GetString()!);
        var itemId = Guid.Parse(saleData.GetProperty("items")[0].GetProperty("id").GetString()!);

        // Cancel the individual item
        var cancelResponse = await Client.PatchAsync($"/api/sales/{saleId}/items/{itemId}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "cancelling an existing item must succeed");

        // Verify sale-level and item-level state independently
        var getResponse = await Client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getJson = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        var retrievedSale = getJson.RootElement.GetProperty("data");

        retrievedSale.GetProperty("isCancelled").GetBoolean().Should().BeFalse(
            because: "cancelling one item must not cancel the entire sale");

        retrievedSale.GetProperty("items")[0]
            .GetProperty("isCancelled").GetBoolean().Should().BeTrue(
            because: "the cancelled item must be persisted with isCancelled = true");
    }

    private static object BuildSaleRequest(int quantity, decimal unitPrice) => new
    {
        saleNumber = $"FUNC-{Guid.NewGuid():N}",
        saleDate = DateTime.UtcNow,
        customerId = Guid.NewGuid(),
        customerName = "Functional Test Customer",
        branchId = Guid.NewGuid(),
        branchName = "Functional Test Branch",
        items = new[]
        {
            new
            {
                productId = Guid.NewGuid(),
                productName = "Functional Test Product",
                quantity,
                unitPrice
            }
        }
    };

    private static JsonElement ParseFirstItem(string responseBody)
    {
        var json = JsonDocument.Parse(responseBody);
        return json.RootElement.GetProperty("data").GetProperty("items")[0];
    }
}
