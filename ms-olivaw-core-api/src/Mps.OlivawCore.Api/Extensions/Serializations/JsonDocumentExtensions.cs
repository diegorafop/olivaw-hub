using Mps.OlivawCore.Api.Extensions.Serializations;
using System.Text.Json;

namespace Mps.OlivawCore.Api.Extensions.Serializations;

public static class JsonDocumentExtensions
{
    public static JsonDocument ToJsonDocument
    (
        this object conteudo
    )
    {
        var json = Newtonsoft
            .Json
            .JsonConvert
            .SerializeObject(conteudo);

        return JsonDocument.Parse
        (
            json,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            }
        );
    }

    public static string SerializeObject
    (
        this object conteudo
    )
    {
        return Newtonsoft
            .Json
            .JsonConvert
            .SerializeObject(conteudo);
    }

    public static object ToObject
    (
        this JsonDocument jsonDocument
    )
    {
        var json = jsonDocument
            .RootElement
            .GetRawText();

        return Newtonsoft
            .Json
            .JsonConvert
            .DeserializeObject(json);
    }

    public static object ToObject
    (
        this string json
    )
    {
        return Newtonsoft
            .Json
            .JsonConvert
            .DeserializeObject(json);
    }

    public static T ToObject<T>
    (
        this string json
    )
    {
        return Newtonsoft
            .Json
            .JsonConvert
            .DeserializeObject<T>(json);
    }

    public static T ToObject<T>
    (
        this JsonDocument jsonDocument
    )
    {
        var json = jsonDocument
            .RootElement
            .GetRawText();

        return Newtonsoft
            .Json
            .JsonConvert
            .DeserializeObject<T>(json);
    }

    public static JsonElement FirstOrDefault
    (
        this JsonElement.ArrayEnumerator propertyMany,
        string propertyKey,
        string propertyValue,
        string propertyName
    )
    {
        return propertyMany
            .FirstOrDefault(item => item
                .GetProperty(propertyKey)
                .GetString()
                !.Contains(propertyValue))
            .GetProperty(propertyName);
    }

    public static bool Any
    (
        this JsonElement.ArrayEnumerator propertyMany,
        string propertyKey,
        string propertyValue
    )
    {
        return propertyMany
            .Any(item => item
                .GetProperty(propertyKey)
                .GetString()
                !.Contains(propertyValue));
    }
}