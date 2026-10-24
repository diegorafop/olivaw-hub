using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Queries;
public sealed class ReadServiceDeploymentByFilterMediatorQuery : IRequest<ReadServiceDeploymentByFilterResponse> { public ReadServiceDeploymentByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadServiceDeploymentByFilterHandler(IRepositoryServiceDeployment repository) : IRequestHandler<ReadServiceDeploymentByFilterMediatorQuery, ReadServiceDeploymentByFilterResponse>
{
    public async Task<ReadServiceDeploymentByFilterResponse> Handle(ReadServiceDeploymentByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var page = await repository.ReadPageAsync(payload.ServiceExternalId, payload.Environment ?? string.Empty, payload.Page, payload.PageSize, cancellationToken);
        return new() { Page = payload.Page, PageSize = payload.PageSize, Total = page.Total, Items = page.Items.Select(item => new ServiceDeploymentItem { ExternalId = item.ExternalId, ServiceExternalId = item.ManagedService.ExternalId, ServiceName = item.ManagedService.Name, Environment = item.Environment, Name = item.Name, Status = item.Status, EndedAt = item.EndedAt }).ToList() };
    }
}
public sealed class ReadServiceDeploymentByFilterValidator : AbstractValidator<ReadServiceDeploymentByFilterQuery>
{
    public ReadServiceDeploymentByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.ServiceExternalId).NotEqual(Guid.Empty).When(item => item.ServiceExternalId.HasValue);
        RuleFor(item => item.Environment).Must(value => string.IsNullOrEmpty(value) || ServiceDeployment.IsEnvironment(value));
    }
}
public sealed class ReadServiceDeploymentByFilterMediatorValidator : AbstractValidator<ReadServiceDeploymentByFilterMediatorQuery>
{
    public ReadServiceDeploymentByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadServiceDeploymentByFilterValidator()).OverridePropertyName(string.Empty); }
}
