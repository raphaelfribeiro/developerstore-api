using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory> { }

/// <summary>
/// Custom WebApplicationFactory that spins up a real PostgreSQL container
/// using Testcontainers for integration tests.
/// Shared across all test classes via the "Integration" collection fixture,
/// so only one container starts for the entire integration test suite.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("developer_evaluation_test")
        .WithUsername("developer")
        .WithPassword("ev@luAt10n")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        // Services triggers the host build with ConfigureWebHost already applied,
        // so the DbContext points to the test container at migration time.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}
