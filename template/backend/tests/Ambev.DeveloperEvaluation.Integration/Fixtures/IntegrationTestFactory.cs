using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that spins up a real PostgreSQL container
/// using Testcontainers for integration tests.
/// Implements IAsyncLifetime so the container starts before tests and stops after.
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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DefaultContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Register DefaultContext pointing to test container
            services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Apply migrations to test database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}
