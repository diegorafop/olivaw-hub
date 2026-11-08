using System.Collections.Generic;

namespace Owh.OlivawCore.Settings;

public interface ISetting
{
    bool Configured { get; }
    string ServiceName { get; }
    string BaseAddress { get; }
    string AuthenticationType { get; }
    int HttpClientTimeout { get; }
    string DefaultHeaders { get; }

    Dictionary<string, string> Routes { get; }
    Dictionary<string, string> Claims { get; }
}