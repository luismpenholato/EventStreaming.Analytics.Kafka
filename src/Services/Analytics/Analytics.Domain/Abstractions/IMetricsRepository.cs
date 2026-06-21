namespace Analytics.Domain.Abstractions;

public interface IMetricsRepository
{
    /// <summary>
    /// Runs processing inside a transaction.
    /// Returns false when the EventId was already processed (idempotency).
    /// </summary>
    Task<bool> ProcessIdempotentlyAsync(
        Guid eventId,
        string eventType,
        DateTimeOffset occurredAt,
        Func<CancellationToken, Task> process,
        CancellationToken cancellationToken = default);

    Task IncrementProductViewAsync(string productId, CancellationToken cancellationToken = default);
    Task IncrementAddedToCartAsync(string productId, CancellationToken cancellationToken = default);
    Task IncrementOrderCompletedAsync(IReadOnlyList<string> productIds, CancellationToken cancellationToken = default);
}
