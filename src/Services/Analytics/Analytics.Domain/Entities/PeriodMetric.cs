namespace Analytics.Domain.Entities;

public sealed class PeriodMetric
{
    public DateTime PeriodStart { get; set; }
    public string EventType { get; set; } = null!;
    public long Count { get; set; }
}
