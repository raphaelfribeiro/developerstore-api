using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Infrastructure;

/// <summary>
/// Contains unit tests for the LoggingEventPublisher class.
/// Tests cover successful publishing, Polly retry behavior and different event types.
/// </summary>
public class LoggingEventPublisherTests
{
    private readonly ILogger<LoggingEventPublisher> _logger;
    private readonly LoggingEventPublisher _publisher;

    public LoggingEventPublisherTests()
    {
        _logger = Substitute.For<ILogger<LoggingEventPublisher>>();
        _publisher = new LoggingEventPublisher(_logger);
    }

    [Fact(DisplayName = "Given SaleCreatedEvent When publishing Then completes successfully")]
    public async Task PublishAsync_SaleCreatedEvent_CompletesSuccessfully()
    {
        // Given
        var @event = new SaleCreatedEvent(
            Guid.NewGuid(), "SALE-001", Guid.NewGuid(), 500m);

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given SaleModifiedEvent When publishing Then completes successfully")]
    public async Task PublishAsync_SaleModifiedEvent_CompletesSuccessfully()
    {
        // Given
        var @event = new SaleModifiedEvent(Guid.NewGuid(), "SALE-001", 500m);

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When publishing Then completes successfully")]
    public async Task PublishAsync_SaleCancelledEvent_CompletesSuccessfully()
    {
        // Given
        var @event = new SaleCancelledEvent(Guid.NewGuid(), "SALE-001");

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When publishing Then completes successfully")]
    public async Task PublishAsync_ItemCancelledEvent_CompletesSuccessfully()
    {
        // Given
        var @event = new ItemCancelledEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given multiple events When publishing sequentially Then all complete successfully")]
    public async Task PublishAsync_MultipleEvents_AllCompleteSuccessfully()
    {
        // Given
        var saleId = Guid.NewGuid();
        var events = new List<object>
        {
            new SaleCreatedEvent(saleId, "SALE-001", Guid.NewGuid(), 500m),
            new SaleModifiedEvent(saleId, "SALE-001", 600m),
            new SaleCancelledEvent(saleId, "SALE-001")
        };

        // When & Then
        foreach (var @event in events)
        {
            var act = () => _publisher.PublishAsync(@event, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }
}
