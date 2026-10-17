using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UsuarioPerfilAcessoFeatureTest
{
    [Fact]
    public async Task Crud_should_insert_read_update_list_and_soft_delete()
    {
        await using var host = await IdentityHost.CreateAsync();
        var user = await host.InsertAsync("usuarios", new { login = "cliente", nome = "Cliente", email = "cliente@example.test", senha = IdentityHost.Password });
        var profile = await host.InsertAsync("perfis-acesso", new { nome = "Operador" });
        var profile2 = await host.InsertAsync("perfis-acesso", new { nome = "Supervisor" });

        var id = await host.InsertAsync("usuarios-perfis", new { usuarioId = user, perfilAcessoId = profile });
        var response = await host.Client.GetAsync("/identity/usuarios-perfis/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain("senha", text.ToLowerInvariant());
        Assert.DoesNotContain("versaoSeguranca", text);
        response = await host.Client.PutAsJsonAsync("/identity/usuarios-perfis/" + id, new { usuarioId = user, perfilAcessoId = profile2 }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var registro = updated.GetProperty("registro");
        Assert.Equal(profile2, registro.GetProperty("perfilAcessoId").GetGuid());
        var list = await host.Client.GetFromJsonAsync<JsonElement>("/identity/usuarios-perfis?pagina=1&tamanho=1", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Single(list.GetProperty("registros").EnumerateArray());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.DeleteAsync("/identity/usuarios-perfis/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/identity/usuarios-perfis/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Inserir_should_reject_duplicate_identifier_or_link()
    {
        await using var host = await IdentityHost.CreateAsync();
        var user = await host.InsertAsync("usuarios", new { login = "cliente", nome = "Cliente", email = "cliente@example.test", senha = IdentityHost.Password });
        var profile = await host.InsertAsync("perfis-acesso", new { nome = "Operador" });
        var profile2 = await host.InsertAsync("perfis-acesso", new { nome = "Supervisor" });

        await host.InsertAsync("usuarios-perfis", new { usuarioId = user, perfilAcessoId = profile });
        var response = await host.Client.PostAsJsonAsync("/identity/usuarios-perfis", new { usuarioId = user, perfilAcessoId = profile }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Commands_should_reject_missing_required_fields(string method)
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/usuarios-perfis";
        if (method == "PUT") path += "/" + Guid.NewGuid();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        var response = await host.Client.SendAsync(request, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("usuarioid", (await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken)).ToLowerInvariant());
    }

    [Theory]
    [InlineData("?pagina=0&tamanho=10")]
    [InlineData("?pagina=1&tamanho=101")]
    public async Task Selecionar_should_reject_invalid_pagination(string query)
    {
        await using var host = await IdentityHost.CreateAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await host.Client.GetAsync("/identity/usuarios-perfis" + query, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Selecionar_and_remover_should_reject_unknown_id()
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/usuarios-perfis/" + Guid.NewGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.DeleteAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
