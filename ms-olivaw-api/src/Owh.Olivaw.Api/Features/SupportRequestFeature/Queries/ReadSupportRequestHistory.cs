using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportRequestHistoryMediatorQuery : IRequest<ReadSupportRequestHistoryResponse> { public ReadSupportRequestHistoryQuery Payload { get; set; } = new(); }
public sealed class ReadSupportRequestHistoryHandler(IRepository<SupportRequest> repository, IRepository<SupportRequestChange> changes) : IRequestHandler<ReadSupportRequestHistoryMediatorQuery, ReadSupportRequestHistoryResponse>
{
    public async Task<ReadSupportRequestHistoryResponse> Handle(ReadSupportRequestHistoryMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var entity = await InventoryRules.RequireAsync(repository, p.ExternalId, token);
        var rows = (await changes.TakeOrderedAsync(item => item.SupportRequestId == entity.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadSupportRequestHistoryResponse { Items = rows.Take(20).Select(item => new SupportRequestHistoryItem { Revision = item.Revision, CreatedAt = item.CreatedAt, ActorName = item.ActorName, Title = item.Title, Requester = item.Requester, Impact = item.Impact, Owner = item.Owner, NextAction = item.NextAction, InternalNote = item.InternalNote, PublicSummary = item.PublicSummary, Category = item.Category, Status = item.Status }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}
public sealed class ReadSupportRequestHistoryValidator : AbstractValidator<ReadSupportRequestHistoryQuery>
{
    public ReadSupportRequestHistoryValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); }
}
public sealed class ReadSupportRequestHistoryMediatorValidator : AbstractValidator<ReadSupportRequestHistoryMediatorQuery>
{
    public ReadSupportRequestHistoryMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadSupportRequestHistoryValidator()).OverridePropertyName(string.Empty); }
}
