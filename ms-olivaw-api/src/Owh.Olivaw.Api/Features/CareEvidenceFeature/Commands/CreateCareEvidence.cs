using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.CareEvidenceFeature.Commands;
public sealed class CreateCareEvidenceMediatorCommand : IRequest<CreateCareEvidenceResult>
{
    public CreateCareEvidenceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateCareEvidenceResult { public string? Conflict { get; set; } public CreateCareEvidenceResponse Response { get; set; } = new(); }
public sealed class CreateCareEvidenceHandler(IRepository<CareEvidence> repository, IRepository<InventoryResource> resources) : IRequestHandler<CreateCareEvidenceMediatorCommand, CreateCareEvidenceResult>
{
    public async Task<CreateCareEvidenceResult> Handle(CreateCareEvidenceMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var existing = await repository.GetFirstOrDefaultAsync(item => item.InventoryResourceId == resource.Id && item.SampleKey == p.SampleKey, token);
        if (existing != null)
        {
            if (existing.ObservedAt == p.ObservedAt && existing.Source == p.Source && existing.Note == p.Note && existing.Kind == p.Kind && existing.Outcome == p.Outcome && existing.ReferenceUrl == p.ReferenceUrl) return new() { Response = new() { ExternalId = existing.ExternalId } };
            return new() { Conflict = "A chave desta amostra já foi usada com outros dados." };
        }
        var entity = CareEvidence.Create(resource, p.SampleKey, p.ObservedAt, p.Source, p.Note, p.Kind, p.Outcome, p.ReferenceUrl, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Amostra concorrente. Reenvie a mesma chave para conferir o resultado." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateCareEvidenceValidator : AbstractValidator<CreateCareEvidenceCommand>
{
    public CreateCareEvidenceValidator()
    {
        RuleFor(item => item.Source).NotNull().MaximumLength(300);
        RuleFor(item => item.Note).NotNull().MaximumLength(1000);
        RuleFor(item => item.Kind).NotNull().MaximumLength(20);
        RuleFor(item => item.Outcome).NotNull().MaximumLength(20);
        RuleFor(item => item.ReferenceUrl).NotNull().MaximumLength(300);
        RuleFor(item => item.Source).NotEmpty();
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.SampleKey).NotEmpty();
        RuleFor(item => item.ObservedAt).Must(value => value.Kind == DateTimeKind.Utc && value <= DateTime.UtcNow);
        RuleFor(item => item.Kind).Must(CareEvidence.IsKind);
        RuleFor(item => item.Outcome).Must(CareEvidence.IsOutcome);
        RuleFor(item => item.ReferenceUrl).Must(InventoryResourceData.IsReferenceUrl);
    }
}
public sealed class CreateCareEvidenceMediatorValidator : AbstractValidator<CreateCareEvidenceMediatorCommand>
{
    public CreateCareEvidenceMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateCareEvidenceValidator()).OverridePropertyName(string.Empty);
    }
}
