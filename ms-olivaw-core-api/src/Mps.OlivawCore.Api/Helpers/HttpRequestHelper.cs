namespace Mps.OlivawCore.Api.Helpers;

public static class HttpRequestHelper
{
    public static readonly string HealthStatusPass = "pass";
    public static readonly string HealthStatusFail = "fail";
    public static readonly string HealthStatusWarn = "warn";

    private static readonly int _addedValue = 2;
    private static readonly int _maxRequestQueryStringLength = 1000;

    public static async Task<IEnumerable<T>> PerformBlockRequestAsync<F, T>
    (
        IEnumerable<F> list,
        int maxRequestItemCount,
        Func<IEnumerable<F>, Task<IEnumerable<T>>> func
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRequestItemCount);

        var response = new List<T>();

        for (int i = 0; i < list.Count(); i += maxRequestItemCount)
        {
            var block = list.Skip(i).Take(maxRequestItemCount);
            var responseBlock = await func(block);
            response = response.Concat(responseBlock).ToList();
        }

        return response;
    }

    public static int GetMaxRequestItemCount
    (
        string nomeCampo,
        int valueMaxLength,
        float fator
    )
    {
        return GetMaxListLengthTipoListaQueryStringNomeRepetido
        (
            nomeCampo,
            valueMaxLength,
            fator
        );
    }

    private static int GetMaxListLength
    (
        Func<int, int> func
    )
    {
        int maxListLength;
        for (maxListLength = 0; func(maxListLength) < _maxRequestQueryStringLength; maxListLength += _addedValue) { }
        return maxListLength - _addedValue;
    }

    private static int GetMaxListLengthTipoListaQueryStringNomeRepetido
    (
        string nomeCampo,
        int valueMaxLength,
        float fator
    )
    {
        var maxListLength = GetMaxListLength
        (
            item => nomeCampo.Length * item + valueMaxLength * item + 2 * item - 1
        );

        return (int)Math.Round(maxListLength * fator);
    }
}