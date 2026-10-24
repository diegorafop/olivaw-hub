using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalMembershipByFilterMediatorQuery : IRequest<ReadPortalMembershipByFilterResponse> { public ReadPortalMembershipByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadPortalMembershipByFilterHandler(IRepository<PortalMembership> members, [FromKeyedServices("portal")] IIdentityStore identity, PortalAuthorityOptions options) : IRequestHandler<ReadPortalMembershipByFilterMediatorQuery, ReadPortalMembershipByFilterResponse>
{
    public async Task<ReadPortalMembershipByFilterResponse> Handle(ReadPortalMembershipByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await members.TakeOrderedAsync(item => item.DeletedAt == null && (p.CustomerExternalId == null || item.Customer.ExternalId == p.CustomerExternalId) && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token, item => item.Customer)).ToList();
        var ids = rows.Take(20).Select(item => item.PortalUserExternalId).ToArray();
        IReadOnlyList<UserProfile> people = Array.Empty<UserProfile>();
        if (ids.Length > 0) people = await identity.ListAsync<UserProfile>(item => ids.Contains(item.ExternalId), 1, 100, token);
        var users = people.ToDictionary(item => item.ExternalId);
        var response = new ReadPortalMembershipByFilterResponse { AuthorityConfigured = options.Configured, Items = rows.Take(20).Select(item => new PortalMembershipItem { ExternalId = item.ExternalId, CustomerExternalId = item.Customer.ExternalId, Business = item.Customer.Business, Name = users.GetValueOrDefault(item.PortalUserExternalId)?.Name ?? "Conta indisponível", Email = users.GetValueOrDefault(item.PortalUserExternalId)?.Email ?? "", Role = item.Role, IsActive = item.IsActive, Revision = item.Revision }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalMembershipByFilterValidator : AbstractValidator<ReadPortalMembershipByFilterQuery> { public ReadPortalMembershipByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty); } }
public sealed class ReadPortalMembershipByFilterMediatorValidator : AbstractValidator<ReadPortalMembershipByFilterMediatorQuery> { public ReadPortalMembershipByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalMembershipByFilterValidator()).OverridePropertyName(string.Empty); } }
