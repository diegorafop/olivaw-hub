using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Owh.Ltmj.Services;

public interface IPhotoStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);
    Task DeleteAsync(string key, CancellationToken cancellationToken);
    Task<Stream> OpenAsync(string key, CancellationToken cancellationToken);
}
