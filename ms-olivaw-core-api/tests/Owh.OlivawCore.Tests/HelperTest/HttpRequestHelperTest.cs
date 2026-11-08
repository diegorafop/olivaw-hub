using Owh.OlivawCore.Api.Helpers;
using Xunit;

namespace Owh.OlivawCore.Tests.HelperTest;

public class HttpRequestHelperTest
{
    [Fact]
    public async Task PerformBlockRequestAsync_should_send_each_batch_and_combine_results_in_order()
    {
        var batches = new List<int[]>();
        var result = await HttpRequestHelper.PerformBlockRequestAsync<int, string>(
            [1, 2, 3, 4, 5], 2, batch =>
            {
                batches.Add(batch.ToArray());
                return Task.FromResult(batch.Select(item => $"result-{item}"));
            });

        Assert.Equal(new[] { 2, 2, 1 }, batches.Select(item => item.Length));
        Assert.Equal(new[] { "result-1", "result-2", "result-3", "result-4", "result-5" }, result);
    }

    [Fact]
    public async Task PerformBlockRequestAsync_should_not_call_service_for_empty_input()
    {
        var result = await HttpRequestHelper.PerformBlockRequestAsync<int, int>([], 2,
            batch => throw new InvalidOperationException("Should not be called"));
        Assert.Empty(result);
    }

    [Fact]
    public async Task PerformBlockRequestAsync_should_propagate_service_failure()
    {
        var expected = new HttpRequestException("service unavailable");
        var actual = await Assert.ThrowsAsync<HttpRequestException>(() =>
            HttpRequestHelper.PerformBlockRequestAsync<int, int>([1], 1,
                batch => Task.FromException<IEnumerable<int>>(expected)));
        Assert.Same(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PerformBlockRequestAsync_should_reject_invalid_batch_size(int batchSize)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            HttpRequestHelper.PerformBlockRequestAsync<int, int>([], batchSize,
                batch => Task.FromResult(batch)));
    }

    [Theory]
    [InlineData(1, 24)]
    [InlineData(0.5f, 12)]
    public void GetMaxRequestItemCount_should_respect_query_length_and_factor(float factor, int expected)
    {
        Assert.Equal(expected, HttpRequestHelper.GetMaxRequestItemCount("ids", 36, factor));
    }
}
