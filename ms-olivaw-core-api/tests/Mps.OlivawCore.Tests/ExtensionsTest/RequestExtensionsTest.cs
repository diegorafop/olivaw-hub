using Mps.OlivawCore.Api.Extensions.Https;
using System.Globalization;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class RequestExtensionsTest
{
    [Fact]
    public void ToQueryString_should_preserve_the_existing_date_format()
    {
        var date = new DateTime(2024, 2, 29, 10, 30, 0, DateTimeKind.Utc);
        Assert.Equal($"Date={date}", new { Date = date }.ToQueryString());
    }

    [Fact]
    public void ToQueryString_should_ignore_nulls_and_encode_scalar_values()
    {
        var request = new { Name = "João & Maria", Count = 3, Missing = (string?)null, Status = DayOfWeek.Monday };
        Assert.Equal("Name=Jo%C3%A3o%20%26%20Maria&Count=3&Status=1", request.ToQueryString());
        Assert.Equal(string.Empty, ((object?)null).ToQueryString());
    }

    [Fact]
    public void ToQueryString_should_flatten_nested_properties_and_index_primitive_collections()
    {
        var request = new
        {
            Filter = new { Name = "a b", Missing = (string?)null, Child = new { Id = 1 } },
            Values = new object?[] { 10, null, new { Id = 20 }, "value" }
        };
        Assert.Equal("Filter.Name=a%20b&Values[0]=10&Values[1]=value", request.ToQueryString());
    }

    [Fact]
    public void ToQueryString_should_format_floating_point_values_independently_of_culture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            Assert.Equal("Float=1.5&Double=2.75", new { Float = 1.5f, Double = 2.75 }.ToQueryString());
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ToQueryString_should_repeat_the_name_for_lists_and_format_guids_without_hyphens()
    {
        Guid[] ids = [Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), Guid.Empty];
        Assert.Equal("ids=aaaaaaaabbbbccccddddeeeeeeeeeeee&ids=00000000000000000000000000000000", ids.ToQueryString("ids"));
        Assert.Equal("ids=1&ids=2", new[] { 1, 2 }.ToQueryString("ids"));
        Assert.Equal(string.Empty, Array.Empty<int>().ToQueryString("ids"));
    }
}