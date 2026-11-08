using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class CreateDeploymentResourceMediatorCommand : IRequest<CreateDeploymentResourceResult>
{
    public CreateDeploymentResourceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateDeploymentResourceResult { public string? Conflict { get; set; } public CreateDeploymentResourceResponse Response { get; set; } = new(); }
public sealed class CreateDeploymentResourceHandler(IRepository<DeploymentResource> repository, IRepository<InventoryResource> resources, IRepository<ServiceDeployment> targets, IRepositoryInventoryRevision inventory) : IRequestHandler<CreateDeploymentResourceMediatorCommand, CreateDeploymentResourceResult>
{
    public async Task<CreateDeploymentResourceResult> Handle(CreateDeploymentResourceMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var source = await InventoryRules.RequireAsync(resources, payload.ResourceExternalId, cancellationToken);
        var target = await InventoryRules.RequireAsync(targets, payload.TargetExternalId, cancellationToken, item => item.ManagedService);
        InventoryRules.Require(source.Status != "retired", "ResourceExternalId", "Recurso desativado.");
        InventoryRules.Require(!target.EndedAt.HasValue, "TargetExternalId", "Implantação encerrada.");
        InventoryRules.Require(target.ManagedService.Status != "closed", "TargetExternalId", "Serviço encerrado.");
        if (await repository.ExistsAsync(item => item.InventoryResourceId == source.Id && item.ServiceDeploymentId == target.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Este vínculo já está vigente." };
        var link = DeploymentResource.Create(source, target, payload.Purpose, payload.ReservedCpu, payload.ReservedMemoryGiB, payload.ReservedStorageGiB, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(link, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Atualize a ficha antes de tentar novamente." };
        return new() { Response = new() { ExternalId = link.ExternalId, Revision = link.Revision } };
    }
}
public sealed class CreateDeploymentResourceValidator : AbstractValidator<CreateDeploymentResourceCommand>
{
    public CreateDeploymentResourceValidator()
    {
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.TargetExternalId).NotEmpty();
        RuleFor(item => item.Purpose).NotEmpty().MaximumLength(300);
        RuleFor(item => item.ReservedCpu).InclusiveBetween(0M, 1000000000M).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.ReservedMemoryGiB).InclusiveBetween(0M, 1000000000M).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.ReservedStorageGiB).InclusiveBetween(0M, 1000000000M).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
    }
}
public sealed class CreateDeploymentResourceMediatorValidator : AbstractValidator<CreateDeploymentResourceMediatorCommand>
{
    public CreateDeploymentResourceMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateDeploymentResourceValidator()).OverridePropertyName(string.Empty);
    }
}
