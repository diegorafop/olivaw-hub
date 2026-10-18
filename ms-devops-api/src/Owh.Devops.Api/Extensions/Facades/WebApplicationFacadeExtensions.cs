using Owh.OlivawCore.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Devops.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationFacadeExtensions
{
    public static void UseSetups(this WebApplication webApplication)
    {
        webApplication.Use(async (context, next) =>
        {
            try
            {
                await next();
                if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                    webApplication.Logger.LogWarning(
                        "HTTP {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
                        context.Response.StatusCode,
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
            }
            catch (Exception exception)
            {
                webApplication.Logger.LogError(exception,
                    "Unhandled API error for {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);
                throw;
            }
        });

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
