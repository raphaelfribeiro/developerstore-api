using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.E2E;

/// <summary>
/// End-to-end integration tests that exercise the full application flow.
/// Verifies that multiple features work together correctly: product creation,
/// cart management, sale creation and business rule enforcement (discounts).
/// </summary>
[Collection("Integration")]
public class E2EIntegrationTests : BaseIntegrationTest
{
    public E2EIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "E2E: Create product, add to cart, create sale — 10% discount applied for 5 items")]
    public async Task FullFlow_ProductCartSale_10PercentDiscountApplied()
    {
        // Authenticate once for the entire flow
        await AuthenticateClientAsync(
            $"e2e.flow{Guid.NewGuid():N}@test.com",
            "ValidPassword@123",
            $"e2eflow{Guid.NewGuid():N}");

        // Step 1 — Create a product
        var productRequest = new
        {
            title = "E2E Test Product",
            price = 100m,
            description = "Product used in end-to-end test",
            category = "e2e-test",
            image = "https://example.com/e2e.jpg",
            rating = new { rate = 4.0m, count = 25 }
        };

        var productResponse = await Client.PostAsJsonAsync("/api/products", productRequest);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = await DeserializeIdAsync(productResponse);

        // Step 2 — Create a cart with that product
        var userId = Guid.NewGuid();
        var cartRequest = new
        {
            userId,
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId, quantity = 5 }
            }
        };

        var cartResponse = await Client.PostAsJsonAsync("/api/carts", cartRequest);
        cartResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 3 — Create a sale based on the cart items (5 items → 10% discount)
        var saleRequest = new
        {
            saleNumber = $"SALE-E2E-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = userId,
            customerName = "E2E Customer",
            branchId = Guid.NewGuid(),
            branchName = "E2E Branch",
            items = new[]
            {
                new
                {
                    productId,
                    productName = "E2E Test Product",
                    quantity = 5,       // 10% discount tier (4–9 items)
                    unitPrice = 100m
                }
            }
        };

        var saleResponse = await Client.PostAsJsonAsync("/api/sales", saleRequest);
        saleResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = JsonDocument.Parse(await saleResponse.Content.ReadAsStringAsync());
        var saleData = json.RootElement.GetProperty("data");
        var item = saleData.GetProperty("items")[0];

        // Step 4 — Assert discount rules were applied correctly
        item.GetProperty("discount").GetDecimal().Should().Be(0.10m,
            because: "5 items fall in the 10% discount tier (4–9 items)");
        item.GetProperty("totalAmount").GetDecimal().Should().Be(450m,
            because: "5 items × $100 × (1 - 0.10) = $450");
        saleData.GetProperty("totalAmount").GetDecimal().Should().Be(450m);
        saleData.GetProperty("isCancelled").GetBoolean().Should().BeFalse();

        // Step 5 — Retrieve the sale and confirm persisted state
        var saleId = Guid.Parse(saleData.GetProperty("id").GetString()!);
        var getResponse = await Client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getJson = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        getJson.RootElement.GetProperty("data")
            .GetProperty("totalAmount").GetDecimal().Should().Be(450m);
    }

    [Fact(DisplayName = "E2E: Create sale with 10 items — 20% discount applied and persisted")]
    public async Task FullFlow_Sale_20PercentDiscountAppliedAndPersisted()
    {
        await AuthenticateClientAsync(
            $"e2e.disc20{Guid.NewGuid():N}@test.com",
            "ValidPassword@123",
            $"e2edisc20{Guid.NewGuid():N}");

        var saleRequest = new
        {
            saleNumber = $"SALE-E2E-20-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "E2E Bulk Customer",
            branchId = Guid.NewGuid(),
            branchName = "E2E Bulk Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Bulk Product",
                    quantity = 10,      // 20% discount tier (10–20 items)
                    unitPrice = 200m
                }
            }
        };

        var saleResponse = await Client.PostAsJsonAsync("/api/sales", saleRequest);
        saleResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = JsonDocument.Parse(await saleResponse.Content.ReadAsStringAsync());
        var item = json.RootElement.GetProperty("data").GetProperty("items")[0];

        item.GetProperty("discount").GetDecimal().Should().Be(0.20m,
            because: "10 items fall in the 20% discount tier (10–20 items)");
        item.GetProperty("totalAmount").GetDecimal().Should().Be(1600m,
            because: "10 items × $200 × (1 - 0.20) = $1600");
    }

    private static async Task<Guid> DeserializeIdAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var idStr = json.RootElement.GetProperty("data").GetProperty("id").GetString();
        return Guid.Parse(idStr!);
    }
}
