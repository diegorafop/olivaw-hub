using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AuthenticationTest
{
    [Fact]
    public async Task Bootstrap_should_create_only_root_and_preserve_password_on_repeat()
    {
        await using var host = await IdentityHost.CreateAsync();
        await using var scope = host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestContext>();
        var usuario = Assert.Single(await context.Set<Usuario>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        Assert.Equal("root", usuario.Login);
        Assert.True(usuario.Root);
        Assert.NotEqual(IdentityHost.Password, usuario.SenhaHash);
        var hash = usuario.SenhaHash;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Bootstrap:Enabled"] = "true",
            ["Identity:Bootstrap:Password"] = "different password 123"
        }).Build();
        await scope.ServiceProvider.GetRequiredService<RootBootstrap>().EnsureAsync(config, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(hash, usuario.SenhaHash);
        Assert.Empty(await context.Set<PerfilAcesso>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        Assert.Empty(await context.Set<Autorizacao>().ToListAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("root", "wrong password")]
    [InlineData("missing", "wrong password")]
    public async Task Login_should_reject_invalid_credentials(string login, string senha)
    {
        await using var host = await IdentityHost.CreateAsync(false);
        var response = await host.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login, senha }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("SenhaHash", await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Login_should_preserve_password_whitespace()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        var response = await host.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login = "root", senha = IdentityHost.Password.Trim() }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await host.LoginAsync("root", IdentityHost.Password);
    }

    [Fact]
    public async Task Logout_should_invalidate_existing_tokens()
    {
        await using var host = await IdentityHost.CreateAsync();
        var response = await host.Client.PostAsJsonAsync("/identity/autenticacao/logout", new { }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/usuarios", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        await host.LoginAsync("root", IdentityHost.Password);
    }

    [Fact]
    public async Task ChangePassword_should_revoke_tokens_and_require_new_password()
    {
        await using var host = await IdentityHost.CreateAsync();
        var response = await host.Client.PutAsJsonAsync("/identity/autenticacao/senha", new { senhaAtual = IdentityHost.Password, novaSenha = "new password 123! " }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/usuarios", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        var failed = await host.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login = "root", senha = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        await host.LoginAsync("root", "new password 123! ");
    }

    [Fact]
    public async Task Login_should_lock_account_after_five_failures()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login = "root", senha = "incorrect" }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        var response = await host.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login = "root", senha = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task APIs_should_reject_unauthenticated_requests()
    {
        await using var host = await IdentityHost.CreateAsync(false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/identity/usuarios", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Independent_databases_should_not_share_users()
    {
        await using var joao = await IdentityHost.CreateAsync();
        await using var maria = await IdentityHost.CreateAsync();
        await joao.InsertAsync("usuarios", new { login = "joao", nome = "João", email = "joao@example.test", senha = IdentityHost.Password });
        var response = await maria.Client.PostAsJsonAsync("/identity/autenticacao/login", new { login = "joao", senha = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
