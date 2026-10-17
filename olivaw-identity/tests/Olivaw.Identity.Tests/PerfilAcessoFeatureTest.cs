using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Olivaw.Identity.Tests;

public class PerfilAcessoFeatureTest
{
    [Fact]
    public async Task Crud_should_insert_read_update_list_and_soft_delete()
    {
        await using var host = await IdentityHost.CreateAsync();

        var id = await host.InsertAsync("perfis-acesso", new { nome = "Operador" });
        var response = await host.Client.GetAsync("/identity/perfis-acesso/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain("senha", text.ToLowerInvariant());
        Assert.DoesNotContain("versaoSeguranca", text);
        response = await host.Client.PutAsJsonAsync("/identity/perfis-acesso/" + id, new { nome = "Supervisor", descricao = "Alterado", ativo = false }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var registro = updated.GetProperty("registro");
        Assert.Equal("Supervisor", registro.GetProperty("nome").GetString());
        var list = await host.Client.GetFromJsonAsync<JsonElement>("/identity/perfis-acesso?pagina=1&tamanho=1", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Single(list.GetProperty("registros").EnumerateArray());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.DeleteAsync("/identity/perfis-acesso/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/identity/perfis-acesso/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Inserir_should_reject_duplicate_identifier_or_link()
    {
        await using var host = await IdentityHost.CreateAsync();

        await host.InsertAsync("perfis-acesso", new { nome = "Operador" });
        var response = await host.Client.PostAsJsonAsync("/identity/perfis-acesso", new { nome = "Operador" }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Commands_should_reject_missing_required_fields(string method)
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/perfis-acesso";
        if (method == "PUT") path += "/" + Guid.NewGuid();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        var response = await host.Client.SendAsync(request, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("nome", (await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken)).ToLowerInvariant());
    }

    [Theory]
    [InlineData("?pagina=0&tamanho=10")]
    [InlineData("?pagina=1&tamanho=101")]
    public async Task Selecionar_should_reject_invalid_pagination(string query)
    {
        await using var host = await IdentityHost.CreateAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await host.Client.GetAsync("/identity/perfis-acesso" + query, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Selecionar_and_remover_should_reject_unknown_id()
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/perfis-acesso/" + Guid.NewGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.DeleteAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
