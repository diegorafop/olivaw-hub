using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Owh.OlivawCore.Api.Validation;

public class FluentValidationExceptionFilter(ILogger<FluentValidationExceptionFilter> logger)
    : IActionFilter, IOrderedFilter
{
    public int Order { get; set; } = -10;

    [ExcludeFromCodeCoverage]
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted
    (
        ActionExecutedContext context
    )
    {
        if (context.Exception is ValidationException exception)
        {
            logger.LogWarning(exception,
                "Handled API validation/business error for {Method} {Path}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);
            var details = GetDetails(exception);
            context.Result = GetResult(details);
            context.ExceptionHandled = true;
        }
    }

    private static IActionResult GetResult
    (
        ValidationProblemDetails details
    )
    {
        return details.Status switch
        {
            StatusCodes.Status404NotFound => new NotFoundObjectResult(details),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(details),
            _ => new BadRequestObjectResult(details),
        };
    }

    private static ValidationProblemDetails GetDetails
    (
        ValidationException exception
    )
    {
        var errors = exception
            .Errors
            .GroupBy(item => item.PropertyName, item => item.ToString())
            .ToDictionary(item => item.Key, item => item.ToArray());

        var response = new FluentValidationResponse(exception);

        return new ValidationProblemDetails(errors)
        {
            Title = response.Title,
            Status = response.Status
        };
    }
}
