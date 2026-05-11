using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Sales;

/// <summary>
/// Integration tests for the Sales API endpoints.
/// Tests run against a real PostgreSQL database via Testcontainers.
/// </summary>
[Collection("Integration")]
public class SalesIntegrationTests : BaseIntegrationTest
{
    public SalesIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "GET /api/sales Without authentication Then returns 401 Unauthorized")]
    public async Task GetSales_WithoutAuth_Returns401()
    {
        var unauthClient = CreateUnauthenticatedClient();

        var response = await unauthClient.GetAsync("/api/sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/sales When quantity is 5 Then returns 201 with 10% discount applied")]
    public async Task CreateSale_QuantityFive_Returns201With10PercentDiscount()
    {
        await AuthenticateClientAsync($"sales.create{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salescreate{Guid.NewGuid():N}");

        var request = new
        {
            saleNumber = $"SALE-INT-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Integration Customer",
            branchId = Guid.NewGuid(),
            branchName = "Integration Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Integration Product",
                    quantity = 5,    // 10% discount tier (4–9 items)
                    unitPrice = 100m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var item = json.RootElement.GetProperty("data").GetProperty("items")[0];

        item.GetProperty("discount").GetDecimal().Should().Be(0.10m);
        item.GetProperty("totalAmount").GetDecimal().Should().Be(450m);  // 5 * 100 * 0.90
        json.RootElement.GetProperty("data").GetProperty("totalAmount").GetDecimal().Should().Be(450m);
    }

    [Fact(DisplayName = "POST /api/sales When quantity is 10 Then returns 201 with 20% discount applied")]
    public async Task CreateSale_QuantityTen_Returns201With20PercentDiscount()
    {
        await AuthenticateClientAsync($"sales.disc20{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesdisc20{Guid.NewGuid():N}");

        var request = new
        {
            saleNumber = $"SALE-20PCT-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Discount Customer",
            branchId = Guid.NewGuid(),
            branchName = "Discount Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Discount Product",
                    quantity = 10,   // 20% discount tier (10–20 items)
                    unitPrice = 100m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = json.RootElement.GetProperty("data").GetProperty("items")[0];

        item.GetProperty("discount").GetDecimal().Should().Be(0.20m);
        item.GetProperty("totalAmount").GetDecimal().Should().Be(800m);  // 10 * 100 * 0.80
    }

    [Fact(DisplayName = "POST /api/sales When quantity above 20 Then returns 400 Bad Request")]
    public async Task CreateSale_QuantityAbove20_Returns400()
    {
        await AuthenticateClientAsync($"sales.invalid{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesinvalid{Guid.NewGuid():N}");

        var request = new
        {
            saleNumber = $"SALE-INT-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Integration Customer",
            branchId = Guid.NewGuid(),
            branchName = "Integration Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Integration Product",
                    quantity = 21,
                    unitPrice = 100m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/sales When authenticated Then returns paginated list")]
    public async Task GetSales_Authenticated_ReturnsPaginatedList()
    {
        await AuthenticateClientAsync($"sales.get{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesget{Guid.NewGuid():N}");

        var response = await Client.GetAsync("/api/sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currentPage");
        content.Should().Contain("totalItems");
    }

    [Fact(DisplayName = "GET /api/sales/{id} When sale not found Then returns 404")]
    public async Task GetSale_NotFound_Returns404()
    {
        await AuthenticateClientAsync($"sales.notfound{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesnotfound{Guid.NewGuid():N}");

        var response = await Client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/sales/{id} When sale exists Then returns 200 with re-applied discount")]
    public async Task UpdateSale_SaleExists_Returns200WithReappliedDiscount()
    {
        await AuthenticateClientAsync($"sales.update{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesupdate{Guid.NewGuid():N}");

        var createRequest = new
        {
            saleNumber = $"SALE-UPD-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Update Customer",
            branchId = Guid.NewGuid(),
            branchName = "Update Branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "Original Product", quantity = 3, unitPrice = 50m }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/sales", createRequest);
        var saleId = await DeserializeIdAsync(createResponse);

        var updateRequest = new
        {
            saleNumber = $"SALE-UPD2-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Updated Customer",
            branchId = Guid.NewGuid(),
            branchName = "Updated Branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "Updated Product", quantity = 5, unitPrice = 100m }
            }
        };

        var response = await Client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = json.RootElement.GetProperty("data").GetProperty("items")[0];
        item.GetProperty("discount").GetDecimal().Should().Be(0.10m);
    }

    [Fact(DisplayName = "DELETE /api/sales/{id} When sale exists Then returns 200 and cancels sale")]
    public async Task DeleteSale_SaleExists_Returns200()
    {
        await AuthenticateClientAsync($"sales.delete{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salesdelete{Guid.NewGuid():N}");

        var createRequest = new
        {
            saleNumber = $"SALE-DEL-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Delete Customer",
            branchId = Guid.NewGuid(),
            branchName = "Delete Branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "Delete Product", quantity = 3, unitPrice = 50m }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/sales", createRequest);
        var saleId = await DeserializeIdAsync(createResponse);

        var response = await Client.DeleteAsync($"/api/sales/{saleId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PATCH /api/sales/{id}/items/{itemId}/cancel When item exists Then returns 200")]
    public async Task CancelSaleItem_ItemExists_Returns200()
    {
        await AuthenticateClientAsync($"sales.cancel{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"salescancel{Guid.NewGuid():N}");

        var createRequest = new
        {
            saleNumber = $"SALE-CANCEL-{Guid.NewGuid():N}",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Cancel Customer",
            branchId = Guid.NewGuid(),
            branchName = "Cancel Branch",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "Cancel Product", quantity = 5, unitPrice = 100m }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/sales", createRequest);
        var content = await createResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var saleId = Guid.Parse(json.RootElement.GetProperty("data").GetProperty("id").GetString()!);
        var itemId = Guid.Parse(json.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("id").GetString()!);

        var response = await Client.PatchAsync($"/api/sales/{saleId}/items/{itemId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> DeserializeIdAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var idStr = json.RootElement.GetProperty("data").GetProperty("id").GetString();
        return Guid.Parse(idStr!);
    }
}
