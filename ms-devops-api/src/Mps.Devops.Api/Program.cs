using Mps.Devops.Api.Extensions.Facades;
using Olivaw.Identity.Api;
using Mps.Devops.Api.Extensions.Hostings;

var builder = WebApplication.CreateBuilder(args);

builder.AddSetups();

var app = builder.Build();

app.UseSetups();
app.UseMigrations();

await app.InitializeOlivawIdentityAsync();

app.Run();
