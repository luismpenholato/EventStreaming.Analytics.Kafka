namespace BuildingBlocks.Kafka.Configuration;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ClientId { get; set; } = "event-streaming-analytics";
    public string ConsumerGroupId { get; set; } = "analytics-consumer-group";
    public int RetryBackoffMs { get; set; } = 1000;
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableAutoCommit { get; set; } = false;
}
