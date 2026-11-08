using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalMessagesMediatorQuery : IRequest<ReadPortalMessagesResponse> { public ReadPortalMessagesQuery Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public bool Preview { get; set; } }
public sealed class ReadPortalMessagesHandler(PortalAccess access, IRepository<Customer> customers, IRepository<SupportRequest> requests, IRepository<SupportMessage> messages) : IRequestHandler<ReadPortalMessagesMediatorQuery, ReadPortalMessagesResponse>
{
    public async Task<ReadPortalMessagesResponse> Handle(ReadPortalMessagesMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        Customer customer;
        if (request.Preview) customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        else customer = (await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, false, false, token)).Customer;
        var entity = await requests.GetFirstOrDefaultAsync(item => item.ExternalId == p.RequestExternalId && item.CustomerId == customer.Id && item.DeletedAt == null, token);
        if (entity == null) throw new IdentityFailure(403, "Solicitação não disponível para esta empresa.");
        var rows = (await messages.TakeOrderedAsync(item => item.SupportRequestId == entity.Id && item.IsPublic && item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadPortalMessagesResponse();
        foreach (var item in rows.Take(20)) { var author = item.AuthorName; if (item.IsStaff) author = "Equipe Olivaw"; response.Items.Add(new() { ExternalId = item.ExternalId, Body = item.Body, Author = author, CreatedAt = item.CreatedAt }); }
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalMessagesValidator : AbstractValidator<ReadPortalMessagesQuery> { public ReadPortalMessagesValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.RequestExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalMessagesMediatorValidator : AbstractValidator<ReadPortalMessagesMediatorQuery> { public ReadPortalMessagesMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalMessagesValidator()).OverridePropertyName(string.Empty); } }
