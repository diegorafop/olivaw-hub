using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Olivaw.Identity.Api;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Repositories;
using Xunit;

namespace Olivaw.Identity.Tests;

public class IdentityConsumerTest
{
    private static async Task<WebApplication> CreateConsumerAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(IdentityConsumerTest).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Issuer"] = "identity-tests",
            ["Identity:Audience"] = "identity-tests",
            ["Identity:SigningKey"] = "test-only-signing-key-with-at-least-32-bytes",
        });
        builder.Services.AddOlivawIdentityConsumer(builder.Configuration);
        builder.Services.AddAuthorization(options => options.AddPolicy("deploy", policy => policy
            .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement("devops.deploy"))));
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/protected", () => "ok").RequireAuthorization();
        app.MapGet("/deploy", () => "ok").RequireAuthorization("deploy");
        await app.StartAsync(Xunit.TestContext.Current.CancellationToken);
        return app;
    }

    [Fact]
    public async Task Consumer_should_use_token_claims_until_expiration_without_contacting_hub()
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        await using var authority = await IdentityHost.CreateAsync();
        var root = authority.Client.DefaultRequestHeaders.Authorization;
        var userId = await authority.InsertAsync("userprofiles", new
        {
            login = "ltmj", name = "LTMJ service", email = "ltmj@example.test",
            password = IdentityHost.Password, isServiceAccount = true
        });
        var profile = await authority.InsertAsync("accessprofiles", new { name = "LTMJ deployment" });
        var permission = await authority.InsertAsync("authorizations", new { key = "devops.deploy", name = "Deploy" });
        var grant = await authority.InsertAsync("accessprofileauthorizations", new { accessProfileId = profile, authorizationId = permission });
        await authority.InsertAsync("userprofileaccessprofiles", new { userProfileId = userId, accessProfileId = profile });
        var jwt = await authority.LoginAsync("ltmj", IdentityHost.Password);
        await using var consumer = await CreateConsumerAsync();
        Assert.Null(consumer.Services.GetService<IIdentityStore>());
        Assert.Null(consumer.Services.GetService<AuthenticationService>());
        using var client = consumer.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/deploy", token)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/identity/authentication/login", new { }, token)).StatusCode);
        authority.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await authority.Client.DeleteAsync($"/identity/accessprofileauthorizations/{grant}", token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/deploy", token)).StatusCode);
        var updatedToken = await authority.LoginAsync("ltmj", IdentityHost.Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", updatedToken);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/deploy", token)).StatusCode);
        authority.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/protected", token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await authority.Client.PutAsJsonAsync($"/identity/userprofiles/{userId}", new
        {
            login = "ltmj", name = "LTMJ service", email = "ltmj@example.test", isActive = false, isServiceAccount = true
        }, token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/protected", token)).StatusCode);
    }

    [Fact]
    public async Task Consumer_should_accept_hub_root_until_token_expires_even_after_hub_logout()
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        await using var authority = await IdentityHost.CreateAsync();
        var jwt = await authority.LoginAsync("root", IdentityHost.Password);
        await using var consumer = await CreateConsumerAsync();
        using var client = consumer.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/deploy", token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await authority.Client.PostAsJsonAsync("/identity/authentication/logout", new { }, token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/deploy", token)).StatusCode);
        var issued = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        var expired = new JwtSecurityToken(issued.Issuer, "identity-tests", issued.Claims,
            notBefore: DateTime.UtcNow.AddMinutes(-10), expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-only-signing-key-with-at-least-32-bytes")), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(expired));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/deploy", token)).StatusCode);
    }

    [Theory]
    [InlineData("ltmj", "identity-tests", "test-only-signing-key-with-at-least-32-bytes")]
    [InlineData("identity-tests", "ltmj", "test-only-signing-key-with-at-least-32-bytes")]
    [InlineData("identity-tests", "identity-tests", "client-only-signing-key-with-at-least-32-bytes")]
    public async Task Consumer_should_reject_client_issuer_audience_or_secret(string issuer, string audience, string key)
    {
        await using var authority = await IdentityHost.CreateAsync();
        await using var consumer = await CreateConsumerAsync();
        using var client = consumer.GetTestClient();
        var jwt = new JwtSecurityToken(issuer, audience, new[] { new Claim("sub", Guid.NewGuid().ToString()) },
            expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/protected", Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
