using Mps.Auth.Api.Extensions.Https;
using Mps.Auth.Requests;
using Mps.HolosCore.Api.Extensions.Https;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Mps.Auth.Api.Extensions.Https;

public static class HttpRequestExtensions
{
    public static string ToQueryString
    (
        this object? request
    )
    {
        if (request is null)
            return string.Empty;

        var properties = request
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var queryParameters = new List<string>();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);

            if (value is null)
                continue;

            if (IsPropertyValuePrimitiveOrEnum(value))
                queryParameters.Add(GetQueryParameter(property.Name, value));
            else if (value is IEnumerable enumerable)
            {
                var index = 0;
                foreach (var item in enumerable)
                    if (IsPropertyValuePrimitiveOrEnum(item))
                    {
                        queryParameters.Add(GetQueryParameter($"{property.Name}[{index}]", item));
                        index++;
                    }
            }
            else
                queryParameters.AddRange
                (
                    from itemProperty in value.GetType().GetProperties()
                    let itemValue = itemProperty.GetValue(value)
                    where IsPropertyValuePrimitiveOrEnum(itemValue)
                    select GetQueryParameter($"{property.Name}.{itemProperty.Name}", itemValue!)
                );
        }

        return string.Join("&", queryParameters);
    }

    private static string GetQueryParameter
    (
        string key,
        object value
    )
    {
        return $"{key}={EncodeValue(value)}";
    }

    private static string EncodeValue(object value)
    {
        var consideredValue = value.ToString()!;

        var type = value.GetType();

        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);

            if (underlyingType == typeof(int))
                consideredValue = ((int)value).ToString();
        }
        else switch (value)
            {
                case DateTime dateTime:
                    return dateTime.ToString();
                case float floatValue:
                    consideredValue = floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case double doubleValue:
                    consideredValue = doubleValue.ToString(CultureInfo.InvariantCulture);
                    break;
            }

        return Uri.EscapeDataString(consideredValue);
    }

    private static bool IsPropertyValuePrimitiveOrEnum
    (
        object? value
    )
    {
        if (value is null)
            return false;

        var type = value.GetType();

        return IsPrimitiveOrEnum(type);
    }

    private static bool IsPrimitiveOrEnum(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }

    public static string ToSelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyRoute
    (
        this SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery request,
        string baseRoute
    )
    {
        var queryItems = new List<string>();

        if (request.UsuarioIdMany is not null)
            queryItems.Add(request
                .UsuarioIdMany
                .ToQueryString("UsuarioIdMany"));

        return $"{baseRoute}?{string.Join("&", queryItems)}";
    }
}