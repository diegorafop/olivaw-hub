using Mps.Ltmj.Services;
using Mps.Ltmj.Api.Settings;
using Microsoft.Extensions.Options;

namespace Mps.Ltmj.Api.Services;

public sealed class LocalPhotoStorage(IOptions<PortfolioSettings> settings) : IPhotoStorage
{
    private string PathFor(string key)
    {
        if (System.IO.Path.GetFileName(key) != key) throw new ArgumentException("Chave inválida.", nameof(key));
        return System.IO.Path.Combine(settings.Value.StoragePath, key);
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(settings.Value.StoragePath);
        var key = $"{Guid.NewGuid():N}.{extension}";
        try
        {
            await using var output = new FileStream(PathFor(key), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await content.CopyToAsync(output, cancellationToken);
            return key;
        }
        catch
        {
            File.Delete(PathFor(key));
            throw;
        }
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(PathFor(key));
        return Task.CompletedTask;
    }

    public Task<Stream> OpenAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<Stream>(new FileStream(PathFor(key), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true));
    }
}
