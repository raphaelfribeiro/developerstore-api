using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Features.Users;

/// <summary>
/// Integration tests for PUT /api/users/{id} ownership check
/// and PATCH /api/users/{id}/role admin-only endpoint.
/// </summary>
[Collection("Integration")]
public class UsersIntegrationTests : BaseIntegrationTest
{
    public UsersIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<(Guid Id, string Token)> CreateAndAuthenticateAsync(
        string username, string email, int role = 1)
    {
        var createRequest = new
        {
            username,
            email,
            password = "ValidPassword@123",
            phone = "+5547999999999",
            role,
            status = 1
        };

        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJson = JsonDocument.Parse(createContent);
        var userId = Guid.Parse(
            createJson.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        var loginRequest = new { username, password = "ValidPassword@123" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginJson = JsonDocument.Parse(loginContent);
        var token = loginJson.RootElement.GetProperty("token").GetString()!;

        return (userId, token);
    }

    // ── PUT ownership check ──────────────────────────────────────────────────

    [Fact(DisplayName = "PUT /api/users/{id} When owner Then returns 200 OK")]
    public async Task UpdateUser_Owner_Returns200()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (userId, token) = await CreateAndAuthenticateAsync(
            $"owner{suffix}", $"owner{suffix}@test.com");

        SetAuthToken(token);

        var updateRequest = new
        {
            username = $"owner{suffix}updated",
            email = $"owner{suffix}@test.com",
            phone = "+5547888888888",
            name = new { firstname = "Owner", lastname = "Updated" },
            address = new
            {
                city = "Sao Paulo", street = "Av Paulista",
                number = 1, zipcode = "01310-100",
                geolocation = new { lat = "-23.56", @long = "-46.65" }
            }
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{userId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT /api/users/{id} When different user (non-admin) Then returns 403 Forbidden")]
    public async Task UpdateUser_DifferentUser_Returns403()
    {
        var suffixA = Guid.NewGuid().ToString("N")[..8];
        var suffixB = Guid.NewGuid().ToString("N")[..8];

        var (userBId, _) = await CreateAndAuthenticateAsync(
            $"userb{suffixB}", $"userb{suffixB}@test.com");

        var (_, tokenA) = await CreateAndAuthenticateAsync(
            $"usera{suffixA}", $"usera{suffixA}@test.com");

        SetAuthToken(tokenA);

        var updateRequest = new
        {
            username = $"usera{suffixA}",
            email = $"usera{suffixA}@test.com"
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{userBId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PUT /api/users/{id} Without authentication Then returns 401 Unauthorized")]
    public async Task UpdateUser_NoAuth_Returns401()
    {
        var unauthClient = CreateUnauthenticatedClient();
        var response = await unauthClient.PutAsJsonAsync(
            $"/api/users/{Guid.NewGuid()}",
            new { username = "x", email = "x@x.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PUT /api/users/{id} When non-admin tries to change role Then returns 403 Forbidden")]
    public async Task UpdateUser_NonAdmin_TriesToChangeRole_Returns403()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (userId, token) = await CreateAndAuthenticateAsync(
            $"roletest{suffix}", $"roletest{suffix}@test.com", role: 1);

        SetAuthToken(token);

        var updateRequest = new
        {
            username = $"roletest{suffix}",
            email = $"roletest{suffix}@test.com",
            role = 3
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{userId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PUT /api/users/{id} When admin updates role Then returns 200 OK")]
    public async Task UpdateUser_Admin_ChangesRole_Returns200()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var adminSuffix = Guid.NewGuid().ToString("N")[..8];

        var (targetId, _) = await CreateAndAuthenticateAsync(
            $"tgt{suffix}", $"tgt{suffix}@test.com", role: 1);

        var (_, adminToken) = await CreateAndAuthenticateAsync(
            $"adm{adminSuffix}", $"adm{adminSuffix}@test.com", role: 3);

        SetAuthToken(adminToken);

        var updateRequest = new
        {
            username = $"tgt{suffix}",
            email = $"tgt{suffix}@test.com",
            role = 2,
            status = 1
        };

        var response = await Client.PutAsJsonAsync($"/api/users/{targetId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── PATCH /api/users/{id}/role ───────────────────────────────────────────

    [Fact(DisplayName = "PATCH /api/users/{id}/role When admin Then returns 200 OK")]
    public async Task PatchUserRole_Admin_Returns200()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var adminSuffix = Guid.NewGuid().ToString("N")[..8];

        var (targetId, _) = await CreateAndAuthenticateAsync(
            $"target{suffix}", $"target{suffix}@test.com", role: 1);

        var (_, adminToken) = await CreateAndAuthenticateAsync(
            $"admin{adminSuffix}", $"admin{adminSuffix}@test.com", role: 3);

        SetAuthToken(adminToken);

        var patchRequest = new { role = 2, status = 1 };
        var response = await Client.PatchAsJsonAsync($"/api/users/{targetId}/role", patchRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Manager");
    }

    [Fact(DisplayName = "PATCH /api/users/{id}/role When non-admin Then returns 403 Forbidden")]
    public async Task PatchUserRole_NonAdmin_Returns403()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var targetSuffix = Guid.NewGuid().ToString("N")[..8];

        var (targetId, _) = await CreateAndAuthenticateAsync(
            $"target2{targetSuffix}", $"target2{targetSuffix}@test.com");

        var (_, customerToken) = await CreateAndAuthenticateAsync(
            $"cust{suffix}", $"cust{suffix}@test.com", role: 1);

        SetAuthToken(customerToken);

        var patchRequest = new { role = 3, status = 1 };
        var response = await Client.PatchAsJsonAsync($"/api/users/{targetId}/role", patchRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PATCH /api/users/{id}/role Without authentication Then returns 401 Unauthorized")]
    public async Task PatchUserRole_NoAuth_Returns401()
    {
        var unauthClient = CreateUnauthenticatedClient();
        var response = await unauthClient.PatchAsJsonAsync(
            $"/api/users/{Guid.NewGuid()}/role",
            new { role = 3, status = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PATCH /api/users/{id}/role When non-existent user Then returns 404 Not Found")]
    public async Task PatchUserRole_NonExistentUser_Returns404()
    {
        var adminSuffix = Guid.NewGuid().ToString("N")[..8];
        var (_, adminToken) = await CreateAndAuthenticateAsync(
            $"admin2{adminSuffix}", $"admin2{adminSuffix}@test.com", role: 3);

        SetAuthToken(adminToken);

        var response = await Client.PatchAsJsonAsync(
            $"/api/users/{Guid.NewGuid()}/role",
            new { role = 3, status = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
