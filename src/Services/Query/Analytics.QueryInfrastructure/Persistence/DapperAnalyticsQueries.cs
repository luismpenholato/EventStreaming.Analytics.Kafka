using Analytics.Domain.Metrics;
using Analytics.QueryApplication.Abstractions;
using Analytics.QueryApplication.Queries;
using Dapper;
using Npgsql;

namespace Analytics.QueryInfrastructure.Persistence;

public sealed class DapperAnalyticsQueries(NpgsqlDataSource dataSource) : IAnalyticsQueries
{
    public async Task<ProductAnalyticsDto?> GetProductAnalyticsAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProductAnalyticsDto>(
            """
            SELECT product_id AS ProductId,
                   view_count AS ViewCount,
                   added_to_cart_count AS AddedToCartCount,
                   order_completed_count AS OrderCompletedCount
            FROM product_metrics
            WHERE product_id = @ProductId
            """,
            new { ProductId = productId });
    }

    public async Task<SummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var global = await connection.QuerySingleOrDefaultAsync<GlobalRow>(
            """
            SELECT total_product_views AS TotalProductViews,
                   total_orders_completed AS TotalOrdersCompleted
            FROM global_metrics
            WHERE id = 'global'
            """);

        var totalEvents = await connection.ExecuteScalarAsync<long>(
            "SELECT COALESCE(SUM(count), 0) FROM event_type_metrics");

        var views = global?.TotalProductViews ?? 0;
        var orders = global?.TotalOrdersCompleted ?? 0;

        return new SummaryDto(views, orders, totalEvents, ConversionRateCalculator.Calculate(views, orders));
    }

    public async Task<IReadOnlyList<EventTypeCountDto>> GetEventsByTypeAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<EventTypeCountDto>(
            """
            SELECT event_type AS EventType, count AS Count
            FROM event_type_metrics
            ORDER BY count DESC
            """);
        return rows.ToList();
    }

    public async Task<ConversionRateDto> GetConversionRateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var global = await connection.QuerySingleOrDefaultAsync<GlobalRow>(
            """
            SELECT total_product_views AS TotalProductViews,
                   total_orders_completed AS TotalOrdersCompleted
            FROM global_metrics
            WHERE id = 'global'
            """);

        var views = global?.TotalProductViews ?? 0;
        var orders = global?.TotalOrdersCompleted ?? 0;
        return new ConversionRateDto(views, orders, ConversionRateCalculator.Calculate(views, orders));
    }

    private sealed record GlobalRow(long TotalProductViews, long TotalOrdersCompleted);
}
