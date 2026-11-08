using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.IncidentFeature.Queries;
public sealed class ReadIncidentByFilterMediatorQuery : IRequest<ReadIncidentByFilterResponse> { public ReadIncidentByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadIncidentByFilterHandler(IRepository<Incident> repository) : IRequestHandler<ReadIncidentByFilterMediatorQuery, ReadIncidentByFilterResponse>
{
    public async Task<ReadIncidentByFilterResponse> Handle(ReadIncidentByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId) && (p.ResourceExternalId == null || item.Resource.ExternalId == p.ResourceExternalId) && (p.Status == null || item.Status == p.Status), 21, item => item.Id, token, item => item.Resource)).ToList();
        var response = new ReadIncidentByFilterResponse { Items = rows.Take(20).Select(item => new IncidentItem { ExternalId = item.ExternalId, ResourceExternalId = item.Resource.ExternalId, Revision = item.Revision, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt, Title = item.Title, Impact = item.Impact, Owner = item.Owner, NextAction = item.NextAction, InternalNote = item.InternalNote, Severity = item.Severity, Status = item.Status }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}
public sealed class ReadIncidentByFilterValidator : AbstractValidator<ReadIncidentByFilterQuery>
{
    public ReadIncidentByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.Status).Must(value => value == null || IncidentData.IsStatus(value)); RuleFor(item => item.ResourceExternalId).NotEqual(Guid.Empty); }
}
public sealed class ReadIncidentByFilterMediatorValidator : AbstractValidator<ReadIncidentByFilterMediatorQuery>
{
    public ReadIncidentByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadIncidentByFilterValidator()).OverridePropertyName(string.Empty); }
}
