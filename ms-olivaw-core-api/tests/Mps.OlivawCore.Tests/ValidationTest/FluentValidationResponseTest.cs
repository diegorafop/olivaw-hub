using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Mps.OlivawCore.Api.Validation;
using Xunit;

namespace Mps.OlivawCore.Tests.ValidationTest;

public class FluentValidationResponseTest
{
    [Theory]
    [InlineData("Registro NÃO ENCONTRADO", 404, "Objeto não encontrado")]
    [InlineData("Usuário e/ou senha são inválidos", 401, "Não autorizado")]
    [InlineData("não autorizado", 401, "Não autorizado")]
    [InlineData("já foi confirmado", 409, "Conflito")]
    [InlineData("token expirado", 410, "Expirado")]
    [InlineData("campo inválido", 400, "Operação inválida")]
    public void Constructor_should_translate_validation_messages(string message, int status, string title)
    {
        var response = new FluentValidationResponse(new ValidationException(message));
        Assert.Equal(status, response.Status);
        Assert.Equal(title, response.Title);
    }

    [Theory]
    [InlineData("não encontrado", 404)]
    [InlineData("não autorizado", 401)]
    [InlineData("inválido", 400)]
    public void OnActionExecuted_should_handle_validation_and_group_errors(string message, int status)
    {
        var context = CreateContext(new ValidationException(message,
            [new ValidationFailure("Name", "required"), new ValidationFailure("Name", "invalid")]));
        var filter = new FluentValidationExceptionFilter(NullLogger<FluentValidationExceptionFilter>.Instance);
        filter.OnActionExecuted(context);
        var result = Assert.IsAssignableFrom<ObjectResult>(context.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.True(context.ExceptionHandled);
        Assert.Equal(status, result.StatusCode);
        Assert.Equal(status, problem.Status);
        Assert.Equal(new[] { "required", "invalid" }, problem.Errors["Name"]);
        Assert.Equal(-10, filter.Order);
    }

    [Fact]
    public void OnActionExecuted_should_leave_unexpected_exceptions_to_the_host()
    {
        var exception = new InvalidOperationException("unexpected");
        var context = CreateContext(exception);
        new FluentValidationExceptionFilter(NullLogger<FluentValidationExceptionFilter>.Instance).OnActionExecuted(context);
        Assert.False(context.ExceptionHandled);
        Assert.Same(exception, context.Exception);
        Assert.Null(context.Result);
    }

    private static ActionExecutedContext CreateContext(Exception exception)
    {
        var action = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new ActionExecutedContext(action, [], new object()) { Exception = exception };
    }
}
