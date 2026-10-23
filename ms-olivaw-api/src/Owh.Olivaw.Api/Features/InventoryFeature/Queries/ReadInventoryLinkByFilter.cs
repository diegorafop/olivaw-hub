using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Queries;
public sealed class ReadInventoryLinkByFilterMediatorQuery : IRequest<ReadInventoryLinkByFilterResponse> { public ReadInventoryLinkByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadInventoryLinkByFilterHandler(IRepositoryInventoryResource repository, IRepository<InventoryResource> resources, IRepository<ServiceDeployment> deployments) : IRequestHandler<ReadInventoryLinkByFilterMediatorQuery, ReadInventoryLinkByFilterResponse>
{
    public async Task<ReadInventoryLinkByFilterResponse> Handle(ReadInventoryLinkByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        if (payload.ResourceExternalId.HasValue) await InventoryRules.RequireAsync(resources, payload.ResourceExternalId.Value, cancellationToken);
        if (payload.DeploymentExternalId.HasValue) await InventoryRules.RequireAsync(deployments, payload.DeploymentExternalId.Value, cancellationToken);
        var page = await repository.ReadLinksAsync(new InventoryLinkFilter { ResourceExternalId = payload.ResourceExternalId, DeploymentExternalId = payload.DeploymentExternalId, Kind = payload.Kind, IncludeEnded = payload.IncludeEnded, Page = payload.Page, PageSize = payload.PageSize }, cancellationToken);
        return new() { Page = payload.Page, PageSize = payload.PageSize, Total = page.Total, Items = page.Items.Select(item => new InventoryLinkItem
        {
            ExternalId = item.ExternalId,
            ResourceExternalId = item.ResourceExternalId,
            ResourceName = item.ResourceName,
            TargetExternalId = item.TargetExternalId,
            TargetName = item.TargetName,
            RelationshipKind = item.RelationshipKind,
            Environment = item.Environment,
            Purpose = item.Purpose,
            Revision = item.Revision,
            StartedAt = item.StartedAt,
            EndedAt = item.EndedAt,
            StartedByName = item.StartedByName,
            EndedByName = item.EndedByName,
            ReservedCpu = item.ReservedCpu,
            ReservedMemoryGiB = item.ReservedMemoryGiB,
            ReservedStorageGiB = item.ReservedStorageGiB,
        }).ToList() };
    }
}
public sealed class ReadInventoryLinkByFilterValidator : AbstractValidator<ReadInventoryLinkByFilterQuery>
{
    public ReadInventoryLinkByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Kind).Must(value => value is "customer" or "dependency" or "deployment");
        RuleFor(item => item.ResourceExternalId).NotEqual(Guid.Empty).When(item => item.ResourceExternalId.HasValue);
        RuleFor(item => item.DeploymentExternalId).NotEqual(Guid.Empty).When(item => item.DeploymentExternalId.HasValue);
        RuleFor(item => item).Must(item => item.ResourceExternalId.HasValue != item.DeploymentExternalId.HasValue).WithMessage("Informe um recurso ou uma implantação.");
        RuleFor(item => item.Kind).Equal("deployment").When(item => item.DeploymentExternalId.HasValue);
    }
}
public sealed class ReadInventoryLinkByFilterMediatorValidator : AbstractValidator<ReadInventoryLinkByFilterMediatorQuery>
{
    public ReadInventoryLinkByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadInventoryLinkByFilterValidator()).OverridePropertyName(string.Empty); }
}
