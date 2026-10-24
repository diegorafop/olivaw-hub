using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Olivaw.Identity.Api;
using Olivaw.Identity.Dados;
using Xunit;

namespace Olivaw.Identity.Tests;

public sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOlivawIdentity();
    }
}

public sealed class IdentityHost : IAsyncDisposable
{
    public const string Password = "Test password 123! ";
    private readonly SqliteConnection _connection;
    private readonly WebApplication _application;
    public HttpClient Client { get; }
    public IServiceProvider Services => _application.Services;

    private IdentityHost(SqliteConnection connection, WebApplication application)
    {
        _connection = connection;
        _application = application;
        Client = application.GetTestClient();
    }

    public static async Task<IdentityHost> CreateAsync(bool authenticated = true)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Issuer"] = "identity-tests",
            ["Identity:Audience"] = "identity-tests",
            ["Identity:SigningKey"] = "test-only-signing-key-with-at-least-32-bytes",
            ["Identity:Bootstrap:Password"] = Password,
            ["Identity:Bootstrap:Email"] = "root@example.test"
        });
        builder.Services.AddDbContext<TestContext>(options => options.UseSqlite(connection));
        builder.Services.AddOlivawIdentity<TestContext>(builder.Configuration);
        builder.Services.AddAuthorization(options => options.AddPolicy("pedidos.cancelar", policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new Api.Security.PermissionRequirement("pedidos.cancelar"))));
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();
        app.MapGet("/pedidos-protegidos", () => "ok").RequireAuthorization("pedidos.cancelar");
        await using (var scope = app.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<TestContext>().Database.EnsureCreatedAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        await app.InitializeOlivawIdentityAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        await app.StartAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var host = new IdentityHost(connection, app);
        if (authenticated) await host.LoginAsync("root", Password);
        return host;
    }

    public async Task<string> LoginAsync(string login, string password)
    {
        var response = await Client.PostAsJsonAsync("/identity/authentication/login", new { login, password = password }, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var token = json.GetProperty("accessToken").GetString() ?? throw new InvalidOperationException();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    public async Task<Guid> InsertAsync(string resource, object payload)
    {
        var response = await Client.PostAsJsonAsync("/identity/" + resource, payload, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        return json.GetProperty("item").GetProperty("id").GetGuid();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _application.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
