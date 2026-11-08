using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest;

public class CustomerIntegrationTest
{
    [Theory]
    [InlineData("", 401)]
    [InlineData("operator", 403)]
    [InlineData("root", 200)]
    public async Task All_routes_should_require_root_before_accessing_customer_data(string actor, int expected)
    {
        await using var fixture = await CustomerFixture.StartAsync(actor);
        foreach (var url in new[] { "/customers", "/customers/summary", $"/customers/{fixture.Customer.ExternalId}" })
        {
            using var response = await fixture.Client.GetAsync(url);
            Assert.Equal((HttpStatusCode)expected, response.StatusCode);
            if (expected == 200)
            {
                Assert.True(response.Headers.CacheControl?.NoStore);
                Assert.DoesNotContain("actorUserId", await response.Content.ReadAsStringAsync());
            }
        }
        if (expected != 200)
        {
            using var create = await fixture.Client.PostAsJsonAsync("/customers", Input());
            using var update = await fixture.Client.PutAsJsonAsync($"/customers/{fixture.Customer.ExternalId}", new UpdateCustomerCommand());
            Assert.Equal((HttpStatusCode)expected, create.StatusCode);
            Assert.Equal((HttpStatusCode)expected, update.StatusCode);
            fixture.Repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
    [Fact]
    public async Task Create_should_persist_company_contacts_and_audit_without_creating_login()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        using var response = await fixture.Client.PostAsJsonAsync("/customers", Input());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.ExternalId);
        Assert.Equal(1, result.Revision);
        var customer = fixture.Items.Single(item => item.ExternalId == result.ExternalId);
        Assert.Equal("onboarding", customer.Status);
        Assert.Equal("Pastelaria", customer.Business);
        Assert.Equal("11987654321", Assert.Single(customer.Contacts).Whatsapp);
        Assert.Equal(CustomerFixture.ActorId, customer.ActorUserId);
        Assert.Equal("created", Assert.Single(customer.Changes).Action);
        Assert.Equal("Equipe de teste", Assert.Single(customer.Changes).ActorName);
        Assert.Contains(result.ExternalId.ToString(), response.Headers.Location?.ToString());
    }
    [Fact]
    public async Task Update_should_preserve_identity_append_history_and_reject_stale_revision()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        var command = new UpdateCustomerCommand { Business = "Empresa atualizada", Revision = 1, Status = "active", Contacts = Input().Contacts };
        using var response = await fixture.Client.PutAsJsonAsync($"/customers/{fixture.Customer.ExternalId}", command);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, fixture.Customer.Revision);
        Assert.Equal("active", fixture.Customer.Status);
        Assert.Equal(2, fixture.Customer.Changes.Count);
        using var stale = await fixture.Client.PutAsJsonAsync($"/customers/{fixture.Customer.ExternalId}", command);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(2, fixture.Customer.Revision);
        fixture.Repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Create_should_reject_missing_contact_and_invalid_query_and_missing_company()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        var input = Input(); input.Contacts[0].Whatsapp = "";
        using var invalid = await fixture.Client.PostAsJsonAsync("/customers", input);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var query = await fixture.Client.GetAsync("/customers?pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, query.StatusCode);
        using var missing = await fixture.Client.GetAsync($"/customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        fixture.Repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Create_should_reject_duplicate_lead_and_keep_origin_explicit()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        var input = Input(); input.SourceLeadExternalId = fixture.Lead.ExternalId;
        using var first = await fixture.Client.PostAsJsonAsync("/customers", input);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var duplicate = await fixture.Client.PostAsJsonAsync("/customers", input);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var body = await duplicate.Content.ReadAsStringAsync();
        Assert.Contains("existingExternalId", body);
        Assert.Single(fixture.Items, item => item.SourceLeadId == fixture.Lead.Id);
    }
    [Theory]
    [InlineData("/customers?page=1&pageSize=20&search=&status=")]
    [InlineData("/customers?page=1&pageSize=20&search=&status=active")]
    [InlineData("/customers?page=1&pageSize=20&search=Empresa&status=")]
    public async Task Read_should_accept_empty_optional_filters_sent_by_frontend(string url)
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        using var response = await fixture.Client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    internal static CreateCustomerCommand Input() => new()
    {
        Business = "Pastelaria", Contacts = [new CustomerContactInput { Name = "Responsável", Whatsapp = "11987654321" }]
    };
}
internal sealed class CustomerFixture : IAsyncDisposable
{
    public static readonly Guid ActorId = Guid.Parse("12345678-1234-1234-1234-123456789012");
    public required WebApplication App { get; init; }
    public required HttpClient Client { get; init; }
    public required Mock<IRepository<Customer>> Repository { get; init; }
    public required List<Customer> Items { get; init; }
    public required Customer Customer { get; init; }
    public required Lead Lead { get; init; }
    public static async Task<CustomerFixture> StartAsync(string actor)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused",
            ["Identity:Issuer"] = "customer-tests", ["Identity:Audience"] = "customer-tests", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, CustomerTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var customer = Customer.Create("Empresa original", "", null, "", "", ActorId, "Equipe", DateTime.UtcNow, null);
        customer.Id = 1;
        customer.Contacts.Add(CustomerContact.Create("Maria", "", "maria@example.test", "", ActorId, DateTime.UtcNow));
        var items = new List<Customer> { customer };
        var repository = new Mock<IRepository<Customer>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Customer, object>>[]>()))
            .Returns<Expression<Func<Customer, bool>>, CancellationToken, Expression<Func<Customer, object>>[]>((filter, _, _) => Task.FromResult(items.FirstOrDefault(filter.Compile())!));
        repository.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Customer, bool>>, CancellationToken>((filter, _) => Task.FromResult(items.Any(filter.Compile())));
        repository.Setup(item => item.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>())).Callback<Customer, CancellationToken>((entity, _) => { entity.Id = items.Count + 1; items.Add(entity); }).Returns(Task.CompletedTask);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        builder.Services.AddSingleton(repository.Object);
        var read = new Mock<IRepositoryCustomer>();
        read.Setup(item => item.ReadPageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CustomerReadPage { Total = 1, Items = [customer] });
        read.Setup(item => item.ReadCountsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<string, long> { ["onboarding"] = 1 });
        builder.Services.AddSingleton(read.Object);
        var changes = new Mock<IRepository<CustomerChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<CustomerChange, bool>>>(), 20, It.IsAny<Expression<Func<CustomerChange, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<CustomerChange, object>>[]>())).ReturnsAsync(customer.Changes);
        builder.Services.AddSingleton(changes.Object);
        var lead = Lead.Create(Guid.NewGuid(), new string('a',64), 1, "{}", DateTime.UtcNow); lead.Id = 11;
        var leads = new Mock<IRepository<Lead>>();
        leads.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>()))
            .Returns<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) => Task.FromResult(filter.Compile()(lead) ? lead : null!));
        builder.Services.AddSingleton(leads.Object);
        var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        var client = app.GetTestClient(); if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        return new CustomerFixture { App = app, Client = client, Repository = repository, Items = items, Customer = customer, Lead = lead };
    }
    public async ValueTask DisposeAsync() { Client.Dispose(); await App.DisposeAsync(); }
}
public sealed class CustomerTestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var actor = Request.Headers["X-Test-Actor"].ToString();
        if (actor.Length == 0) return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim("sub", CustomerFixture.ActorId.ToString()), new Claim("name", "Equipe de teste"), new Claim("root", (actor == "root").ToString().ToLowerInvariant()) }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
