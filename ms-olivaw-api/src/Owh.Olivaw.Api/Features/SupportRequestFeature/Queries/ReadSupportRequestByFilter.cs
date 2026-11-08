using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportRequestByFilterMediatorQuery : IRequest<ReadSupportRequestByFilterResponse> { public ReadSupportRequestByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadSupportRequestByFilterHandler(IRepository<SupportRequest> repository) : IRequestHandler<ReadSupportRequestByFilterMediatorQuery, ReadSupportRequestByFilterResponse>
{
    public async Task<ReadSupportRequestByFilterResponse> Handle(ReadSupportRequestByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId) && (p.CustomerExternalId == null || item.Customer.ExternalId == p.CustomerExternalId) && (p.Status == null || item.Status == p.Status), 21, item => item.Id, token, item => item.Customer, item => item.ManagedService!)).ToList();
        var response = new ReadSupportRequestByFilterResponse { Items = rows.Take(20).Select(item => new SupportRequestItem { ExternalId = item.ExternalId, Protocol = item.Protocol, ServiceExternalId = item.ManagedService?.ExternalId, ServiceName = item.ManagedService?.Name ?? "", CustomerExternalId = item.Customer.ExternalId, Revision = item.Revision, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt, Title = item.Title, Requester = item.Requester, Impact = item.Impact, Owner = item.Owner, NextAction = item.NextAction, InternalNote = item.InternalNote, PublicSummary = item.PublicSummary, Category = item.Category, Status = item.Status }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}
public sealed class ReadSupportRequestByFilterValidator : AbstractValidator<ReadSupportRequestByFilterQuery>
{
    public ReadSupportRequestByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.Status).Must(value => value == null || SupportRequestData.IsStatus(value)); RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty); }
}
public sealed class ReadSupportRequestByFilterMediatorValidator : AbstractValidator<ReadSupportRequestByFilterMediatorQuery>
{
    public ReadSupportRequestByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadSupportRequestByFilterValidator()).OverridePropertyName(string.Empty); }
}
