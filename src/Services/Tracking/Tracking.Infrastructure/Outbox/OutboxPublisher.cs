using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Serialization;
using BuildingBlocks.Kafka.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Tracking.Application.Abstractions;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Outbox;

public sealed class OutboxPublisher(
    TrackingDbContext db,
    IIntegrationEventPublisher publisher,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<OutboxPublisher> logger) : IOutboxPublisher
{
    private const int BatchSize = 20;
    private static readonly TimeSpan LockStaleAfter = TimeSpan.FromMinutes(5);

    public async Task<int> PublishPendingBatchAsync(CancellationToken cancellationToken = default)
    {
        var maxRetries = kafkaOptions.Value.MaxRetryAttempts;
        var claimed = await ClaimPendingBatchAsync(maxRetries, cancellationToken);
        if (claimed.Count == 0)
            return 0;

        var published = 0;
        foreach (var message in claimed)
        {
            using (LogContext.PushProperty("CorrelationId", message.CorrelationId))
            using (LogContext.PushProperty("EventId", message.EventId))
            using (LogContext.PushProperty("EventType", message.EventType))
            using (LogContext.PushProperty("Topic", message.Topic))
            {
                try
                {
                    var @event = IntegrationEventRegistry.Deserialize(message.EventType, message.Payload);
                    await PublishAsync(publisher, message.Topic, @event, cancellationToken);

                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.LockedAt = null;
                    message.LastError = null;
                    published++;
                    logger.LogInformation("Outbox message {OutboxId} published to {Topic}", message.Id, message.Topic);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.LockedAt = null;
                    message.LastError = ex.Message;
                    logger.LogWarning(ex,
                        "Outbox publish failed for {OutboxId}. Retry {RetryCount}/{MaxRetries}",
                        message.Id, message.RetryCount, maxRetries);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return published;
    }

    private async Task<List<OutboxMessage>> ClaimPendingBatchAsync(
        int maxRetries,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var staleBefore = now - LockStaleAfter;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // Concurrent workers must not claim the same row: lock with SKIP LOCKED and stamp LockedAt.
        var claimed = await db.OutboxMessages
            .FromSql($"""
                SELECT *
                FROM outbox_messages
                WHERE "ProcessedAt" IS NULL
                  AND "RetryCount" < {maxRetries}
                  AND ("LockedAt" IS NULL OR "LockedAt" < {staleBefore})
                ORDER BY "CreatedAt"
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);

        foreach (var message in claimed)
            message.LockedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return claimed;
    }

    private static Task PublishAsync(
        IIntegrationEventPublisher publisher,
        string topic,
        IntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var method = typeof(IIntegrationEventPublisher)
            .GetMethod(nameof(IIntegrationEventPublisher.PublishAsync))!
            .MakeGenericMethod(@event.GetType());

        return (Task)method.Invoke(publisher, [topic, @event, cancellationToken])!;
    }
}
