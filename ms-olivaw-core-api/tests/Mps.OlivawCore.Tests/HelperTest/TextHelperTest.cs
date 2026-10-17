using Mps.OlivawCore.Api.Helpers;
using System.Text;
using Xunit;

namespace Mps.OlivawCore.Tests.HelperTest;

public class TextHelperTest
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("CPF: 123.456-78", "12345678")]
    [InlineData("abc", "")]
    public void OnlyDigit_should_keep_only_digits(string? input, string expected)
    {
        Assert.Equal(expected, TextHelper.OnlyDigit(input!));
    }

    [Fact]
    public void ConvertToStringHtml_should_accept_empty_content()
    {
        Assert.Equal(string.Empty, TextHelper.ConvertToStringHtml(null));
        Assert.Equal(string.Empty, TextHelper.ConvertToStringHtml([]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConvertToStringHtml_should_decode_utf8_without_returning_bom(bool withBom)
    {
        const string html = "<p>Ação e informação</p>";
        byte[] content = Encoding.UTF8.GetBytes(html);
        if (withBom)
            content = [0xEF, 0xBB, 0xBF, .. content];

        Assert.Equal(html, TextHelper.ConvertToStringHtml(content));
    }

    [Fact]
    public void ConvertToStringHtml_should_decode_latin1_when_content_is_not_utf8()
    {
        const string html = "<p>Ação</p>";
        Assert.Equal(html, TextHelper.ConvertToStringHtml(Encoding.Latin1.GetBytes(html)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("20230229")]
    [InlineData("2024-02-29")]
    public void GetDateTimeUtc_should_reject_invalid_dates(string? input)
    {
        Assert.Null(TextHelper.GetDateTimeUtc(input!));
    }

    [Fact]
    public void GetDateTimeUtc_should_parse_leap_day_as_utc()
    {
        var result = TextHelper.GetDateTimeUtc("20240229");
        Assert.Equal(new DateTime(2024, 2, 29, 0, 0, 0, DateTimeKind.Utc), result);
        Assert.Equal(DateTimeKind.Utc, result!.Value.Kind);
    }
}
