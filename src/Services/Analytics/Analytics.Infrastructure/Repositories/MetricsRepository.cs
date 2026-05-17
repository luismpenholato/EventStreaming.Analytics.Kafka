using Analytics.Domain.Abstractions;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Analytics.Infrastructure.Repositories;

public sealed class MetricsRepository(AnalyticsDbContext db) : IMetricsRepository
{
    public async Task<bool> ProcessIdempotentlyAsync(
        Guid eventId,
        string eventType,
        DateTimeOffset occurredAt,
        Func<CancellationToken, Task> process,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            db.ProcessedEvents.Add(new ProcessedEvent
            {
                EventId = eventId,
                EventType = eventType,
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await process(cancellationToken);
        await IncrementEventTypeInternalAsync(eventType, occurredAt, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task IncrementProductViewAsync(string productId, CancellationToken cancellationToken = default)
    {
        await UpsertProductMetricAsync(productId, m => m.ViewCount++, cancellationToken);
        await IncrementGlobalViewsAsync(cancellationToken);
    }

    public async Task IncrementAddedToCartAsync(string productId, CancellationToken cancellationToken = default) =>
        await UpsertProductMetricAsync(productId, m => m.AddedToCartCount++, cancellationToken);

    public async Task IncrementOrderCompletedAsync(IReadOnlyList<string> productIds, CancellationToken cancellationToken = default)
    {
        foreach (var productId in productIds.Distinct())
            await UpsertProductMetricAsync(productId, m => m.OrderCompletedCount++, cancellationToken);

        var global = await GetOrCreateGlobalAsync(cancellationToken);
        global.TotalOrdersCompleted++;
        global.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task UpsertProductMetricAsync(
        string productId,
        Action<ProductMetric> increment,
        CancellationToken cancellationToken)
    {
        var metric = await db.ProductMetrics.FindAsync([productId], cancellationToken);
        if (metric is null)
        {
            metric = new ProductMetric { ProductId = productId, UpdatedAt = DateTimeOffset.UtcNow };
            increment(metric);
            db.ProductMetrics.Add(metric);
        }
        else
        {
            increment(metric);
            metric.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private async Task IncrementEventTypeInternalAsync(
        string eventType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var typeMetric = await db.EventTypeMetrics.FindAsync([eventType], cancellationToken);
        if (typeMetric is null)
        {
            db.EventTypeMetrics.Add(new EventTypeMetric
            {
                EventType = eventType,
                Count = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            typeMetric.Count++;
            typeMetric.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var periodStart = new DateTime(
            occurredAt.Year, occurredAt.Month, occurredAt.Day, occurredAt.Hour, 0, 0, DateTimeKind.Utc);

        var periodMetric = await db.PeriodMetrics.FindAsync([periodStart, eventType], cancellationToken);
        if (periodMetric is null)
        {
            db.PeriodMetrics.Add(new PeriodMetric
            {
                PeriodStart = periodStart,
                EventType = eventType,
                Count = 1
            });
        }
        else
        {
            periodMetric.Count++;
        }
    }

    private async Task IncrementGlobalViewsAsync(CancellationToken cancellationToken)
    {
        var global = await GetOrCreateGlobalAsync(cancellationToken);
        global.TotalProductViews++;
        global.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task<GlobalMetric> GetOrCreateGlobalAsync(CancellationToken cancellationToken)
    {
        var global = await db.GlobalMetrics.FindAsync([GlobalMetric.Key], cancellationToken);
        if (global is not null) return global;

        global = new GlobalMetric { Id = GlobalMetric.Key, UpdatedAt = DateTimeOffset.UtcNow };
        db.GlobalMetrics.Add(global);
        return global;
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
