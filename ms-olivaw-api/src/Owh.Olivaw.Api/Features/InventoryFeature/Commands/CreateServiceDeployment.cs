using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class CreateServiceDeploymentMediatorCommand : IRequest<CreateServiceDeploymentResult>
{
    public CreateServiceDeploymentCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateServiceDeploymentResult { public string? Conflict { get; set; } public CreateServiceDeploymentResponse Response { get; set; } = new(); }
public sealed class CreateServiceDeploymentHandler(IRepository<ServiceDeployment> repository, IRepository<ManagedService> services, IRepositoryInventoryRevision inventory) : IRequestHandler<CreateServiceDeploymentMediatorCommand, CreateServiceDeploymentResult>
{
    public async Task<CreateServiceDeploymentResult> Handle(CreateServiceDeploymentMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var service = await InventoryRules.RequireAsync(services, payload.ServiceExternalId, cancellationToken);
        InventoryRules.Require(service.Status != "closed", "ServiceExternalId", "Serviço encerrado.");
        if (await repository.ExistsAsync(item => item.ManagedServiceId == service.Id && item.Environment == payload.Environment && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Já existe uma implantação vigente para este serviço e ambiente." };
        var deployment = ServiceDeployment.Create(service, payload.Environment, payload.Name, payload.Location, payload.Endpoint, payload.Status, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(deployment, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Reabra a ficha e confira." };
        return new() { Response = new() { ExternalId = deployment.ExternalId, Revision = deployment.Revision } };
    }
}
public sealed class CreateServiceDeploymentValidator : AbstractValidator<CreateServiceDeploymentCommand>
{
    public CreateServiceDeploymentValidator()
    {
        RuleFor(item => item.ServiceExternalId).NotEmpty();
        RuleFor(item => item.Environment).Must(ServiceDeployment.IsEnvironment);
        RuleFor(item => item.Status).Must(value => value is "planned" or "active");
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Location).NotNull().MaximumLength(200);
        RuleFor(item => item.Endpoint).NotNull().MaximumLength(300).Must(InventoryResourceData.IsReferenceUrl);
    }
}
public sealed class CreateServiceDeploymentMediatorValidator : AbstractValidator<CreateServiceDeploymentMediatorCommand>
{
    public CreateServiceDeploymentMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateServiceDeploymentValidator()).OverridePropertyName(string.Empty);
    }
}
