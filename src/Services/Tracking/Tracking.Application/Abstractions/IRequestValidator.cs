namespace Tracking.Application.Abstractions;

public interface IRequestValidator
{
    Task ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default);
}
