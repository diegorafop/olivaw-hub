using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AccessControlTest
{
    [Fact]
    public async Task Permissions_should_follow_profile_links_and_revoke_without_new_login()
    {
        await using var host = await IdentityHost.CreateAsync();
        var user = await host.InsertAsync("usuarios", new { login = "joao", nome = "João", email = "joao@example.test", senha = IdentityHost.Password });
        var profile = await host.InsertAsync("perfis-acesso", new { nome = "Atendente" });
        var permission = await host.InsertAsync("autorizacoes", new { chave = "pedidos.cancelar", nome = "Cancelar pedido" });
        var grant = await host.InsertAsync("perfis-autorizacoes", new { perfilAcessoId = profile, autorizacaoId = permission });
        await host.InsertAsync("usuarios-perfis", new { usuarioId = user, perfilAcessoId = profile });
        var root = host.Client.DefaultRequestHeaders.Authorization;
        await host.LoginAsync("joao", IdentityHost.Password);
        var employee = host.Client.DefaultRequestHeaders.Authorization;
        var acesso = await host.Client.GetFromJsonAsync<JsonElement>("/identity/autenticacao/acesso", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.False(acesso.GetProperty("root").GetBoolean());
        Assert.Equal("pedidos.cancelar", Assert.Single(acesso.GetProperty("autorizacoes").EnumerateArray()).GetString());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/identity/usuarios", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.DeleteAsync("/identity/perfis-autorizacoes/" + grant, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        var restored = await host.InsertAsync("perfis-autorizacoes", new { perfilAcessoId = profile, autorizacaoId = permission });
        Assert.Equal(grant, restored);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.PutAsJsonAsync("/identity/perfis-acesso/" + profile, new { nome = "Atendente", ativo = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Root_should_reject_removal_deactivation_and_renaming()
    {
        await using var host = await IdentityHost.CreateAsync();
        var json = await host.Client.GetFromJsonAsync<JsonElement>("/identity/usuarios", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var root = json.GetProperty("registros")[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.DeleteAsync("/identity/usuarios/" + root, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.PutAsJsonAsync("/identity/usuarios/" + root, new { login = "root", nome = "Root", email = "root@example.test", ativo = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.PutAsJsonAsync("/identity/usuarios/" + root, new { login = "outro", nome = "Root", email = "root@example.test" }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Disabled_user_should_lose_access_with_existing_token()
    {
        await using var host = await IdentityHost.CreateAsync();
        var id = await host.InsertAsync("usuarios", new { login = "joao", nome = "João", email = "joao@example.test", senha = IdentityHost.Password });
        var root = host.Client.DefaultRequestHeaders.Authorization;
        await host.LoginAsync("joao", IdentityHost.Password);
        var employee = host.Client.DefaultRequestHeaders.Authorization;
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.PutAsJsonAsync("/identity/usuarios/" + id, new { login = "joao", nome = "João", email = "joao@example.test", ativo = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
