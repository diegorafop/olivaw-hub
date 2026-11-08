using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Queries;

public sealed class ReadLeadHistoryMediatorQuery : IRequest<ReadLeadHistoryResponse>
{
    public ReadLeadHistoryQuery Payload { get; set; } = new();
}
public sealed class ReadLeadHistoryHandler(IRepository<Lead> repository, IRepository<LeadChange> changes)
    : IRequestHandler<ReadLeadHistoryMediatorQuery, ReadLeadHistoryResponse>
{
    public async Task<ReadLeadHistoryResponse> Handle(ReadLeadHistoryMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstAsync(item => item.ExternalId == request.Payload.ExternalId && item.DeletedAt == null, cancellationToken);
        var before = request.Payload.BeforeRevision ?? long.MaxValue;
        var rows = (await changes.TakeOrderedAsync(item => item.LeadId == lead.Id && item.DeletedAt == null && item.Revision < before,
            21, item => item.Revision, cancellationToken)).ToList();
        var response = new ReadLeadHistoryResponse
        {
            Items = rows.Take(20).Select(item => new LeadHistoryItem
            {
                ExternalId = item.ExternalId, Revision = item.Revision, PreviousStatus = item.PreviousStatus,
                Status = item.Status, Note = item.Note, ActorName = item.ActorName, CreatedAt = item.CreatedAt
            }).ToList()
        };
        if (rows.Count > 20) response.NextBeforeRevision = response.Items[^1].Revision;
        return response;
    }
}
public sealed class ReadLeadHistoryValidator : AbstractValidator<ReadLeadHistoryQuery>
{
    public ReadLeadHistoryValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.BeforeRevision).GreaterThan(0).When(item => item.BeforeRevision.HasValue);
    }
}
public sealed class ReadLeadHistoryMediatorValidator : AbstractValidator<ReadLeadHistoryMediatorQuery>
{
    public ReadLeadHistoryMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadLeadHistoryValidator()).OverridePropertyName(string.Empty);
    }
}
