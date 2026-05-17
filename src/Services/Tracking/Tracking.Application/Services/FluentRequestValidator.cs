using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Application.Abstractions;

namespace Tracking.Application.Services;

public sealed class FluentRequestValidator(IServiceProvider serviceProvider) : IRequestValidator
{
    public async Task ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>()
            ?? throw new InvalidOperationException($"No validator registered for {typeof(TRequest).Name}");

        await validator.ValidateAndThrowAsync(request, cancellationToken);
    }
}
