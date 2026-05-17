namespace Analytics.Domain.Entities;

public sealed class EventTypeMetric
{
    public string EventType { get; set; } = null!;
    public long Count { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
