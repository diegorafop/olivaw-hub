using Mps.OlivawCore.Api.Extensions.Collections;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class EnumerableExtensionsTest
{
    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(5, 1)]
    [InlineData(10, 1)]
    public void GetManyByBatch_should_preserve_order_without_losing_items(int size, int expectedCount)
    {
        var source = Enumerable.Range(1, 5).ToArray();
        var batches = source.GetManyByBatch(size);
        Assert.Equal(expectedCount, batches.Count);
        Assert.Equal(source, batches.SelectMany(item => item));
        Assert.All(batches, item => Assert.InRange(item.Count, 1, size));
    }

    [Fact]
    public void GetManyByBatch_should_accept_empty_input()
    {
        Assert.Empty(Array.Empty<int>().GetManyByBatch(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetManyByBatch_should_reject_invalid_batch_size(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<int>().GetManyByBatch(size));
    }
}