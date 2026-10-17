
using Mps.Auth.Requests;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Extensions.Entities;

public static class UsuarioCargoExtensions
{
    public static string GetRoute
    (
        this SelecionarUsuarioCargoByFilterQuery request,
        string route
    )
    {
        var queryItems = new List<string>();

        if (request.UsuarioIds is not null)
            queryItems.Add(request
                .UsuarioIds
                .ToQueryString("UsuarioIds"));

        if (request.CargoIds is not null)
            queryItems.Add(request
                .CargoIds
                .ToQueryString("CargoIds"));

        if (request.Gestor is not null)
            queryItems.Add($"gestor={request.Gestor}");

        return $"{route}?{string.Join("&", queryItems)}";
    }

    public static SelecionarUsuarioCargoByFilterQueryResponse GetUsuarioCargoByUsuarioId
    (
        this IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse> itens,
        Guid usuarioId
    )
    {
        return itens.FirstOrDefault
        (
            item => item
                .UsuarioId
                .Equals(usuarioId)
        ) ?? new();
    }
}