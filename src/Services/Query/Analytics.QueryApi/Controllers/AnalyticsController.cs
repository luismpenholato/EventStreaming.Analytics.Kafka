using Analytics.QueryApplication.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.QueryApi.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController(IAnalyticsQueries queries) : ControllerBase
{
    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProduct(string productId, CancellationToken cancellationToken)
    {
        var result = await queries.GetProductAnalyticsAsync(productId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(await queries.GetSummaryAsync(cancellationToken));

    [HttpGet("events-by-type")]
    public async Task<IActionResult> GetEventsByType(CancellationToken cancellationToken) =>
        Ok(await queries.GetEventsByTypeAsync(cancellationToken));

    [HttpGet("conversion-rate")]
    public async Task<IActionResult> GetConversionRate(CancellationToken cancellationToken) =>
        Ok(await queries.GetConversionRateAsync(cancellationToken));
}
