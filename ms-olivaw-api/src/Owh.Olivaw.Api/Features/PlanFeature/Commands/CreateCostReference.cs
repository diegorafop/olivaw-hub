using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreateCostReferenceMediatorCommand : IRequest<CreateCostReferenceResult>
{
    public CreateCostReferenceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateCostReferenceResult { public string? Conflict { get; set; } public CreateCostReferenceResponse Response { get; set; } = new(); }
public sealed class CreateCostReferenceHandler(IRepository<CostReference> repository) : IRequestHandler<CreateCostReferenceMediatorCommand, CreateCostReferenceResult>
{
    public async Task<CreateCostReferenceResult> Handle(CreateCostReferenceMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var entity = CostReference.Create(p.Name, p.MonthlyCost, p.Source, p.EffectiveAt, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Já existe uma versão nesta vigência. Confira o histórico." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateCostReferenceValidator : AbstractValidator<CreateCostReferenceCommand>
{
    public CreateCostReferenceValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.MonthlyCost).InclusiveBetween(0, 1000000000).PrecisionScale(18, 2, true);
        RuleFor(item => item.Source).NotEmpty().MaximumLength(300);
        RuleFor(item => item.EffectiveAt).Must(value => value.Kind == DateTimeKind.Utc && value <= DateTime.UtcNow);
    }
}
public sealed class CreateCostReferenceMediatorValidator : AbstractValidator<CreateCostReferenceMediatorCommand>
{
    public CreateCostReferenceMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateCostReferenceValidator()).OverridePropertyName(string.Empty);
    }
}
