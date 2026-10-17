using Microsoft.Extensions.Configuration;

namespace Mps.Auth.Dados.Extensions.Helpers;

public static class EnvironmentHelper
{
    public static IConfigurationRoot GetConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return new ConfigurationBuilder()
        .AddJsonFile
        (
            "appsettings.json",
            optional: true,
            reloadOnChange: true
        )
        .AddJsonFile
        (
            $"appsettings.{environment}.json",
            optional: true,
            reloadOnChange: true
        )
        .Build();
    }
}