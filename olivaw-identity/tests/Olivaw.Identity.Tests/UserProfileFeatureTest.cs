using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UserProfileFeatureTest
{
    [Fact]
    public async Task Crud_should_insert_read_update_list_and_soft_delete()
    {
        await using var host = await IdentityHost.CreateAsync();

        var id = await host.InsertAsync("userprofiles", new { login = "cliente", name = "Cliente", email = "cliente@example.test", password = IdentityHost.Password });
        var response = await host.Client.GetAsync("/identity/userprofiles/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain("password", text.ToLowerInvariant());
        Assert.DoesNotContain("versaoSeguranca", text);
        response = await host.Client.PutAsJsonAsync("/identity/userprofiles/" + id, new { login = "cliente-novo", name = "Cliente alterado", email = "novo@example.test", isActive = true }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var item = updated.GetProperty("item");
        Assert.Equal("Cliente alterado", item.GetProperty("name").GetString());
        var list = await host.Client.GetFromJsonAsync<JsonElement>("/identity/userprofiles?page=1&pageSize=1", cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Single(list.GetProperty("items").EnumerateArray());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.DeleteAsync("/identity/userprofiles/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/identity/userprofiles/" + id, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Create_should_reject_duplicate_identifier_or_link()
    {
        await using var host = await IdentityHost.CreateAsync();

        await host.InsertAsync("userprofiles", new { login = "cliente", name = "Cliente", email = "cliente@example.test", password = IdentityHost.Password });
        var response = await host.Client.PostAsJsonAsync("/identity/userprofiles", new { login = "cliente", name = "Cliente", email = "cliente@example.test", password = IdentityHost.Password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Commands_should_reject_missing_required_fields(string method)
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/userprofiles";
        if (method == "PUT") path += "/" + Guid.NewGuid();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        var response = await host.Client.SendAsync(request, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("login", (await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken)).ToLowerInvariant());
    }

    [Theory]
    [InlineData("?page=0&pageSize=10")]
    [InlineData("?page=1&pageSize=101")]
    public async Task Read_should_reject_invalid_pagination(string query)
    {
        await using var host = await IdentityHost.CreateAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await host.Client.GetAsync("/identity/userprofiles" + query, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Read_and_remover_should_reject_unknown_id()
    {
        await using var host = await IdentityHost.CreateAsync();
        var path = "/identity/userprofiles/" + Guid.NewGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.DeleteAsync(path, cancellationToken: Xunit.TestContext.Current.CancellationToken)).StatusCode);
    }
}
