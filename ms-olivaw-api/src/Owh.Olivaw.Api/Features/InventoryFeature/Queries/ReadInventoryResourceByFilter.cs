using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Queries;
public sealed class ReadInventoryResourceByFilterMediatorQuery : IRequest<ReadInventoryResourceByFilterResponse>
{
    public ReadInventoryResourceByFilterQuery Payload { get; set; } = new();
}
public sealed class ReadInventoryResourceByFilterHandler(IRepositoryInventoryResource repository) : IRequestHandler<ReadInventoryResourceByFilterMediatorQuery, ReadInventoryResourceByFilterResponse>
{
    public async Task<ReadInventoryResourceByFilterResponse> Handle(ReadInventoryResourceByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var result = await repository.ReadPageAsync(new InventoryResourceFilter { Page = p.Page, PageSize = p.PageSize, Search = p.Search, Type = p.Type, Status = p.Status, Provider = p.Provider, CustomerExternalId = p.CustomerExternalId, Environment = p.Environment, Shared = p.Shared }, cancellationToken);
        InventoryRules.RequireGraph(result.LimitExceeded);
        return new() { Page = p.Page, PageSize = p.PageSize, Total = result.Total, Items = result.Items.Select(item => new InventoryResourceItem
        {
            ExternalId = item.Resource.ExternalId, TechnicalId = item.Resource.TechnicalId, Name = item.Resource.Name, Type = item.Resource.Type,
            Status = item.Resource.Status, Provider = item.Resource.Provider, CustomerCount = item.Context.CustomerCount,
            Origins = item.Context.Origins.Select(InventoryReadMapping.Origin).ToList()
        }).ToList() };
    }
}
public sealed class ReadInventoryResourceByFilterValidator : AbstractValidator<ReadInventoryResourceByFilterQuery>
{
    public ReadInventoryResourceByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000); RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Search).NotNull().MaximumLength(100); RuleFor(item => item.Provider).NotNull().MaximumLength(80);
        RuleFor(item => item.Type).Must(value => value == "" || InventoryResourceData.IsType(value));
        RuleFor(item => item.Status).Must(value => value == "" || InventoryResourceData.IsStatus(value));
        RuleFor(item => item.Environment).Must(value => value == "" || ServiceDeployment.IsEnvironment(value));
        RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty).When(item => item.CustomerExternalId.HasValue);
    }
}
public sealed class ReadInventoryResourceByFilterMediatorValidator : AbstractValidator<ReadInventoryResourceByFilterMediatorQuery>
{
    public ReadInventoryResourceByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadInventoryResourceByFilterValidator()).OverridePropertyName(string.Empty); }
}
