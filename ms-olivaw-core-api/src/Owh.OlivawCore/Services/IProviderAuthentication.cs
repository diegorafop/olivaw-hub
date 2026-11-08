using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Owh.OlivawCore.Services;

public interface IProviderAuthentication : IServiceHttpClient
{
    public string AutenticarRoute { get; }

    public Task<AuthenticationHeaderValue> AutenticarAsync
    (
        CancellationToken cancellationToken
    );
}