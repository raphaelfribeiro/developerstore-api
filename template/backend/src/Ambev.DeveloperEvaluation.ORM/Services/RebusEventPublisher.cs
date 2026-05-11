using Ambev.DeveloperEvaluation.Domain.Events;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Implementation of <see cref="IEventPublisher"/> that publishes domain events
/// to the Rebus service bus. In production the bus is backed by RabbitMQ; in
/// development and tests it falls back to an in-memory transport.
/// Rebus handles retry, dead-letter queuing and subscriber routing automatically.
/// </summary>
public sealed class RebusEventPublisher : IEventPublisher
{
    private readonly IBus _bus;

    public RebusEventPublisher(IBus bus) => _bus = bus;

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
        => _bus.Publish(@event);
}
