using System;
using System.Text.Json;
using Newtonsoft.Json;
using Owh.Olivaw.Api.Extensions.Serializations;
using Xunit;

namespace Owh.Olivaw.Tests.ExtensionsTest.Serializations;

public class SystemTextJsonElementConverterTest
{
    [Theory]
    [InlineData("[{\"eventId\":\"known\",\"kind\":\"prepared\"}]")]
    [InlineData("{\"status\":\"found\",\"noEmailNoted\":false}")]
    [InlineData("null")]
    [InlineData("[1,true,\"texto\",null]")]
    public void Converter_should_preserve_raw_json_on_http_read_and_write(string json)
    {
        var settings = new JsonSerializerSettings { Converters = { new SystemTextJsonElementConverter() } };
        var value = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
        var serialized = JsonConvert.SerializeObject(value, settings);
        using var actual = JsonDocument.Parse(serialized);
        Assert.Equal(value.GetRawText(),actual.RootElement.GetRawText());
        Assert.DoesNotContain("ValueKind", serialized);
        var roundtrip = JsonConvert.DeserializeObject<JsonElement>(serialized, settings);
        Assert.Equal(value.GetRawText(),roundtrip.GetRawText());
    }
    [Fact]
    public void Converter_should_handle_nullable_null_and_undefined_without_invalid_json()
    {
        var settings = new JsonSerializerSettings { Converters = { new SystemTextJsonElementConverter() } };
        Assert.Null(JsonConvert.DeserializeObject<JsonElement?>("null",settings));
        Assert.Equal("null",JsonConvert.SerializeObject(default(JsonElement),settings));
        Assert.True(new SystemTextJsonElementConverter().CanConvert(typeof(JsonElement?)));
        Assert.False(new SystemTextJsonElementConverter().CanConvert(typeof(string)));
    }
}
