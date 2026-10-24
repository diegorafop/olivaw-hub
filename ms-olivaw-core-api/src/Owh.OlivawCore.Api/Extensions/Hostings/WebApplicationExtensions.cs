using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Owh.OlivawCore.Api.Extensions.Https;
using Prometheus;
using System.Diagnostics.CodeAnalysis;

namespace Owh.OlivawCore.Api.Extensions.Hostings;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static void UseRequestLocalizationCore
    (
        this WebApplication webApplication
    )
    {
        webApplication.UseRequestLocalization(options =>
        {
            options
                .AddSupportedCultures("pt-BR")
                .AddSupportedUICultures("pt-BR")
                .SetDefaultCulture("pt-BR");
        });
    }

    public static void UseSecurityCore
    (
        this WebApplication webApplication
    )
    {
        webApplication.UseCors();
        webApplication.UseAuthentication();
        webApplication.UseAuthorization();
    }

    public static void UseRoutingCore
    (
        this WebApplication webApplication
    )
    {
        webApplication.UseRouting();

        webApplication
            .MapControllers()
            .RequireAuthorization();

        webApplication.UseHealthChecks("/health");
    }

    public static void UseSwaggerCore
    (
        this WebApplication webApplication
    )
    {
        if (webApplication.ShouldUseSwagger())
        {
            webApplication.UseSwaggerDocumentation();

            webApplication.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint
                (
                    "/openapi/v1.json",
                     "OlivawCore V2 API"
                );

                options.RoutePrefix = "swagger";
            });
        }
    }

    private static bool ShouldUseSwagger
    (
        this WebApplication webApplication
    )
    {
        if (webApplication
            .Environment
            .IsDevelopment()
        )
            return true;

        var swaggerEnabled = webApplication
            .Configuration
            .GetValue<bool>("swagger:enabled");

        return swaggerEnabled;
    }

    public static void UseHttpMetricsWhen
    (
        this WebApplication webApplication
    )
    {
        webApplication.UseWhen
        (
            context => context.ShouldMapRequestMetrics(),
            appBuilder =>
            {
                appBuilder.UseHttpMetrics();
            }
        );
    }

    public static void MetricsCreateGaugeVersion
    (
        this WebApplication webApplication
    )
    {
        var response = webApplication
            .Configuration
            .GetReadVersionQueryResponse();

        var versionGauge = Metrics.CreateGauge
        (
            "ms_version_info",
            "Informações de versão do serviço.",
            new GaugeConfiguration
            {
                LabelNames =
                [
                    "versao",
                    "titulo",
                    "ultima_atualizacao"
                ]
            });

        versionGauge
            .WithLabels
            (
                response.Versao,
                response.Titulo,
                response.UltimaAtualizacao
            )
            .Set(1);
    }

    public static void UseHttpLoggingByServiceLoki
    (
        this WebApplication webApplication
    )
    {
        var configured = webApplication
            .Configuration
            .GetValue<bool>("services:loki:configured");

        if (configured)
            webApplication.UseHttpLogging();
    }

    public static void UseMetrics
    (
        this WebApplication webApplication
    )
    {
        var configured = webApplication
            .Configuration
            .GetValueByKey<bool>("services:loki:configured");

        if (!configured)
            return;

        webApplication.MetricsCreateGaugeVersion();
        webApplication.MapMetrics();
    }
}