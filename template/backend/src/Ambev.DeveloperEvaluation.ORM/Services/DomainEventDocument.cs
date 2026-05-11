using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// MongoDB document that represents a persisted domain event.
/// Stored in the configured events collection (default: "domain_events").
/// </summary>
[BsonIgnoreExtraElements]
public sealed class DomainEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; } = Guid.NewGuid();

    public string EventType { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public static DomainEventDocument From<TEvent>(TEvent @event) where TEvent : class
        => new()
        {
            EventType = typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(@event),
            OccurredAt = DateTime.UtcNow
        };
}
