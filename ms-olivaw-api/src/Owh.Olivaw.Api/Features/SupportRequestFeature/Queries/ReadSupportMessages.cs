using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Validation;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportMessagesMediatorQuery : IRequest<ReadSupportMessagesResponse> { public ReadSupportMessagesQuery Payload { get; set; } = new(); }
public sealed class ReadSupportMessagesHandler(IRepository<SupportRequest> requests, IRepository<SupportMessage> messages) : IRequestHandler<ReadSupportMessagesMediatorQuery, ReadSupportMessagesResponse>
{
    public async Task<ReadSupportMessagesResponse> Handle(ReadSupportMessagesMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload; var entity = await InventoryRules.RequireAsync(requests, p.RequestExternalId, token);
        var rows = (await messages.TakeOrderedAsync(item => item.SupportRequestId == entity.Id && item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadSupportMessagesResponse { Items = rows.Take(20).Select(item => new SupportMessageItem { ExternalId = item.ExternalId, Body = item.Body, IsPublic = item.IsPublic, IsStaff = item.IsStaff, AuthorName = item.AuthorName, CreatedAt = item.CreatedAt }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadSupportMessagesValidator : AbstractValidator<ReadSupportMessagesQuery> { public ReadSupportMessagesValidator() { RuleFor(item => item.RequestExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadSupportMessagesMediatorValidator : AbstractValidator<ReadSupportMessagesMediatorQuery> { public ReadSupportMessagesMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadSupportMessagesValidator()).OverridePropertyName(string.Empty); } }
