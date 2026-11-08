using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Services;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
public sealed class CreateHttpHealthReadingMediatorCommand : IRequest<CreateHttpHealthReadingResult>
{
    public CreateHttpHealthReadingCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateHttpHealthReadingResult { public string? Conflict { get; set; } public CreateHttpHealthReadingResponse Response { get; set; } = new(); }
public sealed class CreateHttpHealthReadingHandler(IRepository<HealthReading> repository, IRepository<InventoryResource> resources, PublicHealthProbe probe) : IRequestHandler<CreateHttpHealthReadingMediatorCommand, CreateHttpHealthReadingResult>
{
    public async Task<CreateHttpHealthReadingResult> Handle(CreateHttpHealthReadingMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        if (!probe.IsAllowed(resource.ReferenceUrl)) return new() { Conflict = "Defina e autorize a URL HTTPS deste recurso na configuração de monitoramento antes de coletar." };
        var existing = await repository.GetFirstOrDefaultAsync(item => item.InventoryResourceId == resource.Id && item.SampleKey == p.SampleKey, token);
        if (existing != null) return new() { Response = new() { ExternalId = existing.ExternalId } };
        var outcome = await probe.ReadAsync(resource.ReferenceUrl, token);
        var now = DateTime.UtcNow;
        var reading = HealthReading.Create(resource, p.SampleKey, now, "Verificação HTTP externa", resource.ReferenceUrl + " — resposta pontual; não comprova os fluxos de negócio.", outcome, 15, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(reading, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Coleta concorrente. Reenvie a mesma chave para conferir o resultado." };
        return new() { Response = new() { ExternalId = reading.ExternalId } };
    }
}
public sealed class CreateHttpHealthReadingValidator : AbstractValidator<CreateHttpHealthReadingCommand>
{
    public CreateHttpHealthReadingValidator() { RuleFor(item => item.ResourceExternalId).NotEmpty(); RuleFor(item => item.SampleKey).NotEmpty(); }
}
public sealed class CreateHttpHealthReadingMediatorValidator : AbstractValidator<CreateHttpHealthReadingMediatorCommand>
{
    public CreateHttpHealthReadingMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateHttpHealthReadingValidator()).OverridePropertyName(string.Empty);
    }
}
