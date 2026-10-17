using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Security;

public sealed class IdentityExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var displayName = context.ActionDescriptor.DisplayName;
        if (displayName is null) return;
        if (!displayName.StartsWith("Olivaw.Identity.", StringComparison.Ordinal)) return;
        if (context.Exception is IdentityFailure failure)
        {
            context.Result = new ObjectResult(new ProblemDetails { Status = failure.Status, Title = failure.Message }) { StatusCode = failure.Status };
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is ValidationException validation)
        {
            var errors = validation.Errors.GroupBy(item => item.PropertyName).ToDictionary(item => item.Key, item => item.Select(error => error.ErrorMessage).ToArray());
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors) { Status = 400 });
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is DbUpdateConcurrencyException)
        {
            context.Result = new ConflictObjectResult(new ProblemDetails { Status = 409, Title = "Registro alterado por outra operação. Tente novamente." });
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is DbUpdateException)
        {
            context.Result = new ConflictObjectResult(new ProblemDetails { Status = 409, Title = "Não foi possível persistir a operação. Confira os vínculos e identificadores." });
            context.ExceptionHandled = true;
        }
    }
}
