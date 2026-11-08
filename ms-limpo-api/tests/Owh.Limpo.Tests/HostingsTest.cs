using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Olivaw.Identity.Repositories;
using Olivaw.Identity.Dados;
using Owh.Limpo.Dados;
using Owh.OlivawCore.Api.Validation;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Services;
using Owh.Limpo.Api.Extensions.Facades;
using Owh.Limpo.Dados.Repositories;
using System.IO;
using Xunit;

namespace Owh.Limpo.Tests;

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
}
