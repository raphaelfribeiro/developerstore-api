using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Decorator over IEventPublisher that additionally persists every domain event
/// to the MongoDB event store before delegating to the inner publisher.
///
/// If the MongoDB write fails the exception is swallowed and a warning is logged,
/// so a temporary MongoDB outage never blocks the sales flow.
/// The inner publisher (Serilog + Polly retry) is always called regardless.
/// </summary>
public sealed class MongoEventPublisher : IEventPublisher
{
    private readonly IEventPublisher _inner;
    private readonly IMongoCollection<DomainEventDocument> _collection;
    private readonly ILogger<MongoEventPublisher> _logger;

    public MongoEventPublisher(
        IEventPublisher inner,
        IMongoCollection<DomainEventDocument> collection,
        ILogger<MongoEventPublisher> logger)
    {
        _inner = inner;
        _collection = collection;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            var document = DomainEventDocument.From(@event);
            await _collection.InsertOneAsync(document, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "MongoDB event store write failed for {EventType}. Event will still be logged via Serilog.",
                typeof(TEvent).Name);
        }

        await _inner.PublishAsync(@event, cancellationToken);
    }
}
