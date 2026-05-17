namespace Analytics.Domain.Entities;

public sealed class GlobalMetric
{
    public const string Key = "global";
    public string Id { get; set; } = Key;
    public long TotalProductViews { get; set; }
    public long TotalOrdersCompleted { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
