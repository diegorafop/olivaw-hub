using Mps.OlivawCore.Settings;

namespace Mps.OlivawCore.Api.Settings;

public class SettingProviderAuthentication : ISettingProviderAuthentication
{
    public const string ConfigurationSection = "services:provider-authentication";

    public string Login { get; set; }
    public string Senha { get; set; }
    public int CacheSlidingExpiration { get; set; }
    public string CacheKey { get; set; }

    public bool Configured { get; set; }
    public string ServiceName { get; set; }
    public string BaseAddress { get; set; }
    public string AuthenticationType { get; set; }
    public int HttpClientTimeout { get; set; }
    public string DefaultHeaders { get; set; }

    public Dictionary<string, string> Routes { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}