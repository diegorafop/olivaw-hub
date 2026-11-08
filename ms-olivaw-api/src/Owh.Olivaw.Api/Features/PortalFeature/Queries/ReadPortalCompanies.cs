using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalCompaniesMediatorQuery : IRequest<ReadPortalCompaniesResponse> { public ReadPortalCompaniesQuery Payload { get; set; } = new(); public Guid ActorUserId { get; set; } }
public sealed class ReadPortalCompaniesHandler(IRepository<PortalMembership> members) : IRequestHandler<ReadPortalCompaniesMediatorQuery, ReadPortalCompaniesResponse>
{
    public async Task<ReadPortalCompaniesResponse> Handle(ReadPortalCompaniesMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await members.TakeOrderedAsync(item => item.PortalUserExternalId == request.ActorUserId && item.IsActive && item.DeletedAt == null && item.Customer.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token, item => item.Customer)).ToList();
        var response = new ReadPortalCompaniesResponse { Items = rows.Take(20).Select(item => new PortalCompanyItem { ExternalId = item.Customer.ExternalId, Business = item.Customer.Business, Role = item.Role }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalCompaniesValidator : AbstractValidator<ReadPortalCompaniesQuery> { public ReadPortalCompaniesValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalCompaniesMediatorValidator : AbstractValidator<ReadPortalCompaniesMediatorQuery> { public ReadPortalCompaniesMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalCompaniesValidator()).OverridePropertyName(string.Empty); } }
