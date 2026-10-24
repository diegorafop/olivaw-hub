using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Queries;
public sealed class ReadServiceDeploymentByIdMediatorQuery : IRequest<ReadServiceDeploymentByIdResponse> { public ReadServiceDeploymentByIdQuery Payload { get; set; } = new(); }
public sealed class ReadServiceDeploymentByIdHandler(IRepository<ServiceDeployment> repository, IRepository<ServiceDeploymentChange> changes) : IRequestHandler<ReadServiceDeploymentByIdMediatorQuery, ReadServiceDeploymentByIdResponse>
{
    public async Task<ReadServiceDeploymentByIdResponse> Handle(ReadServiceDeploymentByIdMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await InventoryRules.RequireAsync(repository, request.Payload.ExternalId, cancellationToken, item => item.ManagedService);
        var history = await changes.TakeOrderedAsync(change => change.ServiceDeploymentId == item.Id && change.DeletedAt == null, 20, change => change.Revision, cancellationToken);
        return new()
        {
            ExternalId = item.ExternalId, ServiceExternalId = item.ManagedService.ExternalId, ServiceName = item.ManagedService.Name,
            Environment = item.Environment, Name = item.Name, Location = item.Location, Endpoint = item.Endpoint, Status = item.Status, Revision = item.Revision, EndedAt = item.EndedAt,
            History = history.Select(change => new ServiceDeploymentHistoryItem { Revision = change.Revision, ActorName = change.ActorName, CreatedAt = change.CreatedAt, Name = change.Name, Location = change.Location, Endpoint = change.Endpoint, Status = change.Status }).ToList()
        };
    }
}
public sealed class ReadServiceDeploymentByIdValidator : AbstractValidator<ReadServiceDeploymentByIdQuery>
{
    public ReadServiceDeploymentByIdValidator() { RuleFor(item => item.ExternalId).NotEmpty(); }
}
public sealed class ReadServiceDeploymentByIdMediatorValidator : AbstractValidator<ReadServiceDeploymentByIdMediatorQuery>
{
    public ReadServiceDeploymentByIdMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadServiceDeploymentByIdValidator()).OverridePropertyName(string.Empty); }
}
