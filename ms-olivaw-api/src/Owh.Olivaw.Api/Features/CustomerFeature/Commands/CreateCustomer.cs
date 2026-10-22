using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.CustomerFeature.Internals;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Commands;

public sealed class CreateCustomerMediatorCommand : IRequest<CreateCustomerResult>
{
    public CreateCustomerCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateCustomerResult
{
    public string? Conflict { get; set; }
    public Guid? ExistingExternalId { get; set; }
    public CreateCustomerResponse Response { get; set; } = new();
}
public sealed class CreateCustomerHandler(IRepository<Customer> repository, IRepository<Lead> leads) : IRequestHandler<CreateCustomerMediatorCommand, CreateCustomerResult>
{
    public async Task<CreateCustomerResult> Handle(CreateCustomerMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        Lead? source = null;
        if (payload.SourceLeadExternalId.HasValue)
        {
            source = await leads.GetFirstOrDefaultAsync(item => item.ExternalId == payload.SourceLeadExternalId.Value && item.DeletedAt == null, cancellationToken);
            await CustomerRules.RequireFoundAsync(source, payload.SourceLeadExternalId.Value, cancellationToken);
            ArgumentNullException.ThrowIfNull(source);
            var existing = await repository.GetFirstOrDefaultAsync(item => item.SourceLeadId == source.Id && item.DeletedAt == null, cancellationToken);
            if (existing != null) return new CreateCustomerResult { Conflict = "Este lead já está vinculado a um cliente.", ExistingExternalId = existing.ExternalId };
        }
        if (!string.IsNullOrEmpty(payload.Document) && await repository.ExistsAsync(item => item.Document == payload.Document && item.DeletedAt == null, cancellationToken))
            return new CreateCustomerResult { Conflict = "Já existe um cliente com este CNPJ." };
        var customer = Customer.Create(payload.Business, payload.LegalName, payload.Document, payload.Owner, payload.Notes, request.ActorUserId, request.ActorName, now, source);
        foreach (var contact in payload.Contacts) customer.Contacts.Add(CustomerRules.Contact(contact, request.ActorUserId, now));
        await repository.AddAsync(customer, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CustomerRules.IsConflict(exception))
        {
            return new CreateCustomerResult { Conflict = "O cadastro mudou ou já existe um cliente com este CNPJ ou lead. Recarregue e confira antes de salvar." };
        }
        return new CreateCustomerResult { Response = new CreateCustomerResponse { ExternalId = customer.ExternalId, Revision = customer.Revision } };
    }
}
public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(item => item.Business).NotEmpty().MaximumLength(120);
        RuleFor(item => item.LegalName).NotNull().MaximumLength(180);
        RuleFor(item => item.Document).Matches("^[0-9]{14}$").When(item => !string.IsNullOrEmpty(item.Document));
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.Notes).NotNull().MaximumLength(3000);
        RuleFor(item => item.Contacts).NotNull().Must(items => items is { Count: >= 1 and <= 8 });
        RuleForEach(item => item.Contacts).NotNull().SetValidator(new CustomerContactValidator());
        RuleFor(item => item.SourceLeadExternalId).NotEqual(Guid.Empty).When(item => item.SourceLeadExternalId.HasValue);
    }
}
public sealed class CreateCustomerMediatorValidator : AbstractValidator<CreateCustomerMediatorCommand>
{
    public CreateCustomerMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotNull().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateCustomerValidator()).OverridePropertyName(string.Empty);
    }
}
