using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class UpdateServiceDeploymentMediatorCommand : IRequest<UpdateServiceDeploymentResult>
{
    public Guid ExternalId { get; set; }
    public UpdateServiceDeploymentCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateServiceDeploymentResult { public string? Conflict { get; set; } public UpdateServiceDeploymentResponse Response { get; set; } = new(); }
public sealed class UpdateServiceDeploymentHandler(IRepository<ServiceDeployment> repository, IRepository<ServiceDeploymentChange> changes, IRepository<DeploymentResource> uses, IRepositoryInventoryRevision inventory) : IRequestHandler<UpdateServiceDeploymentMediatorCommand, UpdateServiceDeploymentResult>
{
    public async Task<UpdateServiceDeploymentResult> Handle(UpdateServiceDeploymentMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var deployment = await InventoryRules.RequireAsync(repository, request.ExternalId, cancellationToken, item => item.ManagedService);
        if (deployment.Revision != payload.Revision) return new() { Conflict = "Esta implantação mudou. Reabra a ficha." };
        if (deployment.EndedAt.HasValue) return new() { Conflict = "Implantação encerrada; crie um novo período." };
        InventoryRules.Require(deployment.ManagedService.Status != "closed", "Status", "Serviço encerrado.");
        var change = deployment.Apply(payload.Name, payload.Location, payload.Endpoint, payload.Status, request.ActorUserId, request.ActorName, now);
        await changes.AddAsync(change, cancellationToken);
        if (deployment.EndedAt.HasValue)
        {
            var active = await uses.GetAsync(item => item.ServiceDeploymentId == deployment.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken);
            foreach (var use in active) use.Close(request.ActorUserId, request.ActorName, now);
        }
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Reabra a ficha e confira." };
        return new() { Response = new() { ExternalId = deployment.ExternalId, Revision = deployment.Revision } };
    }
}
public sealed class UpdateServiceDeploymentValidator : AbstractValidator<UpdateServiceDeploymentCommand>
{
    public UpdateServiceDeploymentValidator()
    {
        RuleFor(item => item.Revision).GreaterThan(0);
        RuleFor(item => item.Status).Must(ServiceDeployment.IsStatus);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Location).NotNull().MaximumLength(200);
        RuleFor(item => item.Endpoint).NotNull().MaximumLength(300).Must(InventoryResourceData.IsReferenceUrl);
    }
}
public sealed class UpdateServiceDeploymentMediatorValidator : AbstractValidator<UpdateServiceDeploymentMediatorCommand>
{
    public UpdateServiceDeploymentMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateServiceDeploymentValidator()).OverridePropertyName(string.Empty);
    }
}
