namespace Analytics.QueryApplication.Queries;

public sealed record ProductAnalyticsDto(
    string ProductId,
    long ViewCount,
    long AddedToCartCount,
    long OrderCompletedCount);

public sealed record SummaryDto(
    long TotalProductViews,
    long TotalOrdersCompleted,
    long TotalEvents,
    decimal ConversionRate);

public sealed record EventTypeCountDto(string EventType, long Count);

public sealed record ConversionRateDto(
    long TotalViews,
    long TotalOrders,
    decimal ConversionRate);
