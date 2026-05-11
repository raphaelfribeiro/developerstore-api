namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Abstraction for publishing domain events.
/// Implementations may log events, publish to a message broker,
/// or use any other delivery mechanism.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
