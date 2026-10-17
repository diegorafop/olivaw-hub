using FluentValidation;
using Moq;
using Mps.OlivawCore.Api.Services;
using Mps.OlivawCore.Api.Settings;
using Mps.OlivawCore.Api.Validation;
using Mps.OlivawCore.Requests;
using Mps.OlivawCore.Services;
using Mps.OlivawCore.Tests.TestSupport;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Mps.OlivawCore.Tests.ServicesTest;

public class ServiceHttpClientTest
{
    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Requests_should_use_named_client_preserve_base_path_and_send_json(string method)
    {
        using var factory = new HttpClientStub();
        factory.Respond = async (request, token) =>
        {
            Assert.Equal(method, request.Method.Method);
            Assert.Equal("https://service.test/api/items", request.RequestUri!.AbsoluteUri);
            if (method is "POST" or "PUT")
            {
                Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
                var payload = JObject.Parse(await request.Content.ReadAsStringAsync(token));
                Assert.Equal("Ação", payload["Name"]!.Value<string>());
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"Name\":\"response\"}") };
        };
        var service = new ServiceHttpClient(factory, Setting());
        var token = TestContext.Current.CancellationToken;
        var result = method switch
        {
            "GET" => await service.GetAsync<Payload>("/items", token),
            "POST" => await service.PostAsync<Payload>("/items", new Payload { Name = "Ação" }, token),
            "PUT" => await service.PutAsync<Payload>("/items", new Payload { Name = "Ação" }, token),
            _ => await service.DeleteAsync<Payload>("/items", token)
        };
        Assert.Equal("response", result.Name);
        Assert.Equal("catalog", Assert.Single(factory.ClientNames));
        Assert.True(service.Configured);
        Assert.Equal("https://service.test/api/", service.BaseAddress);
    }

    [Fact]
    public async Task GetAsync_should_read_strings_bytes_and_file_metadata()
    {
        using var factory = new HttpClientStub();
        factory.Respond = (request, token) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([65, 66]) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.TryAddWithoutValidation("Content-Disposition", "attachment; filename=document.pdf");
            return Task.FromResult(response);
        };
        var service = new ServiceHttpClient(factory, Setting());
        var token = TestContext.Current.CancellationToken;
        Assert.Equal("AB", await service.GetAsync<string>("items", token));
        Assert.Equal(new byte[] { 65, 66 }, await service.GetAsync<byte[]>("items", token));
        var file = await service.GetAsync<ConteudoArquivoQueryResponse>("items", token);
        Assert.Equal(new byte[] { 65, 66 }, file.Content);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("document.pdf", file.FileName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("attachment")]
    [InlineData("attachment; filename")]
    public async Task GetAsync_should_accept_files_without_optional_headers(string? disposition)
    {
        using var factory = new HttpClientStub();
        factory.Respond = (request, token) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1]) };
            if (disposition is not null)
                response.Content.Headers.TryAddWithoutValidation("Content-Disposition", disposition);
            return Task.FromResult(response);
        };
        var file = await new ServiceHttpClient(factory, Setting()).GetAsync<ConteudoArquivoQueryResponse>(
            "file", TestContext.Current.CancellationToken);
        Assert.Equal(string.Empty, file.FileName);
        Assert.Equal(string.Empty, file.ContentType);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task File_requests_should_send_multipart_bytes_and_filename(string method)
    {
        using var factory = new HttpClientStub();
        factory.Respond = async (request, token) =>
        {
            Assert.Equal(method, request.Method.Method);
            var form = Assert.IsType<MultipartFormDataContent>(request.Content);
            var part = Assert.Single(form);
            Assert.Equal("file", part.Headers.ContentDisposition!.Name!.Trim('"'));
            Assert.Equal("sample.bin", part.Headers.ContentDisposition.FileName!.Trim('"'));
            Assert.Equal(new byte[] { 1, 2 }, await part.ReadAsByteArrayAsync(token));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("uploaded") };
        };
        var service = new ServiceHttpClient(factory, Setting());
        var token = TestContext.Current.CancellationToken;
        var result = method == "POST"
            ? await service.PostFileAsync<string>("files", [1, 2], "sample.bin", token)
            : await service.PutFileAsync<string>("files", [1, 2], "sample.bin", token);
        Assert.Equal("uploaded", result);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Form_requests_should_use_overrides_and_dispose_the_stream(string method)
    {
        using var factory = new HttpClientStub();
        factory.Respond = async (request, token) =>
        {
            Assert.Equal(method, request.Method.Method);
            var part = Assert.Single(Assert.IsType<MultipartFormDataContent>(request.Content));
            Assert.Equal("form-data", await part.ReadAsStringAsync(token));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("uploaded") };
        };
        var service = new FormService(factory, Setting());
        var token = TestContext.Current.CancellationToken;
        var result = method == "POST"
            ? await service.PostFormDataAsync<string>("files", "form-data", token)
            : await service.PutFormDataAsync<string>("files", "form-data", token);
        Assert.Equal("uploaded", result);
        Assert.False(service.Stream.CanRead);
    }

    [Fact]
    public async Task GetAsync_should_authenticate_and_validate_remote_errors()
    {
        using var factory = new HttpClientStub();
        using var source = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var authentication = new Mock<IProviderAuthentication>(MockBehavior.Strict);

        authentication.Setup(item => item.AutenticarAsync(source.Token))
            .ReturnsAsync(new AuthenticationHeaderValue("Bearer", "test-only"));
        factory.Respond = (request, token) =>
        {
            Assert.Equal("Bearer test-only", request.Headers.Authorization!.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("missing") });
        };
        var service = new ServiceHttpClient(factory, Setting(), authentication.Object, new ServiceResponseValidator());
        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.GetAsync<string>("items", source.Token));
        Assert.Contains(exception.Errors, item => item.ErrorMessage.Contains("não encontrado"));
        authentication.Verify(item => item.AutenticarAsync(source.Token), Times.Once);
    }

    [Fact]
    public async Task GetAsync_should_propagate_cancellation_to_the_transport()
    {
        using var factory = new HttpClientStub();
        using var source = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        factory.Respond = (request, token) =>
        {
            source.Cancel();
            Assert.True(token.IsCancellationRequested);
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Cancellation was not propagated");
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ServiceHttpClient(factory, Setting()).GetAsync<string>("items", source.Token));
    }

    [Theory]
    [InlineData("Healthy", true)]
    [InlineData("unhealthy", false)]
    public async Task HealthAsync_should_interpret_health_response(string content, bool expected)
    {
        using var factory = new HttpClientStub();
        factory.Respond = (request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        Assert.Equal(expected, await new ServiceHttpClient(factory, Setting()).HealthAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HealthAsync_should_skip_disabled_integration()
    {
        using var factory = new HttpClientStub();
        var setting = Setting();
        setting.Configured = false;
        Assert.True(await new ServiceHttpClient(factory, setting).HealthAsync(TestContext.Current.CancellationToken));
        Assert.Empty(factory.ClientNames);
    }

    [Fact]
    public async Task GetAsync_should_propagate_invalid_json_and_transport_errors()
    {
        using var factory = new HttpClientStub();
        var service = new ServiceHttpClient(factory, Setting());
        await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(() => service.GetAsync<Payload>("items", TestContext.Current.CancellationToken));
        var expected = new HttpRequestException("unavailable");
        factory.Respond = (request, token) => Task.FromException<HttpResponseMessage>(expected);
        Assert.Same(expected, await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAsync<string>("items", TestContext.Current.CancellationToken)));
    }

    [Fact]
    public void Constructors_should_reject_missing_dependencies()
    {
        using var factory = new HttpClientStub();
        Assert.Equal("factory", Assert.Throws<ArgumentNullException>(() => new ServiceHttpClient(null!, Setting())).ParamName);
        Assert.Equal("setting", Assert.Throws<ArgumentNullException>(() => new ServiceHttpClient(factory, null!)).ParamName);
        Assert.Equal("validator", Assert.Throws<ArgumentNullException>(() => new ServiceHttpClient(factory, Setting(), null!)).ParamName);
        Assert.Equal("service", Assert.Throws<ArgumentNullException>(() => new ServiceHttpClient(factory, Setting(), null!, new ServiceResponseValidator())).ParamName);
    }

    private static SettingProviderAuthentication Setting() => new()
    {
        ServiceName = "catalog",
        Configured = true,
        BaseAddress = "https://service.test/api/"
    };

    public class Payload
    {
        public string Name { get; set; } = "";
    }

    private sealed class FormService(HttpClientStub factory, SettingProviderAuthentication setting)
        : ServiceHttpClient(factory, setting)
    {
        public MemoryStream Stream { get; } = new();
        protected override Stream GetFileStream(object content) => Stream;
        protected override MultipartFormDataContent GetMultipartFormDataContent(object content, Stream stream)
            => new() { { new StringContent((string)content), "field" } };
    }
}