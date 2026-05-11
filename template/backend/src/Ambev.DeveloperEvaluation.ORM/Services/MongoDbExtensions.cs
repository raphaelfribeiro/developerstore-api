using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Extension methods for registering the MongoDB domain event store.
/// Keeps all MongoDB-specific types contained within the ORM project so that
/// the IoC composition root does not need a direct reference to MongoDB.Driver.
/// </summary>
public static class MongoDbExtensions
{
    /// <summary>
    /// Registers MongoDB infrastructure and wires <see cref="MongoEventPublisher"/>
    /// as the <see cref="IEventPublisher"/> decorator when a connection string is present.
    ///
    /// Returns <c>true</c> when MongoDB is configured and the decorator is registered.
    /// Returns <c>false</c> when the connection string is absent — the caller should then
    /// register the fallback <see cref="IEventPublisher"/> implementation.
    /// </summary>
    public static bool AddMongoEventStore(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(MongoDbSettings.SectionName);
        var settings = new MongoDbSettings
        {
            ConnectionString = section["ConnectionString"] ?? string.Empty,
            DatabaseName = section["DatabaseName"] ?? "developer_evaluation",
            EventsCollectionName = section["EventsCollectionName"] ?? "domain_events"
        };

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            return false;

        builder.Services.AddSingleton<IMongoClient>(new MongoClient(settings.ConnectionString));

        builder.Services.AddSingleton<IMongoCollection<DomainEventDocument>>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client
                .GetDatabase(settings.DatabaseName)
                .GetCollection<DomainEventDocument>(settings.EventsCollectionName);
        });

        builder.Services.AddSingleton<IEventPublisher>(provider => new MongoEventPublisher(
            provider.GetRequiredService<LoggingEventPublisher>(),
            provider.GetRequiredService<IMongoCollection<DomainEventDocument>>(),
            provider.GetRequiredService<ILogger<MongoEventPublisher>>()));

        return true;
    }
}
