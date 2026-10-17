using Microsoft.Extensions.Configuration;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class ConfigurationExtensionsTest
{
    [Fact]
    public void Settings_should_bind_required_sections()
    {
        using var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["services:provider-authentication:ServiceName"] = "auth",
            ["services:provider-authentication:BaseAddress"] = "https://service.test/",
            ["services:provider-authentication:HttpClientTimeout"] = "20",
            ["services:provider-authentication:Routes:autenticar-usuario"] = "authenticate",
            ["masstransit:Configured"] = "false",
            ["masstransit:Prefix"] = "olivaw"
        });
        var authentication = config.GetSettingProviderAuthentication();
        Assert.Equal("auth", authentication.ServiceName);
        Assert.Equal(20, authentication.HttpClientTimeout);
        Assert.Equal("authenticate", authentication.Routes["autenticar-usuario"]);
        Assert.Equal("olivaw", config.GetSettingMassTransit().Prefix);
        Assert.False(config.GetSettingMassTransit().Configured);
    }

    [Fact]
    public void Settings_should_report_missing_section_names()
    {
        using var config = new ConfigurationManager();
        Assert.Contains("services:provider-authentication", Assert.Throws<InvalidOperationException>(
            () => config.GetSettingProviderAuthentication()).Message);
        Assert.Contains("masstransit", Assert.Throws<InvalidOperationException>(() => config.GetSettingMassTransit()).Message);
    }
}