using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Infrastructure;

public class MongoEventPublisherTests
{
    private readonly IEventPublisher _inner;
    private readonly IMongoCollection<DomainEventDocument> _collection;
    private readonly ILogger<MongoEventPublisher> _logger;
    private readonly MongoEventPublisher _publisher;

    public MongoEventPublisherTests()
    {
        _inner = Substitute.For<IEventPublisher>();
        _collection = Substitute.For<IMongoCollection<DomainEventDocument>>();
        _logger = Substitute.For<ILogger<MongoEventPublisher>>();
        _publisher = new MongoEventPublisher(_inner, _collection, _logger);
    }

    [Fact(DisplayName = "Given valid event When publishing Then inserts document into MongoDB")]
    public async Task PublishAsync_ValidEvent_InsertsDocumentIntoMongo()
    {
        // Given
        var @event = new SaleCreatedEvent(Guid.NewGuid(), "SALE-001", Guid.NewGuid(), 500m);

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _collection.Received(1).InsertOneAsync(
            Arg.Is<DomainEventDocument>(d =>
                d.EventType == nameof(SaleCreatedEvent) &&
                d.Payload.Contains("SALE-001")),
            Arg.Any<InsertOneOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid event When publishing Then always delegates to inner publisher")]
    public async Task PublishAsync_ValidEvent_AlwaysCallsInnerPublisher()
    {
        // Given
        var @event = new SaleCreatedEvent(Guid.NewGuid(), "SALE-001", Guid.NewGuid(), 500m);

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _inner.Received(1).PublishAsync(@event, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given MongoDB failure When publishing Then does not throw and still calls inner publisher")]
    public async Task PublishAsync_MongoFailure_DoesNotThrowAndCallsInnerPublisher()
    {
        // Given
        var @event = new SaleModifiedEvent(Guid.NewGuid(), "SALE-002", 750m);
        _collection
            .InsertOneAsync(Arg.Any<DomainEventDocument>(), Arg.Any<InsertOneOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new MongoException("Connection refused"));

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
        await _inner.Received(1).PublishAsync(@event, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given MongoDB failure When publishing Then logs a warning")]
    public async Task PublishAsync_MongoFailure_LogsWarning()
    {
        // Given
        var @event = new SaleCancelledEvent(Guid.NewGuid(), "SALE-003");
        _collection
            .InsertOneAsync(Arg.Any<DomainEventDocument>(), Arg.Any<InsertOneOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new MongoException("Timeout"));

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When publishing Then document EventType matches")]
    public async Task PublishAsync_SaleCancelledEvent_DocumentEventTypeMatchesTypeName()
    {
        // Given
        var @event = new SaleCancelledEvent(Guid.NewGuid(), "SALE-004");

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _collection.Received(1).InsertOneAsync(
            Arg.Is<DomainEventDocument>(d => d.EventType == nameof(SaleCancelledEvent)),
            Arg.Any<InsertOneOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When publishing Then document is persisted and inner called")]
    public async Task PublishAsync_ItemCancelledEvent_PersistsAndDelegates()
    {
        // Given
        var @event = new ItemCancelledEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _collection.Received(1).InsertOneAsync(
            Arg.Is<DomainEventDocument>(d => d.EventType == nameof(ItemCancelledEvent)),
            Arg.Any<InsertOneOptions?>(),
            Arg.Any<CancellationToken>());
        await _inner.Received(1).PublishAsync(@event, Arg.Any<CancellationToken>());
    }
}
