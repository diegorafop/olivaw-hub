using System.Globalization;
using System.Text;

namespace Mps.OlivawCore.Api.Helpers;

public static class TextHelper
{
    private static readonly Encoding Utf8Strict = new UTF8Encoding
    (
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true
    );

    private static readonly Encoding Latin1 = Encoding.GetEncoding("iso-8859-1");

    public static string OnlyDigit
    (
        string texto
    )
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        return new string([.. texto.Where(char.IsDigit)]);
    }

    public static string ConvertToStringHtml
    (
        byte[]? content
    )
    {
        if (content is null)
            return string.Empty;

        if (content.Length == 0)
            return string.Empty;

        ReadOnlySpan<byte> span = content;

        if (span.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }))
            return Encoding.UTF8.GetString(span[3..]);

        return GetStringUtf8Strict(content);
    }

    private static string GetStringUtf8Strict
    (
        byte[] content
    )
    {
        try
        {
            return Utf8Strict.GetString(content);
        }
        catch (DecoderFallbackException)
        {
            return Latin1.GetString(content);
        }
    }

    public static DateTime? GetDateTimeUtc
    (
        string dateTime
    )
    {
        var isDate = DateTime.TryParseExact
        (
            dateTime,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date
        );

        if (!isDate)
            return null;

        return DateTime.SpecifyKind
        (
            date,
            DateTimeKind.Utc
        );
    }

    public static string FirstCharToUpper
    (
        string input
    )
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} não pode ser vazio", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
    }

    public static int GetGuidLenght()
    {
        return Guid
            .Empty
            .ToString()
            .Replace("-", string.Empty)
            .Length;
    }
}