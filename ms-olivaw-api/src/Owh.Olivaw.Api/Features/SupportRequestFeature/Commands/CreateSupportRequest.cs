using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Commands;
public sealed class CreateSupportRequestMediatorCommand : IRequest<CreateSupportRequestResult>
{
    public CreateSupportRequestCommand Payload { get; set; } = new();
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateSupportRequestResult { public string? Conflict { get; set; } public CreateSupportRequestResponse Response { get; set; } = new(); }
public sealed class CreateSupportRequestHandler(IRepository<SupportRequest> repository, IRepository<Customer> anchors, IRepository<ManagedService> services, IRepository<ManagedServiceCustomer> associations) : IRequestHandler<CreateSupportRequestMediatorCommand, CreateSupportRequestResult>
{
    public async Task<CreateSupportRequestResult> Handle(CreateSupportRequestMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var data = new SupportRequestData { Title = p.Title, Requester = p.Requester, Impact = p.Impact, Owner = p.Owner, NextAction = p.NextAction, InternalNote = p.InternalNote, PublicSummary = p.PublicSummary, Category = p.Category, Status = p.Status };
        var anchor = await InventoryRules.RequireAsync(anchors, p.CustomerExternalId, token);
        ManagedService? service = null;
        if (p.ServiceExternalId.HasValue)
        {
            service = await InventoryRules.RequireAsync(services, p.ServiceExternalId.Value, token);
            if (!await associations.ExistsAsync(item => item.CustomerId == anchor.Id && item.ManagedServiceId == service.Id && item.EndedAt == null, token)) return new() { Conflict = "Serviço não vinculado ao cliente." };
        }
        var entity = SupportRequest.Create(anchor, data, request.ActorUserId, request.ActorName, DateTime.UtcNow, service);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Conflito de atualização. Confira o registro atual." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class CreateSupportRequestValidator : AbstractValidator<CreateSupportRequestCommand>
{
    public CreateSupportRequestValidator()
    {
        RuleFor(item => item.PublicSummary).NotNull().MaximumLength(1000);
        RuleFor(item => item.Title).NotNull().MaximumLength(120);
        RuleFor(item => item.Requester).NotNull().MaximumLength(120);
        RuleFor(item => item.Impact).NotNull().MaximumLength(1000);
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.NextAction).NotNull().MaximumLength(500);
        RuleFor(item => item.InternalNote).NotNull().MaximumLength(3000);
        RuleFor(item => item.Title).NotEmpty();
        RuleFor(item => item.Owner).NotEmpty();
        RuleFor(item => item.NextAction).NotEmpty();
        RuleFor(item => item.Requester).NotEmpty();
        RuleFor(item => item.Status).Must(SupportRequestData.IsStatus);
        RuleFor(item => item.Category).Must(SupportRequestData.IsCategory);
        RuleFor(item => item.InternalNote).NotEmpty().When(item => item.Status is "resolved" or "closed" or "cancelled");
        RuleFor(item => item.ServiceExternalId).NotEqual(Guid.Empty);
        RuleFor(item => item.CustomerExternalId).NotEmpty();
        RuleFor(item => item.Status).Equal("received");
    }
}
public sealed class CreateSupportRequestMediatorValidator : AbstractValidator<CreateSupportRequestMediatorCommand>
{
    public CreateSupportRequestMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateSupportRequestValidator()).OverridePropertyName(string.Empty);
    }
}
