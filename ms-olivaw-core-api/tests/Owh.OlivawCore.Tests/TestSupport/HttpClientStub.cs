using System.Net;

namespace Owh.OlivawCore.Tests.TestSupport;

internal sealed class HttpClientStub : HttpMessageHandler, IHttpClientFactory
{
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond { get; set; }
        = (request, token) => Task.FromResult
    (
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok")
        }
    );

    public List<string> ClientNames { get; } = [];
    public int Requests { get; private set; }
    public Uri BaseAddress { get; set; } = new("https://service.test/api/");

    public HttpClient CreateClient(string name)
    {
        ClientNames.Add(name);
        return new HttpClient(this, disposeHandler: false) { BaseAddress = BaseAddress };
    }

    protected override Task<HttpResponseMessage> SendAsync
    (
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Requests++;
        return Respond(request, cancellationToken);
    }
}