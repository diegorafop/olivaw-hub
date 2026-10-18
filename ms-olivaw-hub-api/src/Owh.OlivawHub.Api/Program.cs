using Owh.OlivawHub.Api.Extensions.Facades;
using Owh.OlivawHub.Api.Extensions.Hostings;

var builder = WebApplication.CreateBuilder(args);

builder.AddSetups();

var app = builder.Build();

app.UseSetups();
await app.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);

app.Run();
