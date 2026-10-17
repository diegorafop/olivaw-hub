using Mps.OlivawCore.Services;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.Devops.Api.Features.ReadyFeature.Queries;
using Mps.OlivawCore.Domain.Enum;
using Mps.OlivawCore.Dados.Extensions;

namespace Mps.Devops.Api.Extensions.Https;

public static class ReadyExtensions
{
    public static string GetHealthStatus
    (
        this SelecionarReadyQueryResponse request
    )
    {
        var hasFail = request
            .Details
            .Select(item => item.Status)
            .Any(SearchPredicateHealthStatusFail());

        if (hasFail)
            return HttpRequestHelper.HealthStatusFail;

        return HttpRequestHelper.HealthStatusPass;
    }

    private static Func<string, bool> SearchPredicateHealthStatusFail()
    {
        return item => item.Equals
        (
            HttpRequestHelper.HealthStatusFail,
            StringComparison.OrdinalIgnoreCase
        );
    }

    public static IList<SelecionarReadyDetailQueryResponse>
        ToSelecionarReadyDetailQueryResponse
    (
        this List<Task<SelecionarReadyDetailQueryResponse>> itens
    )
    {
        return itens
            .Select(item => item.Result)
            .ToList();
    }

    public static SelecionarReadyQueryResponse
        ToSelecionarReadyQuery
    (
        this SelecionarReadyQuery request
    )
    {
        var response = request.Convert<SelecionarReadyQueryResponse>();

        response.Details = [];
        response.Status = HttpRequestHelper.HealthStatusPass;

        return response;
    }

    public static SelecionarReadyDetailQueryResponse
        ToSelecionarReadyDetailPostgresSqlQueryResponse
    (
        this SelecionarReadyQueryResponse request,
        bool health,
        IConfiguration configuration
    )
    {
        var resource = HealthCheckResourceEnum.Postgres;

        var address = configuration.GetDbContextHost();

        return request.ToSelecionarReadyDetailQueryResponse
        (
            GetHealthStatus(health),
            resource.GetService(),
            resource.GetComponentType(),
            address
        );
    }

    public static SelecionarReadyDetailQueryResponse
        ToSelecionarReadyDetailProviderAuthenticationQueryResponse
    (
        this SelecionarReadyQueryResponse request,
        bool health,
        IProviderAuthentication provider
    )
    {
        var resource = HealthCheckResourceEnum.AuthApi;

        return request.ToSelecionarReadyDetailQueryResponse
        (
            GetHealthStatus(health),
            resource.GetService(),
            resource.GetComponentType(),
            provider.BaseAddress
        );
    }

    private static string GetHealthStatus
    (
        bool health
    )
    {
        if (health)
            return HttpRequestHelper.HealthStatusPass;

        return HttpRequestHelper.HealthStatusFail;
    }

    public static SelecionarReadyDetailQueryResponse
        ToSelecionarReadyDetailQueryResponse
    (
        this SelecionarReadyQueryResponse request,
        string status,
        string service,
        string componentType,
        string address
    )
    {
        var response = EntityExtensions
            .Convert<SelecionarReadyQueryResponse,
                SelecionarReadyDetailQueryResponse>(request);

        response.Service = service;
        response.Status = status;
        response.ComponentType = componentType;
        response.Address = address;
        response.Output = GetOutPutByHealthStatus(status);
        response.Time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        return response;
    }

    private static string GetOutPutByHealthStatus
    (
        string healthStatus
    )
    {
        if (string.IsNullOrEmpty(healthStatus))
            return "unknow";

        if (healthStatus.Equals(HttpRequestHelper.HealthStatusPass))
            return "connection ok";

        if (healthStatus.Equals(HttpRequestHelper.HealthStatusFail))
            return "connection failure";

        if (healthStatus.Equals(HttpRequestHelper.HealthStatusWarn))
            return "connection warn";

        return "unknow";
    }

    public static string GetService
    (
        this HealthCheckResourceEnum healthCheckResourceEnum
    )
    {
        return healthCheckResourceEnum
            .ToString()
            .ToLower();
    }

    public static string GetComponentType
    (
        this HealthCheckResourceEnum value
    )
    {
        return value switch
        {
            HealthCheckResourceEnum.None => "unknown",
            HealthCheckResourceEnum.Postgres => "datastore",
            HealthCheckResourceEnum.AuthApi
                or HealthCheckResourceEnum.AdvogadoApi
                or HealthCheckResourceEnum.AuthApi
                or HealthCheckResourceEnum.BancoCentralBrasilApi
                or HealthCheckResourceEnum.DevopsApi
                or HealthCheckResourceEnum.DiarioJusticaEletronicoNacionalApi
                or HealthCheckResourceEnum.DocumentoApi
                or HealthCheckResourceEnum.EjudApi
                or HealthCheckResourceEnum.EjudTarefasApi
                or HealthCheckResourceEnum.FluxoApi
                or HealthCheckResourceEnum.FluxoEventoApi
                or HealthCheckResourceEnum.FluxoEventoApp
                or HealthCheckResourceEnum.GedApi
                or HealthCheckResourceEnum.GraphvizApi
                or HealthCheckResourceEnum.HistoricoApi
                or HealthCheckResourceEnum.OlivawCorePepApi
                or HealthCheckResourceEnum.OlivawCorePortalPrecatorioApi
                or HealthCheckResourceEnum.IntegracaoBanrisulApi
                or HealthCheckResourceEnum.IntegracaoBancoBrasilApi
                or HealthCheckResourceEnum.IntegracaoErpMpsApp
                or HealthCheckResourceEnum.IntegracaoSistemaOriginarioApi
                or HealthCheckResourceEnum.IntegracaoTrf5Api
                or HealthCheckResourceEnum.LocalizacaoApi
                or HealthCheckResourceEnum.ModeloTextoApi
                or HealthCheckResourceEnum.MonitorServidorEmailApi
                or HealthCheckResourceEnum.NotificacaoApi
                or HealthCheckResourceEnum.NotificacaoApp
                or HealthCheckResourceEnum.OficioRequisitorioApi
                or HealthCheckResourceEnum.PdfApi
                or HealthCheckResourceEnum.PessoaApi
                or HealthCheckResourceEnum.PeticaoApi
                or HealthCheckResourceEnum.PrecatorioTarefasApi
                or HealthCheckResourceEnum.QuestionarioApi
                or HealthCheckResourceEnum.ReceitaFederalApi
                or HealthCheckResourceEnum.RelatorioApi
                or HealthCheckResourceEnum.SefazApi
                or HealthCheckResourceEnum.SegurancaApi
                or HealthCheckResourceEnum.TarefasApp
                or HealthCheckResourceEnum.TesseractApi
                or HealthCheckResourceEnum.UsuarioApi => "restful",

            HealthCheckResourceEnum.OlivawCoreAssinadorWpf
                or HealthCheckResourceEnum.IntegracaoOfficeDesktopApp => "microsoft-store-app",
            HealthCheckResourceEnum.OlivawCoreNg
                or HealthCheckResourceEnum.OlivawCorePepNg
                or HealthCheckResourceEnum.OlivawCorePortalPrecatorioNg => "frontend-angular",
            HealthCheckResourceEnum.ParametroTextoOfficeApp => "frontend-office-plugin",
            _ => "unknown",
        };
    }
}