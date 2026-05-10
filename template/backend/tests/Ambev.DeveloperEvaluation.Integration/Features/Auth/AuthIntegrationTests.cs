using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Auth;

/// <summary>
/// Integration tests for Auth and Users API endpoints.
/// </summary>
public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact(DisplayName = "POST /api/users When valid user Then returns 201 Created")]
    public async Task CreateUser_ValidRequest_Returns201()
    {
        // Arrange
        var request = new
        {
            username = "newtestuser",
            email = $"newuser{Guid.NewGuid():N}@test.com",
            password = "ValidPassword@123",
            phone = "+5547999999999",
            role = 1,
            status = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("name");
        content.Should().Contain("email");
    }

    [Fact(DisplayName = "POST /api/users When duplicate email Then returns 400 Bad Request")]
    public async Task CreateUser_DuplicateEmail_Returns400()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid():N}@test.com";
        var request = new
        {
            username = "duplicateuser",
            email,
            password = "ValidPassword@123",
            phone = "+5547999999999",
            role = 1,
            status = 1
        };

        await Client.PostAsJsonAsync("/api/users", request);

        // Act — second request with same email
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/auth When valid credentials Then returns token")]
    public async Task Authenticate_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = $"auth{Guid.NewGuid():N}@test.com";
        var password = "ValidPassword@123";

        var createRequest = new
        {
            username = "authuser",
            email,
            password,
            phone = "+5547999999999",
            role = 1,
            status = 1
        };
        await Client.PostAsJsonAsync("/api/users", createRequest);

        var loginRequest = new { email, password };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact(DisplayName = "POST /api/auth When invalid credentials Then returns 400 Bad Request")]
    public async Task Authenticate_InvalidCredentials_Returns400()
    {
        var loginRequest = new
        {
            email = "nonexistent@test.com",
            password = "WrongPassword"
        };

        var response = await Client.PostAsJsonAsync("/api/auth", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/users/{id} When authenticated Then returns user")]
    public async Task GetUser_Authenticated_ReturnsUser()
    {
        // Arrange
        var email = $"getuser{Guid.NewGuid():N}@test.com";
        var createRequest = new
        {
            username = "getusertest",
            email,
            password = "ValidPassword@123",
            phone = "+5547999999999",
            role = 1,
            status = 1
        };

        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var content = await createResponse.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonDocument.Parse(content);
        var userId = json.RootElement.GetProperty("data").GetProperty("id").GetString();

        var token = await AuthenticateAsync(email, "ValidPassword@123", "getusertest");
        SetAuthToken(token);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "DELETE /api/users/{id} When authenticated Then returns 200 OK")]
    public async Task DeleteUser_Authenticated_Returns200()
    {
        // Arrange
        var email = $"deleteuser{Guid.NewGuid():N}@test.com";
        var createRequest = new
        {
            username = "deleteusertest",
            email,
            password = "ValidPassword@123",
            phone = "+5547999999999",
            role = 1,
            status = 1
        };

        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var content = await createResponse.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonDocument.Parse(content);
        var userId = json.RootElement.GetProperty("data").GetProperty("id").GetString();

        var token = await AuthenticateAsync(email, "ValidPassword@123", "deleteusertest");
        SetAuthToken(token);

        // Act
        var response = await Client.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
