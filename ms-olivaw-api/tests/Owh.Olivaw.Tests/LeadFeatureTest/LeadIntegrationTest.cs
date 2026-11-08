using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest;

public class LeadIntegrationTest
{
    [Fact]
    public async Task Update_should_persist_public_submission_without_exposing_read_or_cross_journey_write()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql",
            ["ConnectionStrings:DbContext"] = "Host=localhost;Database=lead_tests;SearchPath=public",
            ["Identity:Issuer"] = "lead-tests", ["Identity:Audience"] = "lead-tests",
            ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        Lead? saved = null;
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(() => saved!);
        repository.Setup(item => item.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Callback<Lead, CancellationToken>((lead, _) => saved = lead).Returns(Task.CompletedTask);
        builder.Services.AddSingleton(repository.Object);
        await using var app = builder.Build();
        app.UseSetups();
        await app.StartAsync();
        using var client = app.GetTestClient();
        var request = LeadFeatureFakerBuilder.Create();
        var url = $"/leads/{request.ExternalId}";
        using var missingKey = await client.PutAsJsonAsync(url, request.Payload);
        Assert.Equal(HttpStatusCode.BadRequest, missingKey.StatusCode);
        client.DefaultRequestHeaders.Add("X-Lead-Key", request.WriteKey);
        using var response = await client.PutAsJsonAsync(url, request.Payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(saved);
        using var retry = await client.PutAsJsonAsync(url, request.Payload);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        repository.Verify(item => item.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        request.Payload.Proposal = new() { Title = "Condições falsas" };
        using var spoofed = await client.PutAsJsonAsync(url, request.Payload);
        Assert.Equal(HttpStatusCode.BadRequest, spoofed.StatusCode);
        request.Payload.Proposal = null;
        using var read = await client.GetAsync(url);
        Assert.NotEqual(HttpStatusCode.OK, read.StatusCode);
        client.DefaultRequestHeaders.Remove("X-Lead-Key");
        client.DefaultRequestHeaders.Add("X-Lead-Key", new string('b', 64));
        request.Payload.Revision = 2;
        using var forbidden = await client.PutAsJsonAsync(url, request.Payload);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        await using var scope = app.Services.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<WriteDbContext>().Model;
        Assert.Equal("lead", model.FindEntityType(typeof(Lead))!.GetTableName());
        Assert.True(model.FindEntityType(typeof(Lead))!.FindProperty(nameof(Lead.Revision))!.IsConcurrencyToken);
    }

    [Fact]
    public void Model_should_support_sql_server_lead_mapping_without_database_connection()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>().UseSqlServer("Server=localhost;Database=lead_tests;Integrated Security=true;").UseLowerCaseNamingConvention().Options;
        using var context = new WriteDbContext(options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = "SqlServer" }).Build());
        var entity = context.Model.FindEntityType(typeof(Lead))!;
        Assert.Equal("lead", entity.GetTableName());
        Assert.Equal(32000, entity.FindProperty(nameof(Lead.Snapshot))!.GetMaxLength());
    }
}
