using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Services;
using Mps.OlivawCore.Repositories;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

public sealed class UploadEventPhotosCommand : IRequest<UploadEventPhotosResponse>
{
    public Guid Id { get; set; }
    public List<IFormFile> Files { get; set; } = [];
    internal Guid UserId { get; set; }
}
public sealed class UploadEventPhotosResponse { public int Count { get; set; } }

public sealed class UploadEventPhotosHandler(IRepository<PortfolioEvent> repository, IPhotoStorage storage)
    : IRequestHandler<UploadEventPhotosCommand, UploadEventPhotosResponse>
{
    public async Task<UploadEventPhotosResponse> Handle(UploadEventPhotosCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await repository.GetFirstAsync(item => item.ExternalId == request.Id, cancellationToken);
        if (item.Photos.Count + request.Files.Count > 100)
            throw new ValidationException("Cada evento pode ter até 100 fotos.");
        // Valida o lote completo antes de gravar qualquer arquivo.
        var formats = new List<(string Extension, string ContentType)>();
        foreach (var file in request.Files)
        {
            await using var stream = file.OpenReadStream();
            var header = new byte[12];
            var count = await stream.ReadAtLeastAsync(header, 12, false, cancellationToken);
            formats.Add(DetectFormat(header, count));
        }
        var added = new List<PortfolioPhoto>();
        try
        {
            for (var index = 0; index < request.Files.Count; index++)
            {
                await using var stream = request.Files[index].OpenReadStream();
                var key = await storage.SaveAsync(stream, formats[index].Extension, cancellationToken);
                added.Add(new PortfolioPhoto
                {
                    ExternalId = Guid.NewGuid(),
                    UsuarioLogadoId = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    PortfolioEventId = item.Id,
                    PortfolioEvent = item,
                    StorageKey = key,
                    ContentType = formats[index].ContentType,
                    Position = item.Photos.Count + index
                });
            }
            item.Photos.AddRange(added);
            item.Update(item.Title, item.Description, item.Category, request.UserId);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var photo in added) await storage.DeleteAsync(photo.StorageKey, CancellationToken.None);
            throw;
        }
        return new UploadEventPhotosResponse { Count = added.Count };
    }

    private static (string, string) DetectFormat(byte[] header, int count)
    {
        if (count < 12) throw new ValidationException("Imagem inválida.");
        if (header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff) return ("jpg", "image/jpeg");
        if (header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ("png", "image/png");
        if (System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP") return ("webp", "image/webp");
        throw new ValidationException("Envie imagens JPEG, PNG ou WebP.");
    }
}
public sealed class UploadEventPhotosValidator : AbstractValidator<UploadEventPhotosCommand>
{
    public UploadEventPhotosValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Files).NotEmpty().Must(files => files.Count <= 20)
            .WithMessage("Envie até 20 fotos por lote.");
        RuleForEach(item => item.Files).Must(file => file.Length > 0 && file.Length <= 10 * 1024 * 1024)
            .WithMessage("Cada foto deve ter até 10 MB.");
        RuleFor(item => item.Files).Must(files => files.Sum(file => file.Length) <= 100 * 1024 * 1024)
            .WithMessage("O lote deve ter até 100 MB.");
    }
}
