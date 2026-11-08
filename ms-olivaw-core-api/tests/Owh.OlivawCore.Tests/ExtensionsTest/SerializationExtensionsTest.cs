using Owh.OlivawCore.Api.Extensions.Serializations;
using Owh.OlivawCore.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class SerializationExtensionsTest
{
    [Fact]
    public void Json_extensions_should_roundtrip_unicode_and_nullable_properties()
    {
        var source = new Payload { Name = "Ação", Count = 5 };
        var json = source.Serialize();
        Assert.Equal(json, source.SerializeObject());
        Assert.Equal(source.Name, json.Deserialize<Payload>().Name);
        Assert.Equal(5, json.ToObject<Payload>().Count);
        Assert.Null(json.Deserialize<Payload>().Optional);
        using var document = source.ToJsonDocument();
        Assert.Equal(source.Name, document.ToObject<Payload>().Name);
        Assert.Equal("Ação", Assert.IsType<JObject>(document.ToObject())["Name"]!.Value<string>());
        Assert.Equal(5, Assert.IsType<JObject>(json.ToObject())["Count"]!.Value<int>());
    }

    [Fact]
    public void Json_extensions_should_propagate_invalid_json()
    {
        Assert.Throws<JsonReaderException>(() => "invalid".Deserialize<Payload>());
        Assert.Throws<JsonReaderException>(() => "invalid".ToObject<Payload>());
    }

    [Fact]
    public void Json_array_extensions_should_find_matching_property_values()
    {
        using var document = JsonDocument.Parse("[{\"key\":\"alpha\",\"value\":1},{\"key\":\"beta\",\"value\":2}]");
        Assert.True(document.RootElement.EnumerateArray().Any("key", "bet"));
        Assert.False(document.RootElement.EnumerateArray().Any("key", "missing"));
        Assert.Equal(2, document.RootElement.EnumerateArray().FirstOrDefault("key", "beta", "value").GetInt32());
    }

    [Fact]
    public async Task ToFileStreamResult_should_preserve_bytes_media_type_and_filename()
    {
        var file = new ConteudoArquivoQueryResponse { Content = [1, 2, 3], ContentType = "application/pdf", FileName = "document.pdf" };
        var result = file.ToFileStreamResult();
        using var stream = result.FileStream;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, TestContext.Current.CancellationToken);
        Assert.Equal(file.Content, buffer.ToArray());
        Assert.Equal(file.ContentType, result.ContentType);
        Assert.Equal(file.FileName, result.FileDownloadName);
    }

    public class Payload
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public string? Optional { get; set; }
    }
}