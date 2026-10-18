using System.Threading;
using System.Threading.Tasks;

namespace Owh.OlivawCore.Services;

public interface IServiceHttpClient
{
    public string BaseAddress { get; }

    public bool Configured { get; }

    public Task<bool> HealthAsync
    (
        CancellationToken cancellationToken
    );

    public Task<T> GetAsync<T>
    (
        string route,
        CancellationToken cancellationToken
    ) where T : class;

    public Task<T> PostAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class;

    public Task<T> PutAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class;

    public Task<T> PostFormDataAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class;

    public Task<T> PutFormDataAsync<T>
    (
        string route,
        object content,
        CancellationToken cancellationToken
    ) where T : class;

    public Task<T> DeleteAsync<T>
    (
        string route,
        CancellationToken cancellationToken
    ) where T : class;
}