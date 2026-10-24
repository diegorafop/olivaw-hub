using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.CareEvidenceFeature.Queries;
public sealed class ReadCareEvidenceByFilterMediatorQuery : IRequest<ReadCareEvidenceByFilterResponse> { public ReadCareEvidenceByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadCareEvidenceByFilterHandler(IRepository<CareEvidence> repository, IRepository<InventoryResource> resources) : IRequestHandler<ReadCareEvidenceByFilterMediatorQuery, ReadCareEvidenceByFilterResponse>
{
    public async Task<ReadCareEvidenceByFilterResponse> Handle(ReadCareEvidenceByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && item.InventoryResourceId == resource.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadCareEvidenceByFilterResponse { Items = rows.Take(20).Select(Map).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
    private static CareEvidenceItem Map(CareEvidence item) => new() { ExternalId = item.ExternalId, ObservedAt = item.ObservedAt, ActorName = item.ActorName, Source = item.Source, Note = item.Note, Kind = item.Kind, Outcome = item.Outcome, ReferenceUrl = item.ReferenceUrl };
}
public sealed class ReadCareEvidenceByFilterValidator : AbstractValidator<ReadCareEvidenceByFilterQuery>
{
    public ReadCareEvidenceByFilterValidator() { RuleFor(item => item.ResourceExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); }
}
public sealed class ReadCareEvidenceByFilterMediatorValidator : AbstractValidator<ReadCareEvidenceByFilterMediatorQuery>
{
    public ReadCareEvidenceByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadCareEvidenceByFilterValidator()).OverridePropertyName(string.Empty); }
}
