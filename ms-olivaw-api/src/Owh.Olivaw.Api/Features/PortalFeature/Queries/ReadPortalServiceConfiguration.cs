using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalServiceConfigurationMediatorQuery : IRequest<ReadPortalServiceConfigurationResponse> { public ReadPortalServiceConfigurationQuery Payload { get; set; } = new(); }
public sealed class ReadPortalServiceConfigurationHandler(IRepository<PortalServiceLink> links) : IRequestHandler<ReadPortalServiceConfigurationMediatorQuery, ReadPortalServiceConfigurationResponse>
{
    public async Task<ReadPortalServiceConfigurationResponse> Handle(ReadPortalServiceConfigurationMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await links.TakeOrderedAsync(item => item.Customer.ExternalId == p.CustomerExternalId && (p.BeforeId == 0 || item.Id < p.BeforeId) && item.DeletedAt == null, 21, item => item.Id, token, item => item.Customer, item => item.ManagedService, item => item.HealthResource!)).ToList();
        var response = new ReadPortalServiceConfigurationResponse { Items = rows.Take(20).Select(item => new PortalServiceConfigurationItem { ExternalId = item.ExternalId, ServiceExternalId = item.ManagedService.ExternalId, ServiceName = item.ManagedService.Name, HealthResourceExternalId = item.HealthResource?.ExternalId, Label = item.Label, PublicUrl = item.PublicUrl, IsActive = item.IsActive, Revision = item.Revision }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalServiceConfigurationValidator : AbstractValidator<ReadPortalServiceConfigurationQuery> { public ReadPortalServiceConfigurationValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalServiceConfigurationMediatorValidator : AbstractValidator<ReadPortalServiceConfigurationMediatorQuery> { public ReadPortalServiceConfigurationMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalServiceConfigurationValidator()).OverridePropertyName(string.Empty); } }
