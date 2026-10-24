using Owh.Olivaw.Api.Validation;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using FluentValidation;
using MediatR;
using Owh.OlivawCore.Api.Validation;
using Olivaw.Identity.Api;

namespace Owh.Olivaw.Api.Extensions.Facades;

public sealed class TemplateValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TRequest).Assembly == typeof(IdentityRegistration).Assembly)
            return await next(cancellationToken);
        if (request is IPreserveInputRequest)
        {
            var results = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(request, cancellationToken)));
            var failures = results.SelectMany(result => result.Errors).ToArray();
            if (failures.Length > 0) throw new ValidationException(failures);
            return await next(cancellationToken);
        }
        return await new FluentValidatorBehavior<TRequest, TResponse>(validators).Handle(request, next, cancellationToken);
    }
}
