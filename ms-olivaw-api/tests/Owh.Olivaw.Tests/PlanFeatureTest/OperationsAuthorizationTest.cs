using Owh.Olivaw.Api.Features.ExpenseFeature.Commands;
using Owh.Olivaw.Api.Features.ExpenseFeature.Queries;
using Owh.Olivaw.Client.Features.ExpenseFeature.Queries;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Tests.LeadFeatureTest;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Api.Features.IncidentFeature.Commands;
using Owh.Olivaw.Api.Features.IncidentFeature.Queries;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
using Owh.Olivaw.Api.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Api.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
namespace Owh.Olivaw.Tests.PlanFeatureTest;
public sealed class OperationsAuthorizationTest
{
    [Theory]
    [InlineData("", 401)] [InlineData("company", 403)] [InlineData("admin", 403)] [InlineData("root", 200)]
    public async Task Routes_should_require_root_and_keep_internal_data_out_of_public_responses(string actor, int expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused", ["Identity:Issuer"] = "test", ["Identity:Audience"] = "test", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes", ["masstransit:configured"] = "false", ["services:loki:configured"] = "false" });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, LeadTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<ReadHealthReadingByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadHealthReadingByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateHealthReadingMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateHealthReadingResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadUsageReadingByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadUsageReadingByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateUsageReadingMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateUsageReadingResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadCareEvidenceByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadCareEvidenceByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateCareEvidenceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateCareEvidenceResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadIncidentByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadIncidentByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateIncidentMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateIncidentResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadSupportRequestByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadSupportRequestByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateSupportRequestMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateSupportRequestResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadCostReferenceByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadCostReferenceByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateCostReferenceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateCostReferenceResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadCustomerPlanByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadCustomerPlanByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateCustomerPlanMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateCustomerPlanResult());
        mediator.Setup(item => item.Send(It.IsAny<ReadPlanStudyByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadPlanStudyByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreatePlanStudyMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreatePlanStudyResult());
        mediator.Setup(x=>x.Send(It.IsAny<ReadExpenseByFilterMediatorQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new ReadExpenseByFilterResponse());
        mediator.Setup(x=>x.Send(It.IsAny<CreateExpenseMediatorCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CreateExpenseResult());
        builder.Services.AddSingleton(mediator.Object);
        await using var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient(); if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        foreach (var route in new[] { "/expenses", "/health-readings", "/usage-readings", "/care-evidence", "/incidents", "/support-requests", "/plans/costs", "/plans/conditions", "/plans/studies" })
        {
            using var read = await client.GetAsync(route); Assert.Equal((HttpStatusCode)expected, read.StatusCode);
            if (expected == 200) Assert.True(read.Headers.CacheControl?.NoStore);
            using var created = await client.PostAsJsonAsync(route, new { }); Assert.Equal(expected == 200 ? HttpStatusCode.Created : (HttpStatusCode)expected, created.StatusCode);
        }
    }
}
