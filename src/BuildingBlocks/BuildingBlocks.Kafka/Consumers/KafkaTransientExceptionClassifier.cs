namespace BuildingBlocks.Kafka.Consumers;

/// <summary>
/// Classifies exceptions that are worth retrying before publishing to the DLQ.
/// </summary>
public static class KafkaTransientExceptionClassifier
{
    public static bool IsTransient(Exception ex)
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
