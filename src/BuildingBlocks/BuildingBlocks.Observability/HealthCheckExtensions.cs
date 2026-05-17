using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Observability;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddKafkaHealthCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string name = "kafka")
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new KafkaHealthCheck(bootstrapServers),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "kafka"]));
    }
}

internal sealed class KafkaHealthCheck(string bootstrapServers) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

            var metadata = admin.GetMetadata(TimeSpan.FromSeconds(5));
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Kafka reachable. Brokers: {metadata.Brokers.Count}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka unreachable", ex));
        }
    }
}
