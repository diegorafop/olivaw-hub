using FluentValidation;
using Mps.OlivawCore.Api.Validation;
using System.Net;
using Xunit;

namespace Mps.OlivawCore.Tests.ValidationTest;

public class ServiceResponseValidatorTest
{
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "ignored", "unauthorized")]
    [InlineData(HttpStatusCode.NotFound, "record", "não encontrado : record")]
    [InlineData(HttpStatusCode.NotFound, "não encontrado", "não encontrado")]
    [InlineData(HttpStatusCode.InternalServerError, "", "empty")]
    [InlineData(HttpStatusCode.BadRequest, "not json", "not json")]
    [InlineData(HttpStatusCode.BadRequest, "[]", "[]")]
    [InlineData(HttpStatusCode.BadRequest, "{\"value\":1}", "value")]
    [InlineData(HttpStatusCode.BadRequest, "{\"errors\":\"invalid field\",\"status\":400}", "invalid field")]
    public async Task ValidateResponseAsync_should_expose_remote_failure(HttpStatusCode status, string content, string expected)
    {
        using var response = new HttpResponseMessage(status);
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            new ServiceResponseValidator().ValidateResponseAsync(response, content, TestContext.Current.CancellationToken));
        Assert.Contains(exception.Errors, item => item.PropertyName == "Status");
        Assert.Contains(exception.Errors, item => item.ErrorMessage.Contains(expected));
    }

    [Fact]
    public async Task ValidateResponseAsync_should_accept_success_without_errors()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        await new ServiceResponseValidator().ValidateResponseAsync(response,
            "{\"errors\":null,\"status\":200}", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidateResponseAsync_should_not_reuse_rules_from_previous_requests()
    {
        var validator = new ServiceResponseValidator();
        using var failure = new HttpResponseMessage(HttpStatusCode.BadRequest);
        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateResponseAsync(failure,
            "failed", TestContext.Current.CancellationToken));
        using var success = new HttpResponseMessage(HttpStatusCode.OK);

        await validator.ValidateResponseAsync(success, "{\"errors\":null,\"status\":200}", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidateResponseAsync_should_propagate_cancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new ServiceResponseValidator()
            .ValidateResponseAsync(response, "{\"errors\":null}", source.Token));
    }
}
