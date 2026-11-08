using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class CreateResourceCustomerMediatorCommand : IRequest<CreateResourceCustomerResult>
{
    public CreateResourceCustomerCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateResourceCustomerResult { public string? Conflict { get; set; } public CreateResourceCustomerResponse Response { get; set; } = new(); }
public sealed class CreateResourceCustomerHandler(IRepository<ResourceCustomer> repository, IRepository<InventoryResource> resources, IRepository<Customer> targets, IRepositoryInventoryRevision inventory) : IRequestHandler<CreateResourceCustomerMediatorCommand, CreateResourceCustomerResult>
{
    public async Task<CreateResourceCustomerResult> Handle(CreateResourceCustomerMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var source = await InventoryRules.RequireAsync(resources, payload.ResourceExternalId, cancellationToken);
        var target = await InventoryRules.RequireAsync(targets, payload.TargetExternalId, cancellationToken);
        InventoryRules.Require(source.Status != "retired", "ResourceExternalId", "Recurso desativado.");
        if (await repository.ExistsAsync(item => item.InventoryResourceId == source.Id && item.CustomerId == target.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken)) return new() { Conflict = "Este vínculo já está vigente." };
        var link = ResourceCustomer.Create(source, target, payload.Purpose, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(link, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou. Atualize a ficha antes de tentar novamente." };
        return new() { Response = new() { ExternalId = link.ExternalId, Revision = link.Revision } };
    }
}
public sealed class CreateResourceCustomerValidator : AbstractValidator<CreateResourceCustomerCommand>
{
    public CreateResourceCustomerValidator()
    {
        RuleFor(item => item.ResourceExternalId).NotEmpty();
        RuleFor(item => item.TargetExternalId).NotEmpty();
        RuleFor(item => item.Purpose).NotEmpty().MaximumLength(300);
    }
}
public sealed class CreateResourceCustomerMediatorValidator : AbstractValidator<CreateResourceCustomerMediatorCommand>
{
    public CreateResourceCustomerMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateResourceCustomerValidator()).OverridePropertyName(string.Empty);
    }
}
