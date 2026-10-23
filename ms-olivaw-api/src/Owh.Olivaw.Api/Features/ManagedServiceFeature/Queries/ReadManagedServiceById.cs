using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;

public sealed class ReadManagedServiceByIdMediatorQuery : IRequest<ReadManagedServiceByIdResponse>
{
    public ReadManagedServiceByIdQuery Payload { get; set; } = new();
}
public sealed class ReadManagedServiceByIdHandler(IRepository<ManagedService> repository, IRepository<ManagedServiceCustomer> links, IRepository<ManagedServiceChange> changes)
    : IRequestHandler<ReadManagedServiceByIdMediatorQuery, ReadManagedServiceByIdResponse>
{
    public async Task<ReadManagedServiceByIdResponse> Handle(ReadManagedServiceByIdMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var service = await repository.GetFirstAsync(item => item.ExternalId == request.Payload.ExternalId && item.DeletedAt == null, cancellationToken);
        var current = await links.GetAsync(item => item.ManagedServiceId == service.Id && item.EndedAt == null && item.DeletedAt == null, cancellationToken, item => item.Customer);
        var previous = await links.TakeOrderedAsync(item => item.ManagedServiceId == service.Id && item.EndedAt != null && item.DeletedAt == null, 20, item => item.Id, cancellationToken, item => item.Customer);
        var history = await changes.TakeOrderedAsync(item => item.ManagedServiceId == service.Id && item.DeletedAt == null, 20, item => item.Revision, cancellationToken);
        return new()
        {
            ExternalId = service.ExternalId, TechnicalId = service.TechnicalId, Name = service.Name, Revision = service.Revision,
            Type = service.Type, Scope = service.Scope, Status = service.Status, Owner = service.Owner, Description = service.Description,
            Customers = current.Select(MapLink).ToList(), PreviousCustomers = previous.Select(MapLink).ToList(),
            Changes = history.Select(item => new ManagedServiceChangeItem
            {
                Revision = item.Revision, ActorName = item.ActorName, CreatedAt = item.CreatedAt,
                PreviousStatus = item.PreviousStatus, Status = item.Status, Name = item.Name, Type = item.Type,
                Scope = item.Scope, Description = item.Description, Owner = item.Owner
            }).ToList()
        };
    }
    private static ManagedServiceCustomerItem MapLink(ManagedServiceCustomer link) => new()
    {
        CustomerExternalId = link.Customer.ExternalId, Business = link.Customer.Business, StartedAt = link.StartedAt, EndedAt = link.EndedAt, StartedByName = link.StartedByName, EndedByName = link.EndedByName
    };
}
public sealed class ReadManagedServiceByIdValidator : AbstractValidator<ReadManagedServiceByIdQuery>
{
    public ReadManagedServiceByIdValidator() { RuleFor(item => item.ExternalId).NotEmpty(); }
}
public sealed class ReadManagedServiceByIdMediatorValidator : AbstractValidator<ReadManagedServiceByIdMediatorQuery>
{
    public ReadManagedServiceByIdMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadManagedServiceByIdValidator()).OverridePropertyName(string.Empty);
    }
}
