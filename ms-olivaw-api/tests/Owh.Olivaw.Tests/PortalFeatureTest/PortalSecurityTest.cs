using System;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.PortalFeatureTest;
public sealed class PortalSecurityTest
{
    [Fact]
    public async Task Portal_should_separate_authorities_scope_companies_and_apply_revocation_without_reissuing_jwt()
    {
        const string password = "  Password preserved 123  ";
        var hasher = new PasswordHasher<UserProfile>();
        var user = new UserProfile { Id = 101, ExternalId = Guid.NewGuid(), Login = "client@example.test", NormalizedLogin = "CLIENT@EXAMPLE.TEST", Email = "client@example.test", Name = "Cliente", IsActive = true };
        user.PasswordHash = hasher.HashPassword(user, password); var portalStore = new PortalIdentityStoreFake(user);
        var administrator = new UserProfile { Id = 102, ExternalId = InventoryFakerBuilder.Actor, Login = "admin", NormalizedLogin = "ADMIN", Name = "Operador", IsActive = true, IsRoot = true };
        administrator.PasswordHash = hasher.HashPassword(administrator, password); var internalStore = new PortalIdentityStoreFake(administrator);
        var company = ManagedServiceFakerBuilder.Customer(); var other = ManagedServiceFakerBuilder.Customer(2);
        var member = PortalMembership.Create(user.ExternalId, company, "manager", administrator.ExternalId, "Operador", InventoryFakerBuilder.Now);
        var service = ManagedServiceFakerBuilder.Create("customer"); service.Id = 1;
        service.Customers.Add(ManagedServiceCustomer.Create(service, company, administrator.ExternalId, "Operador", InventoryFakerBuilder.Now));
        var resource = InventoryFakerBuilder.Resource();
        var link = PortalServiceLink.Create(company, service, resource, "Seu site", "https://client.example.test", administrator.ExternalId, InventoryFakerBuilder.Now); link.Id = 1;
        var reading = HealthReading.Create(resource, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1), "INTERNAL-SOURCE", "INTERNAL-NOTE", "success", 15, administrator.ExternalId, "Operador", DateTime.UtcNow);
        var health = new Mock<IRepositoryHealthReading>(); health.Setup(item => item.ReadLatestAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { reading });
        var plan = CustomerPlan.Create(company, "Plano aprovado", 50, null, 2, "Condição pública", "INTERNAL-APPROVAL", InventoryFakerBuilder.Now, administrator.ExternalId, "Operador", InventoryFakerBuilder.Now);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer(); builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused",
            ["Identity:Issuer"] = "test-admin", ["Identity:Audience"] = "test-admin", ["Identity:SigningKey"] = "admin-test-only-key-at-least-32-bytes",
            ["PortalIdentity:Issuer"] = "test-portal", ["PortalIdentity:Audience"] = "test-portal", ["PortalIdentity:SigningKey"] = "portal-test-only-key-at-least-32-bytes", ["PortalIdentity:TokenMinutes"] = "30",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false" });
        builder.AddSetups(); builder.Services.AddSingleton<IIdentityStore>(internalStore);
        builder.Services.AddKeyedSingleton<IIdentityStore>("portal", portalStore);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository(member).Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository(company, other).Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository(link).Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository(plan).Object);
        builder.Services.AddSingleton(health.Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository<PortalUsageSample>().Object);
        builder.Services.AddSingleton(InventoryFakerBuilder.Repository<SupportRequest>().Object);
        await using var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient();
        var internalOptions = new global::Olivaw.Identity.Api.Security.IdentityOptions { Issuer = "test-admin", Audience = "test-admin", SigningKey = "admin-test-only-key-at-least-32-bytes", TokenMinutes = 30 };
        var portalOptions = app.Services.GetRequiredService<PortalAuthorityOptions>(); Assert.True(portalOptions.Configured);
        var internalToken = await new AuthenticationService(internalStore, hasher, internalOptions).LoginAsync("admin", password, CancellationToken.None);
        var portalToken = await new AuthenticationService(portalStore, hasher, portalOptions.Identity).LoginAsync(user.Email, password, CancellationToken.None);
        using (var anonymous = await client.GetAsync("/portal/companies")) Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", internalToken.AccessToken);
        using (var wrongAuthority = await client.GetAsync("/portal/companies")) Assert.Equal(HttpStatusCode.Forbidden, wrongAuthority.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", portalToken.AccessToken);
        using (var internalRoute = await client.GetAsync("/resources")) Assert.Equal(HttpStatusCode.Unauthorized, internalRoute.StatusCode);
        using (var allowed = await client.GetAsync("/portal/dashboard?customerExternalId=" + company.ExternalId))
        {
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode); Assert.True(allowed.Headers.CacheControl?.NoStore);
            var json = await allowed.Content.ReadAsStringAsync(); Assert.Contains("Seu site", json); Assert.DoesNotContain("INTERNAL-APPROVAL", json);
            Assert.DoesNotContain("INTERNAL", json); Assert.DoesNotContain("provider", json, StringComparison.OrdinalIgnoreCase); Assert.DoesNotContain("SecurityStamp", json, StringComparison.OrdinalIgnoreCase);
        }
        using (var foreign = await client.GetAsync("/portal/dashboard?customerExternalId=" + other.ExternalId)) Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        using (var home = await client.GetAsync("/portal/home?customerExternalId=" + company.ExternalId)) { Assert.Equal(HttpStatusCode.OK, home.StatusCode); var body = await home.Content.ReadAsStringAsync(); Assert.DoesNotContain("INTERNAL", body); }
        using (var foreignHome = await client.GetAsync("/portal/home?customerExternalId=" + other.ExternalId)) Assert.Equal(HttpStatusCode.Forbidden, foreignHome.StatusCode);
        using (var expense = await client.GetAsync("/expenses")) Assert.Equal(HttpStatusCode.Unauthorized, expense.StatusCode);
        using (var invoice = await client.GetAsync("/aws-invoices")) Assert.Equal(HttpStatusCode.Unauthorized, invoice.StatusCode);
        using (var version = await client.GetAsync("/plans/versions?planKey=bronze")) Assert.Equal(HttpStatusCode.Unauthorized, version.StatusCode);
        using (var samples = await client.PostAsJsonAsync("/portal-usage-samples", new { })) Assert.Equal(HttpStatusCode.Unauthorized, samples.StatusCode);
        member.Revoke(administrator.ExternalId, "Operador", DateTime.UtcNow);
        using (var revoked = await client.GetAsync("/portal/dashboard?customerExternalId=" + company.ExternalId)) Assert.Equal(HttpStatusCode.Forbidden, revoked.StatusCode);
    }
    [Theory]
    [InlineData("viewer", false, false)] [InlineData("requester", true, false)] [InlineData("manager", true, true)]
    public void Membership_should_scope_roles_and_disable_every_permission_after_revocation(string role, bool write, bool billing)
    {
        var member = PortalMembership.Create(Guid.NewGuid(), ManagedServiceFakerBuilder.Customer(), role, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        Assert.Equal(write, member.CanRequest); Assert.Equal(billing, member.CanReadBilling);
        member.Revoke(InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now.AddDays(1)); Assert.False(member.CanRequest); Assert.False(member.CanReadBilling);
    }
}
