using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.CustomerFeature.Internals;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Queries;

public sealed class ReadCustomerByIdMediatorQuery : IRequest<ReadCustomerByIdResponse>
{
    public ReadCustomerByIdQuery Payload { get; set; } = new();
}
public sealed class ReadCustomerByIdHandler(IRepository<Customer> repository, IRepository<CustomerChange> changes) : IRequestHandler<ReadCustomerByIdMediatorQuery, ReadCustomerByIdResponse>
{
    public async Task<ReadCustomerByIdResponse> Handle(ReadCustomerByIdMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var customer = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ExternalId && item.DeletedAt == null, cancellationToken, item => item.Contacts, item => item.SourceLead!);
        await CustomerRules.RequireFoundAsync(customer, request.Payload.ExternalId, cancellationToken);
        ArgumentNullException.ThrowIfNull(customer);
        var history = await changes.TakeOrderedAsync(item => item.CustomerId == customer.Id, 20, item => item.Id, cancellationToken);
        return new ReadCustomerByIdResponse
        {
            ExternalId = customer.ExternalId, SourceLeadExternalId = customer.SourceLead?.ExternalId, Revision = customer.Revision,
            Business = customer.Business, LegalName = customer.LegalName, Document = customer.Document, Owner = customer.Owner, Notes = customer.Notes,
            Status = customer.Status, CreatedAt = customer.CreatedAt, UpdatedAt = customer.UpdatedAt,
            Contacts = customer.Contacts.OrderBy(item => item.Id).Select(item => new CustomerContactInput { Name = item.Name, Role = item.Role, Email = item.Email, Whatsapp = item.Whatsapp }).ToList(),
            Changes = history.Select(item => new CustomerChangeItem { At = item.CreatedAt, Actor = item.ActorName, Action = item.Action, PreviousStatus = item.PreviousStatus, Status = item.Status }).ToList()
        };
    }
}
public sealed class ReadCustomerByIdValidator : AbstractValidator<ReadCustomerByIdQuery>
{
    public ReadCustomerByIdValidator() { RuleFor(item => item.ExternalId).NotEmpty(); }
}
public sealed class ReadCustomerByIdMediatorValidator : AbstractValidator<ReadCustomerByIdMediatorQuery>
{
    public ReadCustomerByIdMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadCustomerByIdValidator()).OverridePropertyName(string.Empty); }
}
