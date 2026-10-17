using Mps.OlivawCore.Services;

namespace Mps.OlivawCore.Api.Settings;

public class SettingLoki : ISettingLoki
{
    public const string ConfigurationSection = "services:loki";

    public bool Configured { get; set; }

    public string ServiceName { get; set; }
    public string BaseAddress { get; set; }
    public string AuthenticationType { get; set; }
    public int HttpClientTimeout { get; set; }
    public string DefaultHeaders { get; set; }

    public Dictionary<string, string> Routes { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}