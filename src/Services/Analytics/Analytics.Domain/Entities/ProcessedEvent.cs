namespace Analytics.Domain.Entities;

public sealed class ProcessedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTimeOffset ProcessedAt { get; set; }
}
