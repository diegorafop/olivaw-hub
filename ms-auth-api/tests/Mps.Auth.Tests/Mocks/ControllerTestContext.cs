using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mps.Auth.Tests.Mocks;

internal static class ControllerTestContext
{
    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["authentication:oidc:standard-claims:sub"] = "usuario_teste"
        })
        .Build();

    public static void Configure(ControllerBase controller)
    {
        var token = new JwtSecurityToken(claims: new[]
        {
            new Claim("usuario_teste", "b8f00faa-ae91-4fc9-a1cd-7fb12770c90a")
        });
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {new JwtSecurityTokenHandler().WriteToken(token)}";
        controller.ControllerContext = new ControllerContext { HttpContext = context };
    }
}
