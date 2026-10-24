using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Internals;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;

public sealed class CreateManagedServiceMediatorCommand : IRequest<CreateManagedServiceResult>
{
    public CreateManagedServiceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateManagedServiceResult
{
    public string? Conflict { get; set; }
    public CreateManagedServiceResponse Response { get; set; } = new();
}
public sealed class CreateManagedServiceHandler(IRepository<ManagedService> repository, IRepository<Customer> customers)
    : IRequestHandler<CreateManagedServiceMediatorCommand, CreateManagedServiceResult>
{
    public async Task<CreateManagedServiceResult> Handle(CreateManagedServiceMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        if (await repository.ExistsAsync(item => item.TechnicalId == payload.TechnicalId, cancellationToken))
            return new() { Conflict = "Este identificador técnico já está cadastrado." };
        var service = ManagedService.Create(payload.TechnicalId, payload.Name, payload.Type, payload.Description, payload.Owner, payload.Scope, payload.Status, request.ActorUserId, request.ActorName, now);
        var current = Array.Empty<ManagedServiceCustomer>();
        var selected = await ManagedServiceRules.ReadCustomersAsync(payload.CustomerExternalIds, customers, cancellationToken);
        ManagedServiceRules.SynchronizeLinks(service, current, selected, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(service, cancellationToken);
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
public sealed class CreateManagedServiceValidator : AbstractValidator<CreateManagedServiceCommand>
{
    public CreateManagedServiceValidator()
    {
        RuleFor(item => item.TechnicalId).NotEmpty().MaximumLength(80).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
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
public sealed class CreateManagedServiceMediatorValidator : AbstractValidator<CreateManagedServiceMediatorCommand>
{
    public CreateManagedServiceMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateManagedServiceValidator()).OverridePropertyName(string.Empty);
    }
}
