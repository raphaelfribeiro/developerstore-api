using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.Functional.Fixtures;

/// <summary>
/// Base class for all functional tests.
/// Provides a pre-configured HttpClient and authentication helpers.
/// xUnit creates a new test class instance per test method, so each test
/// starts with a fresh Client and no Authorization header.
/// </summary>
public abstract class BaseFunctionalTest
{
    private readonly FunctionalTestFactory _factory;
    protected readonly HttpClient Client;

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected BaseFunctionalTest(FunctionalTestFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Registers a user, authenticates, and sets the Bearer token on Client.
    /// </summary>
    protected async Task AuthenticateClientAsync(string email, string password, string username)
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

        var loginRequest = new { username, password };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var token = json.RootElement
            .GetProperty("token")
            .GetString();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!);
    }
}
