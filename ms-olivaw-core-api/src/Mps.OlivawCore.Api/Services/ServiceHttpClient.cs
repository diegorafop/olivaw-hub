using Mps.OlivawCore.Api.Extensions.Https;
using Mps.OlivawCore.Api.Extensions.Serializations;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Api.Validation;
using Mps.OlivawCore.Requests;
using Mps.OlivawCore.Services;
using Mps.OlivawCore.Settings;
using System.Net.Http.Headers;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Mps.OlivawCore.Api.Services;

public class ServiceHttpClient : IServiceHttpClient
{
    protected readonly ISetting _setting;
    private readonly IHttpClientFactory _factory;
    private readonly IProviderAuthentication? _service;
    private readonly ServiceResponseValidator? _validator;

    public ServiceHttpClient
    (
        IHttpClientFactory factory,
        ISetting setting
    )
    {
        _setting = setting
            ?? throw new ArgumentNullException(nameof(setting));

        _factory = factory
            ?? throw new ArgumentNullException(nameof(factory));

        _service = null;
        _validator = null;
    }

    public ServiceHttpClient
    (
        IHttpClientFactory factory,
        ISetting setting,
        ServiceResponseValidator validator
    )
    {
        _setting = setting
            ?? throw new ArgumentNullException(nameof(setting));

        _factory = factory
            ?? throw new ArgumentNullException(nameof(factory));

        _validator = validator
            ?? throw new ArgumentNullException(nameof(validator));

        _service = null;
    }

    public ServiceHttpClient
    (
        IHttpClientFactory factory,
        ISetting setting,
        IProviderAuthentication service,
        ServiceResponseValidator validator
    )
    {
        _factory = factory
            ?? throw new ArgumentNullException(nameof(factory));

        _setting = setting
            ?? throw new ArgumentNullException(nameof(setting));

        _service = service
            ?? throw new ArgumentNullException(nameof(service));

        _validator = validator
            ?? throw new ArgumentNullException(nameof(validator));
    }

    public string BaseAddress => _setting.BaseAddress;

    public bool Configured => _setting.Configured;

    public virtual async Task<bool> HealthAsync
    (

        CancellationToken cancellationToken
    )
    {
        if (!_setting.Configured)
            return true;

        var response = await GetAsync<string>
        (
            "health",
            cancellationToken
        );

        return response
            .ToLower()
            .Equals("healthy");
    }

    public async Task<T> GetAsync<T>
    (
        string route,
        CancellationToken cancellationToken
    ) where T : class
    {
        using HttpClient httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.GetAsync
        (
            httpClient.GetRoute(route),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PostAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PostAsync
        (
            httpClient.GetRoute(route),
            GetStringContent(content),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PutAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PutAsync
        (
            httpClient.GetRoute(route),
            GetStringContent(content),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PostFormDataAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var fileStream = GetFileStream(content);
        using var formData = GetMultipartFormDataContent(content, fileStream);

        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PostAsync
        (
            route,
            formData,
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PutFormDataAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var fileStream = GetFileStream(content);
        using var formData = GetMultipartFormDataContent(content, fileStream);

        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PutAsync
        (
            httpClient.GetRoute(route),
            formData,
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> DeleteAsync<T>
    (
        string route,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.DeleteAsync
        (
            httpClient.GetRoute(route),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PostFileAsync<T>
    (
        string route,
        byte[] content,
        string fileName,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PostAsync
        (
            httpClient.GetRoute(route),
            GetContent(content, fileName),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    public async Task<T> PutFileAsync<T>
    (
        string route,
        byte[] content,
        string fileName,
        CancellationToken cancellationToken
    ) where T : class
    {
        using var httpClient = await GetHttpClient(cancellationToken);

        var response = await httpClient.PutAsync
        (
            httpClient.GetRoute(route),
            GetContent(content, fileName),
            cancellationToken
        );

        return await ReadAsync<T>
        (
            response,
            cancellationToken
        );
    }

    protected async Task<T> ReadAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    ) where T : class
    {
        await ValidateResponseAsync
        (
            response,
            cancellationToken
        );

        return await ReadByTypeAsync<T>
        (
            response,
            cancellationToken
        );
    }

    private static async Task<T> ReadByTypeAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    ) where T : class
    {
        var typeT = typeof(T);

        return typeT switch
        {
            _ when typeof(ConteudoArquivoQueryResponse).IsAssignableFrom(typeT)
                => await ReadAsFileAsync<T>(response, cancellationToken),

            _ when typeT == typeof(byte[])
                => await ReadAsByteArrayAsync<T>(response, cancellationToken),

            _ when typeT == typeof(string)
                => await ReadAsStringAsync<T>(response, cancellationToken),

            _ => await ReadAsJsonAsync<T>(response, cancellationToken),
        };
    }

    private static async Task<T> ReadAsJsonAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    ) where T : class
    {
        var content = await response
            .Content
            .ReadAsStringAsync(cancellationToken);

        return content.Deserialize<T>();
    }

    private static async Task<T> ReadAsStringAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var stringContent = await response
            .Content
            .ReadAsStringAsync(cancellationToken);

        return (T)Convert.ChangeType
        (
            stringContent,
            typeof(T)
        );
    }

    private static async Task<T> ReadAsFileAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var byteContent = await ReadAsByteArrayAsync<byte[]>
        (
            response,
            cancellationToken
        );

        var fileResponse = Activator.CreateInstance<T>() as ConteudoArquivoQueryResponse
            ?? throw new InvalidCastException(MessageHelper.InvalidCastForFile<T>());

        fileResponse.FileName = GetFileName(response);
        fileResponse.ContentType = GetContentType(response);
        fileResponse.Content = byteContent;

        return (T)(object)fileResponse;
    }

    private static async Task<T> ReadAsByteArrayAsync<T>
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var byteContent = await response
            .Content
            .ReadAsByteArrayAsync(cancellationToken);

        return (T)Convert.ChangeType
        (
            byteContent,
            typeof(T)
        );
    }

    private async Task ValidateResponseAsync
    (
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response
                .Content
                .ReadAsStringAsync(cancellationToken);

            if (_validator is not null)
                await _validator.ValidateResponseAsync
                (
                    response,
                    content,
                    cancellationToken
                );
        }
    }

    private static StringContent GetStringContent
    (
        object content
    )
    {
        var stringContent = new StringContent
        (
            content.Serialize(),
            Encoding.UTF8,
            Application.Json
        );

        stringContent
            .Headers
            .ContentType = GetMediaTypeHeaderValue();

        return stringContent;
    }

    private static MediaTypeHeaderValue GetMediaTypeHeaderValue()
    {
        return new MediaTypeHeaderValue("application/json");
    }

    protected virtual Stream GetFileStream
    (
        object content
    )
    {
        return new MemoryStream();
    }

    protected virtual MultipartFormDataContent GetMultipartFormDataContent
    (
        object content,
        Stream stream
    )
    {
        return [];
    }

    private static MultipartFormDataContent GetContent
    (
        byte[] content,
        string fileName
    )
    {
        return new MultipartFormDataContent()
        {
            {
                new ByteArrayContent(content, 0, content.Length),
                "file",
                fileName
            }
        };
    }

    private static string GetContentType
    (
        HttpResponseMessage response
    )
    {
        string contentType = string.Empty;

        if (response
            .Content
            .Headers
            .TryGetValues("Content-Type", out IEnumerable<string> values)
        )
        {
            var value = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
                contentType = value.Trim();
        }

        return contentType;
    }

    private static string GetFileName
    (
        HttpResponseMessage response
    )
    {
        if (response
            .Content
            .Headers
            .TryGetValues("Content-Disposition", out IEnumerable<string> values)
        )
        {
            return GetFileName(values);
        }

        return string.Empty;
    }

    private static string GetFileName
    (
        IEnumerable<string> values
    )
    {
        var value = values.FirstOrDefault();
        if (!string.IsNullOrEmpty(value))
        {
            string[] contentDisposition = value.Split(';', StringSplitOptions.TrimEntries);
            if (contentDisposition.Length > 1)
            {
                contentDisposition = contentDisposition[1].Split('=', StringSplitOptions.TrimEntries);
                if (contentDisposition.Length > 1)
                    return contentDisposition[1].Trim();
            }
        }

        return string.Empty;
    }

    private async Task<HttpClient> GetHttpClient
    (
        CancellationToken cancellationToken
    )
    {
        HttpClient httpClient = _factory.CreateClient(_setting.ServiceName);

        if (_service is not null)
            httpClient
                .DefaultRequestHeaders
                .Authorization = await _service.AutenticarAsync(cancellationToken);

        return httpClient;
    }
}