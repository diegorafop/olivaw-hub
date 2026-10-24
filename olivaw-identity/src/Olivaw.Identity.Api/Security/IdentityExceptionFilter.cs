using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Security;

public sealed class IdentityExceptionFilter(ILogger<IdentityExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is IdentityFailure failure)
        {
            logger.LogWarning(
                "Request rejected with status {Status} for {Method} {Path}",
                failure.Status,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);
            context.Result = new ObjectResult(new ProblemDetails { Status = failure.Status, Title = failure.Message }) { StatusCode = failure.Status };
            context.ExceptionHandled = true;
            return;
        }

        var displayName = context.ActionDescriptor.DisplayName;
        if (displayName is null) return;
        if (!displayName.StartsWith("Olivaw.Identity.", StringComparison.Ordinal)) return;
        logger.LogWarning(context.Exception,
            "Identity API exception for {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);
        if (context.Exception is ValidationException validation)
        {
            var errors = validation.Errors.GroupBy(item => item.PropertyName).ToDictionary(item => item.Key, item => item.Select(error => error.ErrorMessage).ToArray());
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors) { Status = 400 });
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is DbUpdateConcurrencyException)
        {
            context.Result = new ConflictObjectResult(new ProblemDetails { Status = 409, Title = "Item alterado por outra operação. Tente novamente." });
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
