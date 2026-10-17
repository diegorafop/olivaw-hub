using Mps.Auth.Settings;

namespace Mps.Auth.Api.Settings;

public class SettingNotificacao : ISettingNotificacao
{
    public const string ConfigurationSection = "services:notificacao";

    public bool Configured { get; set; }
    public string ServiceName { get; set; }
    public string BaseAddress { get; set; }
    public string AuthenticationType { get; set; }
    public int HttpClientTimeout { get; set; }
    public string DefaultHeaders { get; set; }

    public Dictionary<string, string> Routes { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}