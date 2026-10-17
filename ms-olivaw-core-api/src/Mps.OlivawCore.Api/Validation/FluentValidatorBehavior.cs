using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Reflection;

namespace Mps.OlivawCore.Api.Validation;

public class FluentValidatorBehavior<TRequest, TResponse>
(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse> where TRequest :
    IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators = validators
        ?? throw new ArgumentNullException(nameof(validators));

    public async Task<TResponse> Handle
    (
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var failures = await GetFailures(request, cancellationToken);

        if (failures.Any())
            throw new ValidationException
            (
                "Requisição inválida.",
                failures
            );

        FluentValidatorBehavior<TRequest, TResponse>.ClearFields(request);

        return await next(cancellationToken);
    }

    private async Task<List<ValidationFailure>> GetFailures
    (
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll
        (
            validators.Select
            (
                item => item.ValidateAsync
                (
                    request,
                    cancellationToken
                )
            )
        );

        return [.. validationResults.SelectMany(item => item.Errors)];
    }

    private static void ClearFields
    (
        TRequest request
    )
    {
        PropertyInfo[] props = FluentValidatorBehavior<TRequest, TResponse>
            .GetPropertyInfo(request);

        foreach (var prop in props)
            if (prop.PropertyType == typeof(string))
            {
                object value = prop.GetValue(request);
                if (value is not null)
                    prop.SetValue(request, value?.ToString()?.Trim());
            }
    }

    private static PropertyInfo[] GetPropertyInfo
    (
        TRequest request
    )
    {
        return request
            .GetType()
            .GetProperties();
    }
}