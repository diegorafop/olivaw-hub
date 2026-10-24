using FluentValidation;
using Owh.OlivawCore.Api.Extensions.Serializations;
using Owh.OlivawCore.Api.Helpers;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Owh.OlivawCore.Api.Validation;

public class ValidationErrorResponse
{
    public string Title { get; set; }
    public object Errors { get; set; }

    public HttpStatusCode Status { get; set; }
}

public class ServiceResponseValidator
    : AbstractValidator<ValidationErrorResponse>
{
    public ServiceResponseValidator()
    {
        RuleFor(item => item.Status)
            .Must(status => (int)status is >= (int)HttpStatusCode.OK and < (int)HttpStatusCode.MultipleChoices)
            .WithMessage(item => MessageHelper
                .InvalidFor(item, value => value.Status));

        RuleFor(item => item.Errors)
            .Must(item => string.IsNullOrEmpty(GetOneErrorMessage(item)))
            .WithMessage(item => MessageHelper
                .ServiceErrorFor(item, value => value.Errors));
    }

    public async Task ValidateResponseAsync
    (
        HttpResponseMessage httpResponseMessage,
        string content,
        CancellationToken cancellationToken
    )
    {
        ValidationErrorResponse response = GetValidatonErrorResponse
        (
            httpResponseMessage.StatusCode,
            content
        );

        await this.ValidateAndThrowAsync
        (
            response,
            cancellationToken
        );
    }

    private static string GetOneErrorMessage
    (
        object? errors
    )
    {
        if (errors is null)
            return string.Empty;

        if (errors is string[] itens)
            return string.Join(", ", itens);

        if (errors is string item)
            return item;

        if (errors is JArray array)
            return string.Join(", ", array.Values<string>());

        return string.Empty;
    }

    private static ValidationErrorResponse GetValidatonErrorResponse
    (
        HttpStatusCode statusCode,
        string content
    )
    {
        if (Unauthorized(statusCode))
            return GetValidationErrorResponseByUnauthorized(statusCode);

        if (!CanParseJson(content))
            return GetInternalServerError
            (
                statusCode,
                content
            );

        ValidationErrorResponse response = content.Deserialize<ValidationErrorResponse>();
        response.Status = statusCode;
        return response;
    }

    private static bool Unauthorized
    (
        HttpStatusCode statusCode
    )
    {
        return statusCode.Equals(HttpStatusCode.Unauthorized);
    }

    private static bool NotFound
    (
        HttpStatusCode statusCode
    )
    {
        return statusCode.Equals(HttpStatusCode.NotFound);
    }

    private static bool CanParseJson
    (
        string content
    )
    {
        if (string.IsNullOrEmpty(content))
            return false;

        try
        {
            var json = JToken.Parse(content);

            if (json is not JObject jObject)
                return false;

            string[] errorKeys = ["Errors", "errors"];

            return jObject
                .Properties()
                .Any(item => errorKeys.Contains
                (
                    item.Name,
                    StringComparer.OrdinalIgnoreCase
                ));
        }
        catch
        {
            return false;
        }
    }

    private static ValidationErrorResponse GetInternalServerError
    (
        HttpStatusCode statusCode,
        string content
    )
    {
        return new ValidationErrorResponse
        {
            Title = "internal server error",
            Status = statusCode,
            Errors = GetOneError(statusCode, content)
        };
    }

    private static ValidationErrorResponse GetValidationErrorResponseByUnauthorized
    (
        HttpStatusCode statusCode
    )
    {
        return new ValidationErrorResponse
        {
            Title = "não autorizado",
            Status = HttpStatusCode.Unauthorized,
            Errors = GetOneError(statusCode, "unauthorized")
        };
    }

    private static string GetOneError
    (
        HttpStatusCode statusCode,
        string content
    )
    {
        if (string.IsNullOrEmpty(content))
            return "empty";

        if (NotFound(statusCode))
            return GetOneErrorMessageByNotFound(content);

        return content;
    }

    private static string GetOneErrorMessageByNotFound
    (
        string content
    )
    {
        if (HasNotFound(content))
            return content;

        return $"não encontrado : {content}";
    }

    private static bool HasNotFound
    (
        string content
    )
    {
        return content.Contains
        (
            "não encontrado",
            StringComparison.CurrentCultureIgnoreCase
        );
    }
}