using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class UpdateInventoryResourceMediatorCommand : IRequest<UpdateInventoryResourceResult>
{
    public Guid ExternalId { get; set; }
    public UpdateInventoryResourceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateInventoryResourceResult { public string? Conflict { get; set; } public UpdateInventoryResourceResponse Response { get; set; } = new(); }
public sealed class UpdateInventoryResourceHandler(IRepository<InventoryResource> repository, IRepository<InventoryResourceChange> changes, IRepositoryInventoryRevision inventory, IRepository<ResourceCustomer> customers, IRepository<ResourceDependency> dependencies, IRepository<DeploymentResource> uses) : IRequestHandler<UpdateInventoryResourceMediatorCommand, UpdateInventoryResourceResult>
{
    public async Task<UpdateInventoryResourceResult> Handle(UpdateInventoryResourceMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var data = InventoryRules.ToDomain(request.Payload);
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var resource = await InventoryRules.RequireAsync(repository, request.ExternalId, cancellationToken);
        if (resource.Revision != request.Payload.Revision) return new() { Conflict = "Este recurso mudou. Reabra a ficha." };
        var key = InventoryResource.IdentityKey(data);
        if (resource.ProviderIdentityKey != null && resource.ProviderIdentityKey != key) return new() { Conflict = "A identidade do provedor é imutável; cadastre um novo recurso para um novo ativo." };
        if (key != null && await repository.ExistsAsync(item => item.ProviderIdentityKey == key && item.Id != resource.Id, cancellationToken)) return new() { Conflict = "Este recurso do provedor já está cadastrado." };
        if (data.Status == "retired")
        {
            if (await customers.ExistsAsync(item => item.InventoryResourceId == resource.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Encerre os vínculos com clientes antes de desativar o recurso." };
            if (await uses.ExistsAsync(item => item.InventoryResourceId == resource.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Encerre os usos pelas implantações antes de desativar o recurso." };
        }
        if (data.Status == "retired" || data.Type != resource.Type)
        {
            if (await dependencies.ExistsAsync(item => (item.SourceResourceId == resource.Id || item.TargetResourceId == resource.Id) && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Encerre as relações técnicas antes de alterar o tipo ou desativar o recurso." };
        }
        var change = resource.Apply(data, request.ActorUserId, request.ActorName, now);
        await changes.AddAsync(change, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou ou o recurso já existe. Reabra a ficha e confira." };
        return new() { Response = new() { ExternalId = resource.ExternalId, Revision = resource.Revision } };
    }
}
public sealed class UpdateInventoryResourceValidator : AbstractValidator<UpdateInventoryResourceCommand>
{
    public UpdateInventoryResourceValidator()
    {
        RuleFor(item => item.Name).NotNull().MaximumLength(120);
        RuleFor(item => item.Type).NotNull().MaximumLength(30);
        RuleFor(item => item.Purpose).NotNull().MaximumLength(1000);
        RuleFor(item => item.Provider).NotNull().MaximumLength(80);
        RuleFor(item => item.AccountReference).NotNull().MaximumLength(120);
        RuleFor(item => item.Region).NotNull().MaximumLength(80);
        RuleFor(item => item.ProviderResourceId).NotNull().MaximumLength(180);
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.Payer).NotNull().MaximumLength(120);
        RuleFor(item => item.Operator).NotNull().MaximumLength(120);
        RuleFor(item => item.Scope).NotNull().MaximumLength(20);
        RuleFor(item => item.Status).NotNull().MaximumLength(20);
        RuleFor(item => item.Source).NotNull().MaximumLength(500);
        RuleFor(item => item.Monitoring).NotNull().MaximumLength(20);
        RuleFor(item => item.ReferenceUrl).NotNull().MaximumLength(300);
        RuleFor(item => item.CpuCapacity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.CpuCapacity).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.MemoryCapacityGiB).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.MemoryCapacityGiB).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.StorageCapacityGiB).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.StorageCapacityGiB).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.Name).NotEmpty();
        RuleFor(item => item.Type).Must(InventoryResourceData.IsType);
        RuleFor(item => item.Status).Must(InventoryResourceData.IsStatus);
        RuleFor(item => item.Scope).Must(InventoryResourceData.IsScope);
        RuleFor(item => item.Monitoring).Must(InventoryResourceData.IsMonitoring);
        RuleFor(item => item.ReferenceUrl).Must(InventoryResourceData.IsReferenceUrl);
        RuleFor(item => item.Source).NotEmpty().When(item => item.Status == "verified");
        RuleFor(item => item.Provider).NotEmpty().When(item => !string.IsNullOrEmpty(item.ProviderResourceId));
        RuleFor(item => item.Revision).InclusiveBetween(1, long.MaxValue - 1);
    }
}
public sealed class UpdateInventoryResourceMediatorValidator : AbstractValidator<UpdateInventoryResourceMediatorCommand>
{
    public UpdateInventoryResourceMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateInventoryResourceValidator()).OverridePropertyName(string.Empty);
    }
}
