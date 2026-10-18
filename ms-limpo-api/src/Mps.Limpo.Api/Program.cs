using Mps.Limpo.Api.Extensions.Facades;
using Mps.Limpo.Api.Extensions.Hostings;

var builder = WebApplication.CreateBuilder(args);

builder.AddSetups();

var app = builder.Build();

app.UseSetups();
await app.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);

app.Run();
