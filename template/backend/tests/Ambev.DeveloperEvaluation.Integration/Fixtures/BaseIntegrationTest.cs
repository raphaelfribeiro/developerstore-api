using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

/// <summary>
/// Base class for all integration tests.
/// Provides HttpClient, authentication helpers and JSON serialization utilities.
/// xUnit creates a new test class instance per test method, so each test
/// starts with a fresh Client and no Authorization header set.
/// </summary>
public abstract class BaseIntegrationTest
{
    private readonly IntegrationTestFactory _factory;
    protected readonly HttpClient Client;

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Creates a fresh HttpClient with no Authorization header.
    /// Use this in 401-Unauthorized tests to ensure no token is sent.
    /// </summary>
    protected HttpClient CreateUnauthenticatedClient() => _factory.CreateClient();

    /// <summary>
    /// Creates a user and authenticates, returning the JWT token.
    /// </summary>
    protected async Task<string> AuthenticateAsync(
        string email = "integration@test.com",
        string password = "ValidPassword@123",
        string username = "integrationuser")
    {
        var registerRequest = new
        {
            username,
            email,
            password,
            phone = "+5547999999999",
            role = 1,
            status = 1
        };

        await Client.PostAsJsonAsync("/api/users", registerRequest);

        var loginRequest = new { email, password };
        var response = await Client.PostAsJsonAsync("/api/auth", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var token = json.RootElement
            .GetProperty("data")
            .GetProperty("token")
            .GetString();

        return token!;
    }

    /// <summary>
    /// Sets the Authorization header with the given JWT token.
    /// </summary>
    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates and sets the Authorization header in one step.
    /// </summary>
    protected async Task AuthenticateClientAsync(
        string email = "integration@test.com",
        string password = "ValidPassword@123",
        string username = "integrationuser")
    {
        var token = await AuthenticateAsync(email, password, username);
        SetAuthToken(token);
    }

    /// <summary>
    /// Deserializes the data property from a standard ApiResponse.
    /// </summary>
    protected async Task<T?> DeserializeDataAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("data", out var data))
            return data.Deserialize<T>(JsonOptions);

        return default;
    }
}
