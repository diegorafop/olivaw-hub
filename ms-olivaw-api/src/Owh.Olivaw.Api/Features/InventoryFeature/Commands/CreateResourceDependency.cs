using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class CreateResourceDependencyMediatorCommand : IRequest<CreateResourceDependencyResult>
{
    public CreateResourceDependencyCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateResourceDependencyResult { public string? Conflict { get; set; } public CreateResourceDependencyResponse Response { get; set; } = new(); }
public sealed class CreateResourceDependencyHandler(IRepository<ResourceDependency> repository, IRepository<InventoryResource> resources, IRepository<InventoryResource> targets, IRepositoryInventoryRevision inventory, IRepositoryInventoryResource graphRepository) : IRequestHandler<CreateResourceDependencyMediatorCommand, CreateResourceDependencyResult>
{
    public async Task<CreateResourceDependencyResult> Handle(CreateResourceDependencyMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var source = await InventoryRules.RequireAsync(resources, payload.ResourceExternalId, cancellationToken);
        var target = await InventoryRules.RequireAsync(targets, payload.TargetExternalId, cancellationToken);
        InventoryRules.Require(source.Status != "retired", "ResourceExternalId", "Recurso desativado.");
        InventoryRules.Require(source.ExternalId != target.ExternalId, "TargetExternalId", "Escolha outro recurso.");
        InventoryRules.Require(target.Status != "retired", "TargetExternalId", "Recurso de destino desativado.");
        InventoryRules.Require(InventoryRelations.IsCompatible(source.Type, target.Type, payload.Kind), "Kind", "Relação incompatível com os tipos dos recursos.");
        if (payload.Kind == "part-of")
        {
            var graph = await graphRepository.ReadGraphAsync(cancellationToken);
            InventoryRules.RequireGraph(graph.LimitExceeded);
            InventoryRules.Require(!InventoryRelations.CreatesCompositionCycle(source.Id, target.Id, graph.Edges), "Kind", "Esta composição criaria um ciclo.");
        }
        if (await repository.ExistsAsync(item => item.SourceResourceId == source.Id && item.TargetResourceId == target.Id && item.Kind == payload.Kind && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Este vínculo já está vigente." };
        var link = ResourceDependency.Create(source, target, payload.Purpose, payload.Kind, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(link, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Atualize a ficha antes de tentar novamente." };
        return new() { Response = new() { ExternalId = link.ExternalId, Revision = link.Revision } };
    }
}
public sealed class CreateResourceDependencyValidator : AbstractValidator<CreateResourceDependencyCommand>
{
    public CreateResourceDependencyValidator()
    {
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.TargetExternalId).NotEmpty();
        RuleFor(item => item.Purpose).NotEmpty().MaximumLength(300);
        RuleFor(item => item.Kind).Must(InventoryRelations.IsKind);
    }
}
public sealed class CreateResourceDependencyMediatorValidator : AbstractValidator<CreateResourceDependencyMediatorCommand>
{
    public CreateResourceDependencyMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateResourceDependencyValidator()).OverridePropertyName(string.Empty);
    }
}
