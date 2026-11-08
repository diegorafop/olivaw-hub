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
        var user = await host.InsertAsync("userprofiles", new { login = "joao", name = "João", email = "joao@example.test", password = IdentityHost.Password });
        var profile = await host.InsertAsync("accessprofiles", new { name = "Atendente" });
        var permission = await host.InsertAsync("authorizations", new { key = "pedidos.cancelar", name = "Cancelar pedido" });
        var grant = await host.InsertAsync("accessprofileauthorizations", new { accessProfileId = profile, authorizationId = permission });
        await host.InsertAsync("userprofileaccessprofiles", new { userProfileId = user, accessProfileId = profile });
        var root = host.Client.DefaultRequestHeaders.Authorization;
        await host.LoginAsync("joao", IdentityHost.Password);
        var employee = host.Client.DefaultRequestHeaders.Authorization;
        var acesso = await host.Client.GetFromJsonAsync<JsonElement>("/identity/authentication/access", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.False(acesso.GetProperty("isRoot").GetBoolean());
        Assert.Equal("pedidos.cancelar", Assert.Single(acesso.GetProperty("permissions").EnumerateArray()).GetString());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/identity/userprofiles", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.DeleteAsync("/identity/accessprofileauthorizations/" + grant, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        var restored = await host.InsertAsync("accessprofileauthorizations", new { accessProfileId = profile, authorizationId = permission });
        Assert.Equal(grant, restored);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.PutAsJsonAsync("/identity/accessprofiles/" + profile, new { name = "Atendente", isActive = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Root_should_reject_removal_deactivation_and_renaming()
    {
        await using var host = await IdentityHost.CreateAsync();
        var json = await host.Client.GetFromJsonAsync<JsonElement>("/identity/userprofiles", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var root = json.GetProperty("items")[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.DeleteAsync("/identity/userprofiles/" + root, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.PutAsJsonAsync("/identity/userprofiles/" + root, new { login = "root", name = "IsRoot", email = "root@example.test", isActive = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await host.Client.PutAsJsonAsync("/identity/userprofiles/" + root, new { login = "outro", name = "IsRoot", email = "root@example.test" }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Disabled_user_should_lose_access_with_existing_token()
    {
        await using var host = await IdentityHost.CreateAsync();
        var id = await host.InsertAsync("userprofiles", new { login = "joao", name = "João", email = "joao@example.test", password = IdentityHost.Password });
        var root = host.Client.DefaultRequestHeaders.Authorization;
        await host.LoginAsync("joao", IdentityHost.Password);
        var employee = host.Client.DefaultRequestHeaders.Authorization;
        host.Client.DefaultRequestHeaders.Authorization = root;
        Assert.Equal(HttpStatusCode.OK, (await host.Client.PutAsJsonAsync("/identity/userprofiles/" + id, new { login = "joao", name = "João", email = "joao@example.test", isActive = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = employee;
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/pedidos-protegidos", cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
