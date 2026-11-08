using Owh.OlivawCore.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Olivaw.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationFacadeExtensions
{
    public static void UseSetups(this WebApplication webApplication)
    {
        if (webApplication.Environment.IsDevelopment())
            webApplication.UseDeveloperExceptionPage();

        webApplication.UseHttpMetricsWhen();
        webApplication.UseSwaggerCore();
        webApplication.UseRoutingCore();
        webApplication.UseSecurityCore();
        webApplication.UseRateLimiter();
        webApplication.UseRequestLocalizationCore();
        webApplication.UseMetrics();
        webApplication.UseHttpLoggingByServiceLoki();
    }
}
