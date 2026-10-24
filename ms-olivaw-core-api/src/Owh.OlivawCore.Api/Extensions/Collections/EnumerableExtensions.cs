namespace Owh.OlivawCore.Api.Extensions.Collections;

public static class EnumerableExtensions
{
    public static List<List<T>> GetManyByBatch<T>
    (
        this IEnumerable<T> enumerable,
        int batchSize
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var itens = new List<List<T>>();

        for (int i = 0; i < enumerable.Count(); i += batchSize)
        {
            var block = enumerable
                .Skip(i)
                .Take(batchSize)
                .ToList();

            itens.Add(block);
        }

        return itens;
    }
}