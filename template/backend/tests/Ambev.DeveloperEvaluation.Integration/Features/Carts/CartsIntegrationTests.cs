using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Carts;

/// <summary>
/// Integration tests for the Carts API endpoints.
/// Tests run against a real PostgreSQL database via Testcontainers.
/// </summary>
[Collection("Integration")]
public class CartsIntegrationTests : BaseIntegrationTest
{
    public CartsIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "GET /api/carts Without authentication Then returns 401 Unauthorized")]
    public async Task GetCarts_WithoutAuth_Returns401()
    {
        var unauthClient = CreateUnauthenticatedClient();

        var response = await unauthClient.GetAsync("/api/carts?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST /api/carts When valid cart Then returns 201 Created")]
    public async Task CreateCart_ValidRequest_Returns201()
    {
        await AuthenticateClientAsync($"carts.create{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartcreate{Guid.NewGuid():N}");

        var request = new
        {
            userId = Guid.NewGuid(),
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 3 }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/carts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("id");
        content.Should().Contain("userId");
    }

    [Fact(DisplayName = "GET /api/carts/{id} When cart exists Then returns 200 with cart data")]
    public async Task GetCart_CartExists_Returns200()
    {
        await AuthenticateClientAsync($"carts.get{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartget{Guid.NewGuid():N}");

        var userId = Guid.NewGuid();
        var createRequest = new
        {
            userId,
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 2 }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/carts", createRequest);
        var cartId = await DeserializeIdAsync(createResponse);

        var response = await Client.GetAsync($"/api/carts/{cartId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("userId");
        content.Should().Contain("products");
    }

    [Fact(DisplayName = "GET /api/carts/{id} When cart not found Then returns 404")]
    public async Task GetCart_NotFound_Returns404()
    {
        await AuthenticateClientAsync($"carts.notfound{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartnotfound{Guid.NewGuid():N}");

        var response = await Client.GetAsync($"/api/carts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/carts When authenticated Then returns paginated list")]
    public async Task GetCarts_Authenticated_ReturnsPaginatedList()
    {
        await AuthenticateClientAsync($"carts.list{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartlist{Guid.NewGuid():N}");

        var response = await Client.GetAsync("/api/carts?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currentPage");
        content.Should().Contain("totalItems");
    }

    [Fact(DisplayName = "PUT /api/carts/{id} When cart exists Then returns 200 with updated items")]
    public async Task UpdateCart_CartExists_Returns200WithUpdatedItems()
    {
        await AuthenticateClientAsync($"carts.update{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartupdate{Guid.NewGuid():N}");

        var userId = Guid.NewGuid();
        var createRequest = new
        {
            userId,
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 1 }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/carts", createRequest);
        var cartId = await DeserializeIdAsync(createResponse);

        var newProductId = Guid.NewGuid();
        var updateRequest = new
        {
            userId,
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId = newProductId, quantity = 5 }
            }
        };

        var response = await Client.PutAsJsonAsync($"/api/carts/{cartId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var products = json.RootElement.GetProperty("data").GetProperty("products");
        products.GetArrayLength().Should().Be(1);
        products[0].GetProperty("quantity").GetInt32().Should().Be(5);
    }

    [Fact(DisplayName = "DELETE /api/carts/{id} When cart exists Then returns 200 OK")]
    public async Task DeleteCart_CartExists_Returns200()
    {
        await AuthenticateClientAsync($"carts.delete{Guid.NewGuid():N}@test.com", "ValidPassword@123", $"cartdelete{Guid.NewGuid():N}");

        var createRequest = new
        {
            userId = Guid.NewGuid(),
            date = DateTime.UtcNow,
            products = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 2 }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/carts", createRequest);
        var cartId = await DeserializeIdAsync(createResponse);

        var response = await Client.DeleteAsync($"/api/carts/{cartId}");

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
