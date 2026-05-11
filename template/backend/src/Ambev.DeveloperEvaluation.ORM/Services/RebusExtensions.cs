using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM.Services.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Extension methods to configure Rebus messaging.
/// Uses RabbitMQ when <c>RabbitMq:ConnectionString</c> is present in configuration;
/// falls back to an in-memory transport for local development and tests.
/// The bus is managed automatically by the .NET generic host (IHostedService).
/// </summary>
public static class RebusExtensions
{
    private const string QueueName = "developer-evaluation";

    /// <summary>
    /// Registers Rebus, all <c>IHandleMessages&lt;T&gt;</c> handlers, and subscribes to all domain events.
    /// </summary>
    public static void AddRebusMessaging(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration["RabbitMq:ConnectionString"];
        var network = new InMemNetwork();

        builder.Services.AddRebus(
            configure =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                    return configure.Transport(t => t.UseRabbitMq(connectionString, QueueName));

                return configure.Transport(t => t.UseInMemoryTransport(network, QueueName));
            },
            onCreated: async bus =>
            {
                await bus.Subscribe<SaleCreatedEvent>();
                await bus.Subscribe<SaleModifiedEvent>();
                await bus.Subscribe<SaleCancelledEvent>();
                await bus.Subscribe<ItemCancelledEvent>();
            });

        builder.Services.AutoRegisterHandlersFromAssemblyOf<SaleCreatedEventHandler>();
    }
}
