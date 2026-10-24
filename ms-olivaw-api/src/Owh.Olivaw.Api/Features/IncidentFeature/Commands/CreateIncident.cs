using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.IncidentFeature.Commands;
public sealed class CreateIncidentMediatorCommand : IRequest<CreateIncidentResult>
{
    public CreateIncidentCommand Payload { get; set; } = new();
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateIncidentResult { public string? Conflict { get; set; } public CreateIncidentResponse Response { get; set; } = new(); }
public sealed class CreateIncidentHandler(IRepository<Incident> repository, IRepository<InventoryResource> anchors) : IRequestHandler<CreateIncidentMediatorCommand, CreateIncidentResult>
{
    public async Task<CreateIncidentResult> Handle(CreateIncidentMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var data = new IncidentData { Title = p.Title, Impact = p.Impact, Owner = p.Owner, NextAction = p.NextAction, InternalNote = p.InternalNote, Severity = p.Severity, Status = p.Status };
        var anchor = await InventoryRules.RequireAsync(anchors, p.ResourceExternalId, token);
        var key = anchor.ExternalId.ToString("N");
        if (await repository.ExistsAsync(item => item.OpenKey == key, token)) return new() { Conflict = "Já existe incidente aberto para este recurso; atualize a ocorrência existente." };
        var entity = Incident.Create(anchor, data, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Conflito de atualização. Confira o registro atual." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class CreateIncidentValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentValidator()
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
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.Status).Equal("detected");
    }
}
public sealed class CreateIncidentMediatorValidator : AbstractValidator<CreateIncidentMediatorCommand>
{
    public CreateIncidentMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateIncidentValidator()).OverridePropertyName(string.Empty);
    }
}
