using FluentValidation;
using Mps.OlivawCore.Api.Services;
using Mps.OlivawCore.Api.Settings;
using Mps.OlivawCore.Api.Validation;
using Mps.OlivawCore.Tests.TestSupport;
using System.Net;
using Xunit;

namespace Mps.OlivawCore.Tests.ServicesTest;

public class AdditionalServiceHttpClientTest
{
    [Fact]
    public async Task GetAsync_should_return_error_content_when_no_validator_was_configured()
    {
        using var factory = new HttpClientStub
        {
            Respond = (request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("invalid input")
            })
        };
        var service = new ServiceHttpClient(factory, new SettingProviderAuthentication { ServiceName = "test" });
        Assert.Equal("invalid input", await service.GetAsync<string>("items", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_should_validate_error_responses_without_an_authentication_provider()
    {
        using var factory = new HttpClientStub
        {
            Respond = (request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("invalid input")
            })
        };
        var service = new ServiceHttpClient(factory, new SettingProviderAuthentication { ServiceName = "test" }, new ServiceResponseValidator());
        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.GetAsync<string>("items", TestContext.Current.CancellationToken));
        Assert.Contains(exception.Errors, item => item.ErrorMessage.Contains("invalid input"));
    }

    [Fact]
    public async Task PostFormDataAsync_should_send_empty_form_until_a_consumer_overrides_the_hooks()
    {
        using var factory = new HttpClientStub
        {
            Respond = (request, token) =>
            {
                Assert.Empty(Assert.IsType<MultipartFormDataContent>(request.Content));
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("accepted") });
            }
        };
        var service = new ServiceHttpClient(factory, new SettingProviderAuthentication { ServiceName = "test" });
        Assert.Equal("accepted", await service.PostFormDataAsync<string>("files", new object(), TestContext.Current.CancellationToken));
    }
}
