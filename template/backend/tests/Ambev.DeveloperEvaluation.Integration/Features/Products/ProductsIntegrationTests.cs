using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Products;

/// <summary>
/// Integration tests for the Products API endpoints.
/// Tests run against a real PostgreSQL database via Testcontainers.
/// </summary>
public class ProductsIntegrationTests : BaseIntegrationTest
{
    public ProductsIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "POST /api/products When valid product Then returns 201 Created")]
    public async Task CreateProduct_ValidRequest_Returns201()
    {
        // Arrange
        await AuthenticateClientAsync("products.user@test.com", "ValidPassword@123", "productsuser");

        var request = new
        {
            title = "Integration Test Product",
            price = 99.99m,
            description = "A product created during integration tests",
            category = "test",
            image = "https://example.com/image.jpg",
            rating = new { rate = 4.5m, count = 100 }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("id");
    }

    [Fact(DisplayName = "GET /api/products When products exist Then returns paginated list")]
    public async Task GetProducts_ProductsExist_ReturnsPaginatedList()
    {
        // Arrange
        await AuthenticateClientAsync("products.get@test.com", "ValidPassword@123", "productsgetuser");

        // Act
        var response = await Client.GetAsync("/api/products?page=1&size=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currentPage");
        content.Should().Contain("totalCount");
    }

    [Fact(DisplayName = "GET /api/products/categories When products exist Then returns categories")]
    public async Task GetCategories_ProductsExist_ReturnsCategories()
    {
        // Arrange
        await AuthenticateClientAsync("categories.user@test.com", "ValidPassword@123", "categoriesuser");

        // Act
        var response = await Client.GetAsync("/api/products/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/products/{id} When product not found Then returns 404")]
    public async Task GetProduct_NotFound_Returns404()
    {
        // Arrange
        await AuthenticateClientAsync("products.notfound@test.com", "ValidPassword@123", "productsnotfound");
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/products/{id} When product exists Then returns 200 OK")]
    public async Task DeleteProduct_ProductExists_Returns200()
    {
        // Arrange
        await AuthenticateClientAsync("products.delete@test.com", "ValidPassword@123", "productsdeleteuser");

        var createRequest = new
        {
            title = "Product To Delete",
            price = 10m,
            description = "Will be deleted",
            category = "test",
            image = "https://example.com/img.jpg",
            rating = new { rate = 3.0m, count = 10 }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var productId = await DeserializeIdAsync(createResponse);

        // Act
        var response = await Client.DeleteAsync($"/api/products/{productId}");

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
