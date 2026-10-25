using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.LeadFeatureTest;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityRemediationFeatureTest;
public sealed class SecurityRemediationHttpTest
{
    [Theory]
    [InlineData("", 401, 401)]
    [InlineData("operator", 403, 403)]
    [InlineData("executor", 403, 200)]
    [InlineData("root", 200, 200)]
    public async Task Routes_should_reserve_approval_for_root_and_execution_for_scoped_account(string actor, int adminStatus, int queueStatus)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused",
            ["Identity:Issuer"] = "security-tests", ["Identity:Audience"] = "security-tests", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, LeadTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var store = new Mock<IIdentityStore>();
        var user = new UserProfile { ExternalId = Guid.Parse("00000000-0000-0000-0000-000000000017"), IsRoot = actor == "root", SecurityStamp = Guid.Parse("00000000-0000-0000-0000-000000000001") };
        store.Setup(item => item.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        IReadOnlyList<string> permissions = actor == "executor" ? new[] { "security.remediations.execute" } : Array.Empty<string>();
        store.Setup(item => item.PermissionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(permissions);
        builder.Services.AddSingleton(store.Object);
        var report = SecurityRemediationFakerBuilder.Report();
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository(report).Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository<SecurityRemediation>().Object);
        await using var app = builder.Build();app.UseSetups();await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        using var read = await client.GetAsync("/security-remediations?reportExternalId="+report.ExternalId);
        Assert.Equal((HttpStatusCode)adminStatus,read.StatusCode);
        using var approval = await client.PostAsJsonAsync("/security-remediations",new { reportExternalId=report.ExternalId, findingIndex=0 });
        Assert.Equal((HttpStatusCode)adminStatus,approval.StatusCode);
        using var batch = await client.PostAsJsonAsync("/security-remediations/batch",new { reportExternalId=report.ExternalId });
        Assert.Equal((HttpStatusCode)adminStatus,batch.StatusCode);
        using var queue = await client.GetAsync("/security-remediations/queue?hostId=test-host");
        Assert.Equal((HttpStatusCode)queueStatus,queue.StatusCode);
        using var attempt = await client.PutAsJsonAsync("/security-remediations/"+Guid.NewGuid(),new { action="claim",leaseToken=Guid.NewGuid(),resultJson="{}" });
        Assert.Equal(queueStatus == 200 ? HttpStatusCode.Conflict : (HttpStatusCode)queueStatus,attempt.StatusCode);
        using var review = await client.PutAsJsonAsync("/security-remediations/"+Guid.NewGuid()+"/review",new { confirmedStopped=true,note="Processo encerrado e infraestrutura revisada pelo operador." });
        Assert.Equal(adminStatus == 200 ? HttpStatusCode.Conflict : (HttpStatusCode)adminStatus,review.StatusCode);
    }
}
