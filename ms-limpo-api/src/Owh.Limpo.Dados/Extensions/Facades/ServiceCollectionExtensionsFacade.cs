using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Owh.Limpo.Dados.Extensions.Facades;

public static class ServiceCollectionExtensionsFacade
{
    public static void AddDatabase
    (
        this IServiceCollection services,
        ConfigurationManager configuration,
        bool isProduction
    )
    {
        services.AddDbContext(configuration, isProduction);
        services.AddRepositories();
    }
}