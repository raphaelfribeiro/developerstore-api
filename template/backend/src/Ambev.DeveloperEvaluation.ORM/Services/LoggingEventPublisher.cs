using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Implementation of IEventPublisher that logs domain events using Serilog.
/// Applies a Polly retry policy with exponential back-off to ensure reliable delivery.
/// In a production scenario, this implementation can be swapped for one that
/// publishes to a message broker (e.g. RabbitMQ via Rebus) without changing the domain.
/// </summary>
public class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Event publishing failed. Retry {RetryCount} in {Delay}s.",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogInformation(
                "[EVENT] {EventType} published at {OccurredAt} | Payload: {@Event}",
                typeof(TEvent).Name,
                DateTime.UtcNow,
                @event);

            await Task.CompletedTask;
        });
    }
}
