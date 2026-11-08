using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.UsageReadingFeature.Commands;
public sealed class CreateUsageReadingMediatorCommand : IRequest<CreateUsageReadingResult>
{
    public CreateUsageReadingCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateUsageReadingResult { public string? Conflict { get; set; } public CreateUsageReadingResponse Response { get; set; } = new(); }
public sealed class CreateUsageReadingHandler(IRepository<UsageReading> repository, IRepository<InventoryResource> resources) : IRequestHandler<CreateUsageReadingMediatorCommand, CreateUsageReadingResult>
{
    public async Task<CreateUsageReadingResult> Handle(CreateUsageReadingMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var existing = await repository.GetFirstOrDefaultAsync(item => item.InventoryResourceId == resource.Id && item.SampleKey == p.SampleKey, token);
        if (existing != null)
        {
            if (existing.ObservedAt == p.ObservedAt && existing.Source == p.Source && existing.Note == p.Note && existing.Metric == p.Metric && existing.Value == p.Value && existing.Quality == p.Quality) return new() { Response = new() { ExternalId = existing.ExternalId } };
            return new() { Conflict = "A chave desta amostra já foi usada com outros dados." };
        }
        var entity = UsageReading.Create(resource, p.SampleKey, p.ObservedAt, p.Source, p.Note, p.Metric, p.Value, p.Quality, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Amostra concorrente. Reenvie a mesma chave para conferir o resultado." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateUsageReadingValidator : AbstractValidator<CreateUsageReadingCommand>
{
    public CreateUsageReadingValidator()
    {
        RuleFor(item => item.Source).NotNull().MaximumLength(300);
        RuleFor(item => item.Note).NotNull().MaximumLength(1000);
        RuleFor(item => item.Metric).NotNull().MaximumLength(30);
        RuleFor(item => item.Quality).NotNull().MaximumLength(20);
        RuleFor(item => item.Source).NotEmpty();
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.SampleKey).NotEmpty();
        RuleFor(item => item.ObservedAt).Must(value => value.Kind == DateTimeKind.Utc && value <= DateTime.UtcNow);
        RuleFor(item => item.Metric).Must(UsageReading.IsMetric);
        RuleFor(item => item.Quality).Must(UsageReading.IsQuality);
        RuleFor(item => item.Value).InclusiveBetween(0, 1000000000).PrecisionScale(18, 3, true);
    }
}
public sealed class CreateUsageReadingMediatorValidator : AbstractValidator<CreateUsageReadingMediatorCommand>
{
    public CreateUsageReadingMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateUsageReadingValidator()).OverridePropertyName(string.Empty);
    }
}
