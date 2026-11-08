using Owh.Cavalcante.Api.Extensions.Facades;
using Owh.Cavalcante.Api.Extensions.Hostings;

var builder = WebApplication.CreateBuilder(args);

builder.AddSetups();

var app = builder.Build();

app.UseSetups();
await app.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);

app.Run();
