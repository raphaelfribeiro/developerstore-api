using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Fixtures;

[CollectionDefinition("Functional")]
public class FunctionalCollection : ICollectionFixture<FunctionalTestFactory> { }

/// <summary>
/// Custom WebApplicationFactory for functional tests.
/// Spins up a dedicated PostgreSQL container via Testcontainers, isolated
/// from the Integration test suite so both suites can run in parallel.
/// Shared across all functional test classes via the "Functional" collection.
/// </summary>
public class FunctionalTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("developer_evaluation_functional")
        .WithUsername("developer")
        .WithPassword("ev@luAt10n")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
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
