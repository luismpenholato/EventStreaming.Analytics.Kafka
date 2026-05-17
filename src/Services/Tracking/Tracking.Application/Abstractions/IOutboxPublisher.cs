namespace Tracking.Application.Abstractions;

public interface IOutboxPublisher
{
    /// <summary>
    /// Publica um lote de mensagens pendentes no Kafka. Retorna quantas foram publicadas com sucesso.
    /// </summary>
    Task<int> PublishPendingBatchAsync(CancellationToken cancellationToken = default);
}
