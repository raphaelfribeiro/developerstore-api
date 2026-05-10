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
[Collection("Integration")]
public class ProductsIntegrationTests : BaseIntegrationTest
{
    public ProductsIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "GET /api/products Without authentication Then returns 401 Unauthorized")]
    public async Task GetProducts_WithoutAuth_Returns401()
    {
        var unauthClient = CreateUnauthenticatedClient();

        var response = await unauthClient.GetAsync("/api/products?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/products When valid product Then returns 201 Created")]
    public async Task CreateProduct_ValidRequest_Returns201()
    {
        await AuthenticateClientAsync($"products.create{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"prodcreate{Guid.NewGuid():N}");

        var request = new
        {
            title = "Integration Test Product",
            price = 99.99m,
            description = "A product created during integration tests",
            category = "test",
            image = "https://example.com/image.jpg",
            rating = new { rate = 4.5m, count = 100 }
        };

        var response = await Client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("id");
    }

    [Fact(DisplayName = "GET /api/products When authenticated Then returns paginated list")]
    public async Task GetProducts_Authenticated_ReturnsPaginatedList()
    {
        await AuthenticateClientAsync($"products.get{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"prodget{Guid.NewGuid():N}");

        var response = await Client.GetAsync("/api/products?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currentPage");
        content.Should().Contain("totalCount");
    }

    [Fact(DisplayName = "GET /api/products/categories When authenticated Then returns categories")]
    public async Task GetCategories_Authenticated_ReturnsCategories()
    {
        await AuthenticateClientAsync($"categories{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"catuser{Guid.NewGuid():N}");

        var response = await Client.GetAsync("/api/products/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/products/{id} When product not found Then returns 404")]
    public async Task GetProduct_NotFound_Returns404()
    {
        await AuthenticateClientAsync($"products.notfound{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"prodnotfound{Guid.NewGuid():N}");

        var response = await Client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/products/{id} When product exists Then returns 200 with updated data")]
    public async Task UpdateProduct_ProductExists_Returns200WithUpdatedData()
    {
        await AuthenticateClientAsync($"products.update{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"produpdate{Guid.NewGuid():N}");

        var createRequest = new
        {
            title = "Original Product",
            price = 50m,
            description = "Original description",
            category = "original-category",
            image = "https://example.com/original.jpg",
            rating = new { rate = 3.0m, count = 10 }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var productId = await DeserializeIdAsync(createResponse);

        var updateRequest = new
        {
            title = "Updated Product Title",
            price = 75m,
            description = "Updated description",
            category = "updated-category",
            image = "https://example.com/updated.jpg",
            rating = new { rate = 4.5m, count = 50 }
        };

        var response = await Client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Updated Product Title");
    }

    [Fact(DisplayName = "DELETE /api/products/{id} When product exists Then returns 200 OK")]
    public async Task DeleteProduct_ProductExists_Returns200()
    {
        await AuthenticateClientAsync($"products.delete{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"proddelete{Guid.NewGuid():N}");

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

        var response = await Client.DeleteAsync($"/api/products/{productId}");

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
