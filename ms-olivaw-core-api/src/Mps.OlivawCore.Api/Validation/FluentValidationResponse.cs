using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Mps.OlivawCore.Api.Validation;

public class FluentValidationResponse
{
    public FluentValidationResponse
    (
        ValidationException exception
    )
    {
        Status = GetStatus(exception);
        Title = GetTitle();
    }

    public string Title { get; set; }
    public int Status { get; set; }

    private static int GetStatus
    (
        ValidationException exception
    )
    {
        var message = exception
            .Message
            .ToLower();

        if (message.Contains("não encontrado"))
            return StatusCodes.Status404NotFound;

        if (Unauthorized(message))
            return StatusCodes.Status401Unauthorized;

        if (message.Contains("já foi"))
            return StatusCodes.Status409Conflict;

        if (message.Contains("expirado"))
            return StatusCodes.Status410Gone;

        return StatusCodes.Status400BadRequest;
    }

    private static bool Unauthorized
    (
        string message
    )
    {
        if (message.Contains("usuário e/ou senha são inválidos"))
            return true;

        return message.Contains("não autorizado");
    }

    private string GetTitle()
    {
        return Status switch
        {
            StatusCodes.Status404NotFound => "Objeto não encontrado",
            StatusCodes.Status401Unauthorized => "Não autorizado",
            StatusCodes.Status409Conflict => "Conflito",
            StatusCodes.Status410Gone => "Expirado",
            _ => "Operação inválida",
        };
    }
}