using Newtonsoft.Json;

namespace Mps.OlivawCore.Api.Extensions.Serializations;

public static class JsonConvertExtensions
{
    public static string Serialize
    (
        this object item
    )
    {
        return JsonConvert.SerializeObject(item);
    }

    public static T Deserialize<T>
    (
        this string content
    )
    {
        return JsonConvert.DeserializeObject<T>(content);
    }
}