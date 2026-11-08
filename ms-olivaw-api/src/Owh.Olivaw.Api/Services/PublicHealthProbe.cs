using System.Net;
using System.Net.Sockets;

namespace Owh.Olivaw.Api.Services;

public sealed class PublicHealthProbe(HttpClient client, IConfiguration configuration)
{
    public bool IsAllowed(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != "https" || uri.Port != 443 || uri.UserInfo.Length > 0 || uri.Query.Length > 0 || uri.Fragment.Length > 0) return false;
        var allowed = configuration.GetSection("Monitoring:AllowedUrls").Get<string[]>() ?? Array.Empty<string>();
        return allowed.Contains(value, StringComparer.Ordinal);
    }
    public async Task<string> ReadAsync(string target, CancellationToken token)
    {
        if (!IsAllowed(target)) throw new InvalidOperationException("Destino de coleta não autorizado.");
        try
        {
            using var response = await client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, token);
            if (response.StatusCode == HttpStatusCode.OK) return "success";
            return "failure";
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { return "timeout"; }
        catch (HttpRequestException) { return "source-error"; }
    }
    public static bool IsPublicIPv4(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork) return false;
        var bytes = address.GetAddressBytes();
        if (bytes[0] is 0 or 10 or 127 || bytes[0] >= 224) return false;
        if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) return false;
        if (bytes[0] == 169 && bytes[1] == 254) return false;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
        if (bytes[0] == 192 && bytes[1] is 0 or 168) return false;
        if (bytes[0] == 198 && bytes[1] is 18 or 19 or 51) return false;
        if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113) return false;
        return true;
    }
    public static SocketsHttpHandler CreateHandler() => new()
    {
        AllowAutoRedirect = false, UseCookies = false, UseProxy = false, MaxConnectionsPerServer = 2,
        ConnectTimeout = TimeSpan.FromSeconds(3), MaxResponseHeadersLength = 16,
        ConnectCallback = async (context, token) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, token);
            var address = addresses.FirstOrDefault(IsPublicIPv4);
            if (address == null) throw new HttpRequestException("Destino sem endereço público permitido.");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), token);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch { socket.Dispose(); throw; }
        }
    };
}
