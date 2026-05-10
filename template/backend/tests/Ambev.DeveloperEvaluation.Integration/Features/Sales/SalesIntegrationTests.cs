using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Sales;

/// <summary>
/// Integration tests for the Sales API endpoints.
/// Tests run against a real PostgreSQL database via Testcontainers.
/// </summary>
public class SalesIntegrationTests : BaseIntegrationTest
{
    public SalesIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "POST /api/sales When valid sale Then returns 201 Created with correct discount")]
    public async Task CreateSale_ValidRequest_Returns201WithDiscount()
    {
        // Arrange
        await AuthenticateClientAsync("sales.user@test.com", "ValidPassword@123", "salesuser");

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
                    quantity = 5, // 10% discount tier
                    unitPrice = 100m
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("discount");
    }

    [Fact(DisplayName = "POST /api/sales When quantity above 20 Then returns 400 Bad Request")]
    public async Task CreateSale_QuantityAbove20_Returns400()
    {
        // Arrange
        await AuthenticateClientAsync("sales.invalid@test.com", "ValidPassword@123", "salesinvaliduser");

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
                    quantity = 21, // exceeds maximum
                    unitPrice = 100m
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/sales When sales exist Then returns paginated list")]
    public async Task GetSales_SalesExist_ReturnsPaginatedList()
    {
        // Arrange
        await AuthenticateClientAsync("sales.get@test.com", "ValidPassword@123", "salesgetuser");

        // Act
        var response = await Client.GetAsync("/api/sales?page=1&size=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currentPage");
        content.Should().Contain("totalCount");
    }

    [Fact(DisplayName = "GET /api/sales/{id} When sale not found Then returns 404")]
    public async Task GetSale_NotFound_Returns404()
    {
        // Arrange
        await AuthenticateClientAsync("sales.notfound@test.com", "ValidPassword@123", "salesnotfound");
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/sales/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/sales/{id} When sale exists Then returns 200 and cancels sale")]
    public async Task DeleteSale_SaleExists_Returns200()
    {
        // Arrange
        await AuthenticateClientAsync("sales.delete@test.com", "ValidPassword@123", "salesdeleteuser");

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
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Delete Product",
                    quantity = 3,
                    unitPrice = 50m
                }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/sales", createRequest);
        var saleId = await DeserializeIdAsync(createResponse);

        // Act
        var response = await Client.DeleteAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PATCH /api/sales/{id}/items/{itemId}/cancel When item exists Then returns 200")]
    public async Task CancelSaleItem_ItemExists_Returns200()
    {
        // Arrange
        await AuthenticateClientAsync("sales.cancel@test.com", "ValidPassword@123", "salescanceluser");

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
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Cancel Product",
                    quantity = 5,
                    unitPrice = 100m
                }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/sales", createRequest);
        var content = await createResponse.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonDocument.Parse(content);
        var saleId = Guid.Parse(json.RootElement.GetProperty("data").GetProperty("id").GetString()!);
        var itemId = Guid.Parse(json.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("id").GetString()!);

        // Act
        var response = await Client.PatchAsync($"/api/sales/{saleId}/items/{itemId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<Guid> DeserializeIdAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonDocument.Parse(content);
        var idStr = json.RootElement.GetProperty("data").GetProperty("id").GetString();
        return Guid.Parse(idStr!);
    }
}
