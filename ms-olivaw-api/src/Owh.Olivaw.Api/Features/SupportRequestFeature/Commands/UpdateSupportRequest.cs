using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Commands;
public sealed class UpdateSupportRequestMediatorCommand : IRequest<UpdateSupportRequestResult>
{
    public UpdateSupportRequestCommand Payload { get; set; } = new();
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateSupportRequestResult { public string? Conflict { get; set; } public UpdateSupportRequestResponse Response { get; set; } = new(); }
public sealed class UpdateSupportRequestHandler(IRepository<SupportRequest> repository, IRepository<SupportRequestChange> changes) : IRequestHandler<UpdateSupportRequestMediatorCommand, UpdateSupportRequestResult>
{
    public async Task<UpdateSupportRequestResult> Handle(UpdateSupportRequestMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var data = new SupportRequestData { Title = p.Title, Requester = p.Requester, Impact = p.Impact, Owner = p.Owner, NextAction = p.NextAction, InternalNote = p.InternalNote, PublicSummary = p.PublicSummary, Category = p.Category, Status = p.Status };
        var entity = await InventoryRules.RequireAsync(repository, request.ExternalId, token, item => item.Customer);
        if (entity.Revision != p.Revision) return new() { Conflict = "Este registro mudou. Reabra antes de salvar." };
        var change = entity.Apply(data, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await changes.AddAsync(change, token);
        await repository.UpdateAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Conflito de atualização. Confira o registro atual." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class UpdateSupportRequestValidator : AbstractValidator<UpdateSupportRequestCommand>
{
    public UpdateSupportRequestValidator()
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
        RuleFor(item => item.Revision).GreaterThan(0);
    }
}
public sealed class UpdateSupportRequestMediatorValidator : AbstractValidator<UpdateSupportRequestMediatorCommand>
{
    public UpdateSupportRequestMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateSupportRequestValidator()).OverridePropertyName(string.Empty);
    }
}
