using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Internals;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;

public sealed class UpdateManagedServiceMediatorCommand : IRequest<UpdateManagedServiceResult>
{
    public Guid ExternalId { get; set; }
    public UpdateManagedServiceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class UpdateManagedServiceResult
{
    public string? Conflict { get; set; }
    public UpdateManagedServiceResponse Response { get; set; } = new();
}
public sealed class UpdateManagedServiceHandler(IRepository<ManagedService> repository, IRepository<Customer> customers, IRepository<ManagedServiceCustomer> links, IRepository<ManagedServiceChange> changes, IRepositoryInventoryRevision inventory)
    : IRequestHandler<UpdateManagedServiceMediatorCommand, UpdateManagedServiceResult>
{
    public async Task<UpdateManagedServiceResult> Handle(UpdateManagedServiceMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        var service = await repository.GetFirstAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        if (service.Revision != payload.Revision) return new() { Conflict = "Este serviço mudou. Reabra a ficha antes de salvar." };
        await inventory.TouchAsync(request.ActorUserId, now, cancellationToken);
        var current = (await links.GetAsync(item => item.ManagedServiceId == service.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken)).ToArray();
        var selected = await ManagedServiceRules.ReadCustomersAsync(payload.CustomerExternalIds, customers, cancellationToken);
        var change = service.Apply(payload.Name, payload.Type, payload.Description, payload.Owner, payload.Scope, payload.Status, request.ActorUserId, request.ActorName, now);
        ManagedServiceRules.SynchronizeLinks(service, current, selected, request.ActorUserId, request.ActorName, now);
        // As consultas genéricas são rastreadas. Salvar diretamente evita marcar o grafo de clientes como modificado.
        // O EF detecta a alteração do serviço, os vínculos encerrados e os novos vínculos na mesma transação.
        if (service.Status == "closed") await inventory.CloseServiceDeploymentsAsync(service.Id, request.ActorUserId, request.ActorName, now, cancellationToken);
        await changes.AddAsync(change, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (ManagedServiceRules.IsConflict(exception))
        {
            return new() { Conflict = "Este serviço mudou ou o identificador já está cadastrado. Reabra a ficha e confira antes de salvar." };
        }
        return new() { Response = new() { ExternalId = service.ExternalId, Revision = service.Revision } };
    }
}
public sealed class UpdateManagedServiceValidator : AbstractValidator<UpdateManagedServiceCommand>
{
    public UpdateManagedServiceValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, long.MaxValue - 1);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Type).Must(ManagedService.IsType);
        RuleFor(item => item.Status).Must(ManagedService.IsStatus);
        RuleFor(item => item.Scope).Must(ManagedService.IsScope);
        RuleFor(item => item.Description).NotNull().MaximumLength(1000);
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.CustomerExternalIds).NotNull().Must(items => items is { Count: <= 100 });
        RuleForEach(item => item.CustomerExternalIds).NotEmpty();
        When(item => item.CustomerExternalIds != null, () =>
        {
            RuleFor(item => item.CustomerExternalIds).Must(items => items.Distinct().Count() == items.Count).WithMessage("Cliente repetido.");
            RuleFor(item => item.CustomerExternalIds).Empty().When(item => item.Scope == "internal");
            RuleFor(item => item.CustomerExternalIds).Empty().When(item => item.Status == "closed");
            RuleFor(item => item.CustomerExternalIds).NotEmpty().When(item => item.Scope == "customer" && item.Status != "closed");
        });
    }
}
public sealed class UpdateManagedServiceMediatorValidator : AbstractValidator<UpdateManagedServiceMediatorCommand>
{
    public UpdateManagedServiceMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateManagedServiceValidator()).OverridePropertyName(string.Empty);
    }
}
