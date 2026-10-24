using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Olivaw.Identity.Repositories;
using Olivaw.Identity.Dados;
using Owh.Olivaw.Dados;
using Owh.OlivawCore.Api.Validation;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Services;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Dados.Repositories;
using System.IO;
using Xunit;

namespace Owh.Olivaw.Tests;

public class HostingsTest
{
    [Fact]
    public void AddSetups_should_register_core_services_and_local_repository()
    {
        var builder = WebApplication.CreateBuilder
        (
            new WebApplicationOptions
            {
                ContentRootPath = Path.GetTempPath(),
                EnvironmentName = "Development"
            }
        );
        builder.Configuration.AddInMemoryCollection
        (
            new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Identity:Issuer"] = "host-tests",
                ["Identity:Audience"] = "host-tests",
                ["Identity:SigningKey"] = "test-only-key-with-more-than-32-bytes",
                ["DatabaseProvider"] = "PostgreSql",
                ["ConnectionStrings:DbContext"] = "Host=localhost;SearchPath=public",
                ["masstransit:configured"] = "false",
                ["services:loki:configured"] = "false",
                ["services:provider-authentication:serviceName"] = "provider-authentication",
                ["services:provider-authentication:baseAddress"] = "http://localhost",
                ["services:provider-authentication:httpClientTimeout"] = "30"
            }
        );

        builder.AddSetups();

        Assert.Contains(builder.Services, item =>
            item.ServiceType == typeof(IRepository<>)
            && item.ImplementationType == typeof(Repository<>));
        Assert.Contains(builder.Services, item =>
            item.ServiceType == typeof(IIdentityStore)
            && item.ImplementationType == typeof(IdentityStore<WriteDbContext>));
        Assert.Contains(builder.Services, item =>
            item.ServiceType == typeof(IPipelineBehavior<,>)
            && item.ImplementationType == typeof(TemplateValidationBehavior<,>));
    }
    [Fact]
    public void Readiness_context_should_share_the_registered_write_context_within_each_scope()
    {
        var services = new ServiceCollection();
        services.AddDbContext<WriteDbContext>(options => { });
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        Owh.Olivaw.Dados.Extensions.ServiceCollectionExtensions.AddRepositories(services);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        var firstContext = first.ServiceProvider.GetRequiredService<WriteDbContext>();
        Assert.Same(firstContext, first.ServiceProvider.GetRequiredService<IWriteDbContext>());
        Assert.Same(firstContext, first.ServiceProvider.GetRequiredService<IWriteDbContext>());
        Assert.NotSame(firstContext, second.ServiceProvider.GetRequiredService<IWriteDbContext>());
    }
}
