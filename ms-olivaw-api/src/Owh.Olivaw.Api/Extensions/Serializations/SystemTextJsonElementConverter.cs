using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Owh.Olivaw.Api.Extensions.Serializations;

// O MVC usa Newtonsoft, mas os snapshots evolutivos do cliente usam System.Text.Json.
public sealed class SystemTextJsonElementConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(JsonElement) || objectType == typeof(JsonElement?);

    public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        if (value is not JsonElement element || element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            writer.WriteNull();
            return;
        }
        JToken.Parse(element.GetRawText()).WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null && objectType == typeof(JsonElement?)) return null;
        var token = JToken.Load(reader);
        using var document = JsonDocument.Parse(token.ToString(Formatting.None));
        return document.RootElement.Clone();
    }
}
