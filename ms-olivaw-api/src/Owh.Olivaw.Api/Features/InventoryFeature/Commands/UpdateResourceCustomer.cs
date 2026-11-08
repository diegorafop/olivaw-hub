using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class UpdateResourceCustomerMediatorCommand : IRequest<UpdateResourceCustomerResult>
{
    public Guid ExternalId { get; set; }
    public UpdateResourceCustomerCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateResourceCustomerResult { public string? Conflict { get; set; } public UpdateResourceCustomerResponse Response { get; set; } = new(); }
public sealed class UpdateResourceCustomerHandler(IRepository<ResourceCustomer> repository, IRepositoryInventoryRevision inventory) : IRequestHandler<UpdateResourceCustomerMediatorCommand, UpdateResourceCustomerResult>
{
    public async Task<UpdateResourceCustomerResult> Handle(UpdateResourceCustomerMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var link = await InventoryRules.RequireAsync(repository, request.ExternalId, cancellationToken);
        if (link.Revision != request.Payload.Revision) return new() { Conflict = "Este vínculo mudou. Atualize a ficha." };
        link.Close(request.ActorUserId, request.ActorName, now);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Atualize a ficha." };
        return new() { Response = new() { ExternalId = link.ExternalId, Revision = link.Revision } };
    }
}
public sealed class UpdateResourceCustomerValidator : AbstractValidator<UpdateResourceCustomerCommand>
{
    public UpdateResourceCustomerValidator() { RuleFor(item => item.Revision).GreaterThan(0); }
}
public sealed class UpdateResourceCustomerMediatorValidator : AbstractValidator<UpdateResourceCustomerMediatorCommand>
{
    public UpdateResourceCustomerMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateResourceCustomerValidator()).OverridePropertyName(string.Empty);
    }
}
