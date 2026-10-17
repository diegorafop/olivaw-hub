using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Mps.HolosCore.Api.Services;
using Mps.HolosCore.Api.Validation;
using Mps.HolosCore.Repositories;
using Mps.HolosCore.Services;
using Mps.Limpo.Api.Extensions.Facades;
using Mps.Limpo.Dados.Repositories;
using System.IO;
using Xunit;

namespace Mps.Limpo.Tests;

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
            item.ServiceType == typeof(IProviderAuthentication)
            && item.ImplementationType == typeof(ProviderAuthentication));
        Assert.Contains(builder.Services, item =>
            item.ServiceType == typeof(IPipelineBehavior<,>)
            && item.ImplementationType == typeof(FluentValidatorBehavior<,>));
    }
}
