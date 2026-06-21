namespace Tracking.Application.Abstractions;

public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes a batch of pending outbox messages to Kafka. Returns how many were published successfully.
    /// </summary>
    Task<int> PublishPendingBatchAsync(CancellationToken cancellationToken = default);
}
