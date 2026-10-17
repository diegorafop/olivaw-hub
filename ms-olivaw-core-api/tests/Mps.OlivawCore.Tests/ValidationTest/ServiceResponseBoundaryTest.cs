using FluentValidation;
using Mps.OlivawCore.Api.Validation;
using System.Net;
using Xunit;

namespace Mps.OlivawCore.Tests.ValidationTest;

public class ServiceResponseBoundaryTest
{
    [Fact]
    public async Task ValidateAsync_should_preserve_acceptance_of_non_textual_error_details()
    {
        var response = new ValidationErrorResponse { Status = HttpStatusCode.OK, Errors = new { Code = 42 } };
        var result = await new ServiceResponseValidator().ValidateAsync(response, TestContext.Current.CancellationToken);
        Assert.True(result.IsValid);
    }
    [Theory]
    [InlineData(199, false)]
    [InlineData(200, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    public async Task ValidateResponseAsync_should_use_transport_status_when_payload_omits_it(int status, bool valid)
    {
        using var response = new HttpResponseMessage((HttpStatusCode)status);
        var validator = new ServiceResponseValidator();
        var token = TestContext.Current.CancellationToken;
        if (valid)
            await validator.ValidateResponseAsync(response, "{\"errors\":null}", token);
        else
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                validator.ValidateResponseAsync(response, "{\"errors\":null}", token));
            Assert.Equal("Status", Assert.Single(exception.Errors).PropertyName);
        }
    }

    [Fact]
    public async Task ValidateResponseAsync_should_not_accept_a_success_status_in_an_error_payload()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            new ServiceResponseValidator().ValidateResponseAsync(response,
                "{\"errors\":null,\"status\":200}", TestContext.Current.CancellationToken));
        Assert.Equal("Status", Assert.Single(exception.Errors).PropertyName);
    }

    [Fact]
    public async Task ValidateAsync_should_validate_native_error_arrays_without_accumulating_rules()
    {
        var validator = new ServiceResponseValidator();
        var response = new ValidationErrorResponse { Status = HttpStatusCode.OK, Errors = new[] { "first", "second" } };
        var result = await validator.ValidateAsync(response, TestContext.Current.CancellationToken);
        Assert.Equal("Errors", Assert.Single(result.Errors).PropertyName);
        response.Errors = Array.Empty<string>();
        Assert.True((await validator.ValidateAsync(response, TestContext.Current.CancellationToken)).IsValid);
    }

    [Fact]
    public async Task ValidateResponseAsync_should_reject_json_error_arrays_on_success_status()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => new ServiceResponseValidator()
            .ValidateResponseAsync(response, "{\"errors\":[\"invalid\"],\"status\":200}", TestContext.Current.CancellationToken));
        Assert.Equal("Errors", Assert.Single(exception.Errors).PropertyName);
    }

    [Fact]
    public async Task ValidateResponseAsync_should_use_current_data_on_every_call()
    {
        var validator = new ServiceResponseValidator();
        foreach (var content in new[] { "first error", "second error" })
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                validator.ValidateResponseAsync(response, content, TestContext.Current.CancellationToken));
            Assert.Equal(2, exception.Errors.Count());
            Assert.Single(exception.Errors, item => item.PropertyName == "Status");
            Assert.Contains(content, Assert.Single(exception.Errors, item => item.PropertyName == "Errors").ErrorMessage);
        }
    }
}