using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
public sealed class CreateHealthReadingMediatorCommand : IRequest<CreateHealthReadingResult>
{
    public CreateHealthReadingCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateHealthReadingResult { public string? Conflict { get; set; } public CreateHealthReadingResponse Response { get; set; } = new(); }
public sealed class CreateHealthReadingHandler(IRepository<HealthReading> repository, IRepository<InventoryResource> resources) : IRequestHandler<CreateHealthReadingMediatorCommand, CreateHealthReadingResult>
{
    public async Task<CreateHealthReadingResult> Handle(CreateHealthReadingMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var existing = await repository.GetFirstOrDefaultAsync(item => item.InventoryResourceId == resource.Id && item.SampleKey == p.SampleKey, token);
        if (existing != null)
        {
            if (existing.ObservedAt == p.ObservedAt && existing.Source == p.Source && existing.Note == p.Note && existing.Outcome == p.Outcome && existing.MaxAgeMinutes == p.MaxAgeMinutes) return new() { Response = new() { ExternalId = existing.ExternalId } };
            return new() { Conflict = "A chave desta amostra já foi usada com outros dados." };
        }
        var entity = HealthReading.Create(resource, p.SampleKey, p.ObservedAt, p.Source, p.Note, p.Outcome, p.MaxAgeMinutes, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Amostra concorrente. Reenvie a mesma chave para conferir o resultado." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateHealthReadingValidator : AbstractValidator<CreateHealthReadingCommand>
{
    public CreateHealthReadingValidator()
    {
        RuleFor(item => item.Source).NotNull().MaximumLength(300);
        RuleFor(item => item.Note).NotNull().MaximumLength(1000);
        RuleFor(item => item.Outcome).NotNull().MaximumLength(30);
        RuleFor(item => item.Source).NotEmpty();
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.SampleKey).NotEmpty();
        RuleFor(item => item.ObservedAt).Must(value => value.Kind == DateTimeKind.Utc && value <= DateTime.UtcNow);
        RuleFor(item => item.Outcome).Must(HealthReading.IsOutcome);
        RuleFor(item => item.MaxAgeMinutes).InclusiveBetween(1, 1440);
    }
}
public sealed class CreateHealthReadingMediatorValidator : AbstractValidator<CreateHealthReadingMediatorCommand>
{
    public CreateHealthReadingMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateHealthReadingValidator()).OverridePropertyName(string.Empty);
    }
}
