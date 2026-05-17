using Analytics.QueryApplication.Queries;

namespace Analytics.QueryApplication.Abstractions;

public interface IAnalyticsQueries
{
    Task<ProductAnalyticsDto?> GetProductAnalyticsAsync(string productId, CancellationToken cancellationToken = default);
    Task<SummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventTypeCountDto>> GetEventsByTypeAsync(CancellationToken cancellationToken = default);
    Task<ConversionRateDto> GetConversionRateAsync(CancellationToken cancellationToken = default);
}
