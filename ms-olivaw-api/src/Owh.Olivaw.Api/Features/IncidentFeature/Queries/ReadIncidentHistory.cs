using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.IncidentFeature.Queries;
public sealed class ReadIncidentHistoryMediatorQuery : IRequest<ReadIncidentHistoryResponse> { public ReadIncidentHistoryQuery Payload { get; set; } = new(); }
public sealed class ReadIncidentHistoryHandler(IRepository<Incident> repository, IRepository<IncidentChange> changes) : IRequestHandler<ReadIncidentHistoryMediatorQuery, ReadIncidentHistoryResponse>
{
    public async Task<ReadIncidentHistoryResponse> Handle(ReadIncidentHistoryMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var entity = await InventoryRules.RequireAsync(repository, p.ExternalId, token);
        var rows = (await changes.TakeOrderedAsync(item => item.IncidentId == entity.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadIncidentHistoryResponse { Items = rows.Take(20).Select(item => new IncidentHistoryItem { Revision = item.Revision, CreatedAt = item.CreatedAt, ActorName = item.ActorName, Title = item.Title, Impact = item.Impact, Owner = item.Owner, NextAction = item.NextAction, InternalNote = item.InternalNote, Severity = item.Severity, Status = item.Status }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}
public sealed class ReadIncidentHistoryValidator : AbstractValidator<ReadIncidentHistoryQuery>
{
    public ReadIncidentHistoryValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); }
}
public sealed class ReadIncidentHistoryMediatorValidator : AbstractValidator<ReadIncidentHistoryMediatorQuery>
{
    public ReadIncidentHistoryMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadIncidentHistoryValidator()).OverridePropertyName(string.Empty); }
}
