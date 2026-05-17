namespace Analytics.Domain.Entities;

public sealed class ProductMetric
{
    public string ProductId { get; set; } = null!;
    public long ViewCount { get; set; }
    public long AddedToCartCount { get; set; }
    public long OrderCompletedCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
