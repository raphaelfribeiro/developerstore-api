using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM.Services;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Infrastructure;

/// <summary>
/// Contains unit tests for the RebusEventPublisher class.
/// </summary>
public class RebusEventPublisherTests
{
    private readonly IBus _bus;
    private readonly RebusEventPublisher _publisher;

    public RebusEventPublisherTests()
    {
        _bus = Substitute.For<IBus>();
        _publisher = new RebusEventPublisher(_bus);
    }

    [Fact(DisplayName = "Given SaleCreatedEvent When publishing Then calls IBus.Publish")]
    public async Task PublishAsync_SaleCreatedEvent_CallsBusPublish()
    {
        // Given
        var @event = new SaleCreatedEvent(Guid.NewGuid(), "SALE-001", Guid.NewGuid(), 500m);

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(@event, Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Given SaleModifiedEvent When publishing Then calls IBus.Publish")]
    public async Task PublishAsync_SaleModifiedEvent_CallsBusPublish()
    {
        // Given
        var @event = new SaleModifiedEvent(Guid.NewGuid(), "SALE-001", 600m);

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(@event, Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When publishing Then calls IBus.Publish")]
    public async Task PublishAsync_SaleCancelledEvent_CallsBusPublish()
    {
        // Given
        var @event = new SaleCancelledEvent(Guid.NewGuid(), "SALE-001");

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(@event, Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When publishing Then calls IBus.Publish")]
    public async Task PublishAsync_ItemCancelledEvent_CallsBusPublish()
    {
        // Given
        var @event = new ItemCancelledEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // When
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(@event, Arg.Any<Dictionary<string, string>>());
    }

    [Fact(DisplayName = "Given event When bus throws Then exception propagates")]
    public async Task PublishAsync_BusThrows_ExceptionPropagates()
    {
        // Given
        var @event = new SaleCreatedEvent(Guid.NewGuid(), "SALE-001", Guid.NewGuid(), 500m);
        _bus.Publish(Arg.Any<object>(), Arg.Any<Dictionary<string, string>>())
            .Returns(Task.FromException(new InvalidOperationException("bus error")));

        // When
        var act = () => _publisher.PublishAsync(@event, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("bus error");
    }
}
