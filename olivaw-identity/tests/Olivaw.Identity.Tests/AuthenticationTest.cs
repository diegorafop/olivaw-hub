using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Authorization = Olivaw.Identity.Domain.Authorization;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AuthenticationTest
{
    [Fact]
    public async Task Short_password_should_allow_signup_login_and_change_without_email_confirmation()
    {
        await using var host = await IdentityHost.CreateAsync();
        await host.InsertAsync("userprofiles", new { login = "demo", name = "Equipe", email = "equipe@example.test", password = "demo7* " });
        await host.LoginAsync("demo", "demo7* ");
        var token = Xunit.TestContext.Current.CancellationToken;
        var response = await host.Client.PutAsJsonAsync("/identity/authentication/password",
            new { currentPassword = "demo7* ", newPassword = "z " }, cancellationToken: token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/authentication/access", token)).StatusCode);
        await host.LoginAsync("demo", "z ");
        var wrong = await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "demo", password = "z" }, cancellationToken: token);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
    }

    [Fact]
    public async Task Bootstrap_should_create_only_root_and_preserve_password_on_repeat()
    {
        await using var host = await IdentityHost.CreateAsync();
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestContext>();
        var usuario = Assert.Single(await context.Set<UserProfile>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        Assert.Equal("root", usuario.Login);
        Assert.True(usuario.IsRoot);
        Assert.NotEqual(IdentityHost.Password, usuario.PasswordHash);
        var hash = usuario.PasswordHash;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Bootstrap:Password"] = "different password 123"
        }).Build();
        await scope.ServiceProvider.GetRequiredService<RootBootstrap>().EnsureAsync(config, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(hash, usuario.PasswordHash);
        Assert.Empty(await context.Set<AccessProfile>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        Assert.Empty(await context.Set<Authorization>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("root", "wrong password")]
    [InlineData("missing", "wrong password")]
    public async Task Login_should_reject_invalid_credentials(string login, string password)
    {
        await using var host = await IdentityHost.CreateAsync(false);
        var response = await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login, password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("PasswordHash", await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Login_should_preserve_password_whitespace()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        var response = await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "root", password = IdentityHost.Password.Trim() }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await host.LoginAsync("root", IdentityHost.Password);
    }

    [Fact]
    public async Task Logout_should_invalidate_existing_tokens()
    {
        await using var host = await IdentityHost.CreateAsync();
        var response = await host.Client.PostAsJsonAsync("/identity/authentication/logout", new { }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/userprofiles", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        await host.LoginAsync("root", IdentityHost.Password);
    }

    [Fact]
    public async Task ChangePassword_should_revoke_tokens_and_require_new_password()
    {
        await using var host = await IdentityHost.CreateAsync();
        var response = await host.Client.PutAsJsonAsync("/identity/authentication/password", new { currentPassword = IdentityHost.Password, newPassword = "new password 123! " }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/userprofiles", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        var failed = await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "root", password = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        await host.LoginAsync("root", "new password 123! ");
    }

    [Fact]
    public async Task Login_should_lock_account_after_five_failures()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "root", password = "incorrect" }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        var response = await host.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "root", password = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task APIs_should_reject_unauthenticated_requests()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/userprofiles", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Independent_databases_should_not_share_users()
    {
        await using var joao = await IdentityHost.CreateAsync();
        await using var maria = await IdentityHost.CreateAsync();
        await joao.InsertAsync("userprofiles", new { login = "joao", name = "João", email = "joao@example.test", password = IdentityHost.Password });
        var response = await maria.Client.PostAsJsonAsync("/identity/authentication/login", new { login = "joao", password = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
