using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.CustomerFeature.Internals;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Commands;

public sealed class UpdateCustomerMediatorCommand : IRequest<UpdateCustomerResult>
{
    public UpdateCustomerCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public Guid ExternalId { get; set; }
}
public sealed class UpdateCustomerResult
{
    public string? Conflict { get; set; }
    public Guid? ExistingExternalId { get; set; }
    public UpdateCustomerResponse Response { get; set; } = new();
}
public sealed class UpdateCustomerHandler(IRepository<Customer> repository) : IRequestHandler<UpdateCustomerMediatorCommand, UpdateCustomerResult>
{
    public async Task<UpdateCustomerResult> Handle(UpdateCustomerMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        var customer = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken, item => item.Contacts);
        await CustomerRules.RequireFoundAsync(customer, request.ExternalId, cancellationToken);
        ArgumentNullException.ThrowIfNull(customer);
        if (payload.Revision != customer.Revision) return new UpdateCustomerResult { Conflict = "Este cadastro mudou. Reabra a ficha antes de salvar." };
        if (!Customer.CanTransition(customer.Status, payload.Status))
            return new UpdateCustomerResult { Conflict = "Para reabrir um cliente encerrado, volte para Em implantação." };
        if (!string.IsNullOrEmpty(payload.Document) && await repository.ExistsAsync(item => item.Document == payload.Document && item.Id != customer.Id && item.DeletedAt == null, cancellationToken))
            return new UpdateCustomerResult { Conflict = "Já existe um cliente com este CNPJ." };
        customer.Apply(payload.Business, payload.LegalName, payload.Document, payload.Owner, payload.Notes, payload.Status, request.ActorUserId, request.ActorName, now);
        customer.Contacts.Clear();
        foreach (var contact in payload.Contacts) customer.Contacts.Add(CustomerRules.Contact(contact, request.ActorUserId, now));
        await repository.UpdateAsync(customer, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CustomerRules.IsConflict(exception))
        {
            return new UpdateCustomerResult { Conflict = "O cadastro mudou ou já existe um cliente com este CNPJ ou lead. Recarregue e confira antes de salvar." };
        }
        return new UpdateCustomerResult { Response = new UpdateCustomerResponse { ExternalId = customer.ExternalId, Revision = customer.Revision } };
    }
}
public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(item => item.Business).NotEmpty().MaximumLength(120);
        RuleFor(item => item.LegalName).NotNull().MaximumLength(180);
        RuleFor(item => item.Document).Matches("^[0-9]{14}$").When(item => !string.IsNullOrEmpty(item.Document));
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.Notes).NotNull().MaximumLength(3000);
        RuleFor(item => item.Contacts).NotNull().Must(items => items is { Count: >= 1 and <= 8 });
        RuleForEach(item => item.Contacts).NotNull().SetValidator(new CustomerContactValidator());
        RuleFor(item => item.Revision).GreaterThan(0);
        RuleFor(item => item.Status).Must(value => value is "onboarding" or "active" or "paused" or "closed");
    }
}
public sealed class UpdateCustomerMediatorValidator : AbstractValidator<UpdateCustomerMediatorCommand>
{
    public UpdateCustomerMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotNull().MaximumLength(120);
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateCustomerValidator()).OverridePropertyName(string.Empty);
    }
}
