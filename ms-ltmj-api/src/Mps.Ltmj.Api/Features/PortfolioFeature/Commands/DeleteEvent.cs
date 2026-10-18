using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.OlivawCore.Repositories;
using Mps.OlivawCore.Api.Helpers;

using Mps.Ltmj.Portfolio;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

public sealed class DeleteEventCommand : IRequest<DeleteEventResponse>
{
    public Guid Id { get; set; }
    internal Guid UserId { get; set; }
}
public sealed class DeleteEventResponse { }

public sealed class DeleteEventHandler(IRepository<PortfolioEvent> repository, IPhotoStorage storage)
    : IRequestHandler<DeleteEventCommand, DeleteEventResponse>
{
    public async Task<DeleteEventResponse> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await repository.GetFirstAsync(item => item.IdExterno == request.Id, cancellationToken);
        // Oculta antes da limpeza para não expor uma galeria parcialmente removida.
        item.SetVisibility(false, request.UserId);
        await repository.UpdateAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        // Se a limpeza falhar, o registro oculto permite repetir a exclusão.
        foreach (var photo in item.Photos) await storage.DeleteAsync(photo.StorageKey, cancellationToken);
        await repository.RemoveAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new DeleteEventResponse();
    }
}
public sealed class DeleteEventValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
