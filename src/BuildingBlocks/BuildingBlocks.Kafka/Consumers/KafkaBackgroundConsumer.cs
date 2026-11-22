using System.Text;
using BuildingBlocks.Contracts.Topics;
using BuildingBlocks.Kafka.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Kafka.Consumers;

public abstract class KafkaBackgroundConsumer : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly ILogger _logger;
    private readonly string[] _topics;
    private IProducer<string, string>? _deadLetterProducer;

    protected KafkaBackgroundConsumer(
        IOptions<KafkaOptions> options,
        ILogger logger,
        params string[] topics)
    {
        _options = options.Value;
        _logger = logger;
        _topics = topics;
    }

    protected abstract Task ProcessMessageAsync(
        string topic,
        string eventType,
        string payload,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await ConsumeLoopAsync(stoppingToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _deadLetterProducer?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        using var consumer = BuildConsumer();
        consumer.Subscribe(_topics);
        _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", _topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result.Message.Value is null)
                    continue;

                var eventType = GetHeader(result.Message.Headers, "event-type")
                    ?? ExtractEventTypeFromPayload(result.Message.Value);

                var processed = await TryProcessWithRetryAsync(consumer, result, eventType, stoppingToken);
                if (!processed)
                {
                    _logger.LogError(
                        "Exhausted retries for {Topic} partition {Partition} offset {Offset}",
                        result.Topic, result.Partition, result.Offset);
                    await PublishDeadLetterAsync(result, eventType, "Max retry attempts exhausted", stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (ConsumeException ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<bool> TryProcessWithRetryAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> result,
        string eventType,
        CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                await ProcessMessageAsync(
                    result.Topic,
                    eventType,
                    result.Message.Value!,
                    stoppingToken);

                consumer.Commit(result);
                _logger.LogDebug(
                    "Processed {EventType} from {Topic} partition {Partition} offset {Offset}",
                    eventType, result.Topic, result.Partition, result.Offset);
                return true;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < _options.MaxRetryAttempts)
            {
                var delay = _options.RetryBackoffMs * attempt;
                _logger.LogWarning(ex,
                    "Transient error at {Topic}:{Offset}. Retry {Attempt}/{Max}",
                    result.Topic, result.Offset, attempt, _options.MaxRetryAttempts);
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process message at {Topic}:{Offset}. Sending to dead letter.",
                    result.Topic, result.Offset);
                await PublishDeadLetterAsync(result, eventType, ex.ToString(), stoppingToken);
                consumer.Commit(result);
                return true;
            }
        }

        return false;
    }

    private IConsumer<string, string> BuildConsumer() =>
        new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            ClientId = _options.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = _options.EnableAutoCommit,
            EnablePartitionEof = false
        }).Build();

    private async Task PublishDeadLetterAsync(
        ConsumeResult<string, string> result,
        string eventType,
        string error,
        CancellationToken cancellationToken)
    {
        try
        {
            _deadLetterProducer ??= new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = $"{_options.ClientId}-dlq"
            }).Build();

            var dlqPayload = System.Text.Json.JsonSerializer.Serialize(new DeadLetterMessage
            {
                OriginalTopic = result.Topic,
                EventType = eventType,
                Payload = result.Message.Value!,
                Error = error,
                FailedAt = DateTimeOffset.UtcNow
            });

            await _deadLetterProducer.ProduceAsync(
                KafkaTopics.AnalyticsDeadLetter,
                new Message<string, string>
                {
                    Key = result.Message.Key ?? Guid.NewGuid().ToString(),
                    Value = dlqPayload
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to publish dead letter message");
            throw;
        }
    }

    private static string? GetHeader(Headers? headers, string key)
    {
        if (headers is null) return null;
        var header = headers.FirstOrDefault(h => h.Key == key);
        return header is null ? null : Encoding.UTF8.GetString(header.GetValueBytes());
    }

    private static string ExtractEventTypeFromPayload(string payload)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(payload);
        if (doc.RootElement.TryGetProperty("eventType", out var prop))
            return prop.GetString() ?? "Unknown";
        return "Unknown";
    }

    private static bool IsTransient(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            // InvalidOperationException is usually a programming/state error, not a retryable
            // infrastructure fault — keep it out of the transient path so DLQ is reached sooner.
            if (current is TimeoutException)
                return true;

            var name = current.GetType().Name;
            if (name.Contains("DbUpdate", StringComparison.Ordinal)
                || name.Contains("Npgsql", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}

internal sealed record DeadLetterMessage
{
    public required string OriginalTopic { get; init; }
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public required string Error { get; init; }
    public DateTimeOffset FailedAt { get; init; }
}
