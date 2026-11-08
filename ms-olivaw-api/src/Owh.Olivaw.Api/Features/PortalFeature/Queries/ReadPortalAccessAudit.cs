using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalAccessAuditMediatorQuery : IRequest<ReadPortalAccessAuditResponse> { public ReadPortalAccessAuditQuery Payload { get; set; } = new(); }
public sealed class ReadPortalAccessAuditHandler(IRepository<PortalAccessAudit> audits) : IRequestHandler<ReadPortalAccessAuditMediatorQuery, ReadPortalAccessAuditResponse>
{
    public async Task<ReadPortalAccessAuditResponse> Handle(ReadPortalAccessAuditMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await audits.TakeOrderedAsync(item => item.Customer != null && item.Customer.ExternalId == p.CustomerExternalId && item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadPortalAccessAuditResponse { Items = rows.Take(20).Select(item => new PortalAccessAuditItem { ExternalId = item.ExternalId, Action = item.Action, Details = item.Details, ActorName = item.ActorName, ActorAuthority = item.ActorAuthority, CreatedAt = item.CreatedAt }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalAccessAuditValidator : AbstractValidator<ReadPortalAccessAuditQuery> { public ReadPortalAccessAuditValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalAccessAuditMediatorValidator : AbstractValidator<ReadPortalAccessAuditMediatorQuery> { public ReadPortalAccessAuditMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalAccessAuditValidator()).OverridePropertyName(string.Empty); } }
