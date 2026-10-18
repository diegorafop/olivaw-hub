using FluentValidation;
using MediatR;
using Owh.OlivawCore.Api.Validation;
using Olivaw.Identity.Api;

namespace Owh.Ltmj.Api.Extensions.Facades;

public sealed class TemplateValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TRequest).Assembly == typeof(IdentityRegistration).Assembly)
            return next(cancellationToken);
        return new FluentValidatorBehavior<TRequest, TResponse>(validators).Handle(request, next, cancellationToken);
    }
}
