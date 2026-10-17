using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Mps.Devops.Api.Extensions.Facades;
using Mps.Devops.Api.Features.ImplantacaoFeature;
using Mps.Devops.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Xunit;

namespace Mps.Devops.Tests.ImplantacaoFeatureTest;

public class ImplantacaoIntegrationTest
{
    [Theory]
    [InlineData("empty", 400)] [InlineData("injection", 400)] [InlineData("path", 400)]
    [InlineData("checkout", 400)] [InlineData("pull", 400)] [InlineData("docker", 400)]
    [InlineData("environment", 400)] [InlineData("directory", 400)] [InlineData("success", 200)]
    [InlineData("anonymous", 401)] [InlineData("forbidden", 403)]
    public async Task Post_should_return_standard_response_and_enforce_permission(string scenario, int expectedStatus)
    {
        const string key = "test-only-deployment-signing-key-at-least-32-bytes";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing", ApplicationName = typeof(ImplantacaoController).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=test;SearchPath=ms_devops",
            ["Identity:Issuer"] = "deployment-tests", ["Identity:Audience"] = "deployment-tests", ["Identity:SigningKey"] = key,
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        var user = new Usuario { Id = 1, IdExterno = Guid.NewGuid() };
        var store = new Mock<IIdentityStore>();
        store.Setup(item => item.FindAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        IReadOnlyList<string> permissions = new[] { "devops.implantar" };
        if (scenario == "forbidden") permissions = Array.Empty<string>();
        store.Setup(item => item.PermissionsAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(permissions);
        builder.Services.AddScoped(_ => store.Object);
        var processes = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        processes.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeploymentStep>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, string, DeploymentStep, CancellationToken>((folder, branch, environment, step, token) =>
            {
                if (scenario == "directory") throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Folder", "Folder indisponível.") });
                var success = true;
                if (scenario == "checkout") success = step != DeploymentStep.Checkout;
                if (scenario == "pull") success = step != DeploymentStep.Pull;
                if (scenario == "docker") success = step != DeploymentStep.Compose;
                return Task.FromResult(new ProcessExecutionResult { Success = success, Log = step.ToString() });
            });
        builder.Services.AddScoped(_ => processes.Object);
        await using var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient();
        if (scenario != "anonymous")
        {
            var jwt = new JwtSecurityToken("deployment-tests", "deployment-tests",
                new[] { new Claim("sub", user.IdExterno.ToString()), new Claim("stamp", user.VersaoSeguranca.ToString()) },
                expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        }
        var folder = "repo"; var branchName = "develop"; var environmentName = "dev";
        if (scenario == "empty") { folder = ""; branchName = ""; }
        if (scenario == "injection") branchName = "main;id";
        if (scenario == "path") folder = "../repo";
        if (scenario == "environment") environmentName = "dev;id";
        var response = await client.PostAsJsonAsync("/implantacoes", new { folder = folder, branch = branchName, environment = environmentName });
        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
        if (expectedStatus == 400)
        {
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
            Assert.Equal("Operação inválida", json.RootElement.GetProperty("title").GetString());
            Assert.NotEmpty(json.RootElement.GetProperty("errors").EnumerateObject());
            Assert.False(json.RootElement.TryGetProperty("stackTrace", out _));
        }
        if (expectedStatus == 200)
        {
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(string.Join(Environment.NewLine, "Checkout", "Pull", "Compose"), json.RootElement.GetProperty("log").GetString());
        }
        if (scenario is "empty" or "injection" or "path" or "environment" or "anonymous" or "forbidden") processes.VerifyNoOtherCalls();
    }
}
