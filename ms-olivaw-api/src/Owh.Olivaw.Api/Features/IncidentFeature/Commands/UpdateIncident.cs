using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.IncidentFeature.Commands;
public sealed class UpdateIncidentMediatorCommand : IRequest<UpdateIncidentResult>
{
    public UpdateIncidentCommand Payload { get; set; } = new();
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateIncidentResult { public string? Conflict { get; set; } public UpdateIncidentResponse Response { get; set; } = new(); }
public sealed class UpdateIncidentHandler(IRepository<Incident> repository, IRepository<IncidentChange> changes) : IRequestHandler<UpdateIncidentMediatorCommand, UpdateIncidentResult>
{
    public async Task<UpdateIncidentResult> Handle(UpdateIncidentMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var data = new IncidentData { Title = p.Title, Impact = p.Impact, Owner = p.Owner, NextAction = p.NextAction, InternalNote = p.InternalNote, Severity = p.Severity, Status = p.Status };
        var entity = await InventoryRules.RequireAsync(repository, request.ExternalId, token, item => item.Resource);
        if (entity.Revision != p.Revision) return new() { Conflict = "Este registro mudou. Reabra antes de salvar." };
        if (p.Status == "resolved" && entity.Status != "observation") return new() { Conflict = "Confirme a recuperação em observação antes de resolver." };
        var change = entity.Apply(data, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await changes.AddAsync(change, token);
        await repository.UpdateAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Conflito de atualização. Confira o registro atual." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class UpdateIncidentValidator : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentValidator()
    {
        RuleFor(item => item.Title).NotNull().MaximumLength(120);
        RuleFor(item => item.Impact).NotNull().MaximumLength(1000);
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.NextAction).NotNull().MaximumLength(500);
        RuleFor(item => item.InternalNote).NotNull().MaximumLength(3000);
        RuleFor(item => item.Title).NotEmpty();
        RuleFor(item => item.Owner).NotEmpty();
        RuleFor(item => item.NextAction).NotEmpty();
        RuleFor(item => item.Impact).NotEmpty();
        RuleFor(item => item.Status).Must(IncidentData.IsStatus);
        RuleFor(item => item.Severity).Must(IncidentData.IsSeverity);
        RuleFor(item => item.InternalNote).NotEmpty().When(item => item.Status is "resolved" or "closed" or "cancelled");
        RuleFor(item => item.Revision).GreaterThan(0);
    }
}
public sealed class UpdateIncidentMediatorValidator : AbstractValidator<UpdateIncidentMediatorCommand>
{
    public UpdateIncidentMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateIncidentValidator()).OverridePropertyName(string.Empty);
    }
}
