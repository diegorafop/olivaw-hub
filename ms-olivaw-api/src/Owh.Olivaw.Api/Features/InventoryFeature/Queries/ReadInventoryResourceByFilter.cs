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
        var payload = request.Payload;
        var result = await repository.ReadPageAsync(new InventoryResourceFilter { Page = payload.Page, PageSize = payload.PageSize, Search = payload.Search ?? string.Empty, Type = payload.Type ?? string.Empty, Status = payload.Status ?? string.Empty, Provider = payload.Provider ?? string.Empty, CustomerExternalId = payload.CustomerExternalId, Environment = payload.Environment ?? string.Empty, Shared = payload.Shared }, cancellationToken);
        InventoryRules.RequireGraph(result.LimitExceeded);
        return new() { Page = payload.Page, PageSize = payload.PageSize, Total = result.Total, Items = result.Items.Select(item => new InventoryResourceItem
        {
            ExternalId = item.Resource.ExternalId, TechnicalId = item.Resource.TechnicalId, Name = item.Resource.Name, Type = item.Resource.Type,
            Status = item.Resource.Status, Provider = item.Resource.Provider, CustomerCount = item.Context.CustomerCount,
            OriginCount = item.Context.OriginCount, Origins = item.Context.Origins.Select(InventoryReadMapping.Origin).ToList()
        }).ToList() };
    }
}
public sealed class ReadInventoryResourceByFilterValidator : AbstractValidator<ReadInventoryResourceByFilterQuery>
{
    public ReadInventoryResourceByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000); RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Search).MaximumLength(100); RuleFor(item => item.Provider).MaximumLength(80);
        RuleFor(item => item.Type).Must(value => string.IsNullOrEmpty(value) || InventoryResourceData.IsType(value!));
        RuleFor(item => item.Status).Must(value => string.IsNullOrEmpty(value) || InventoryResourceData.IsStatus(value!));
        RuleFor(item => item.Environment).Must(value => string.IsNullOrEmpty(value) || ServiceDeployment.IsEnvironment(value!));
        RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty).When(item => item.CustomerExternalId.HasValue);
    }
}
public sealed class ReadInventoryResourceByFilterMediatorValidator : AbstractValidator<ReadInventoryResourceByFilterMediatorQuery>
{
    public ReadInventoryResourceByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadInventoryResourceByFilterValidator()).OverridePropertyName(string.Empty); }
}
