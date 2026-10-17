using Mps.Auth.Api.Extensions.Facades;
using Mps.Auth.Api.Extensions.Hostings;

var builder = WebApplication.CreateBuilder(args);

builder.AddSetups();

var app = builder.Build();

app.UseSetups();
app.UseMigrations();

app.Run();