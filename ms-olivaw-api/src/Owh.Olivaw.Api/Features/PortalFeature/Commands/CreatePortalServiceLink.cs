using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalServiceLinkMediatorCommand : IRequest<CreatePortalServiceLinkResult> { public CreatePortalServiceLinkCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } }
public sealed class CreatePortalServiceLinkResult { public string? Conflict { get; set; } public CreatePortalServiceLinkResponse Response { get; set; } = new(); }
public sealed class CreatePortalServiceLinkHandler(IRepository<PortalServiceLink> links, IRepository<Customer> customers, IRepository<ManagedService> services, IRepository<ManagedServiceCustomer> associations, IRepository<InventoryResource> resources, IRepositoryInventoryResource graph) : IRequestHandler<CreatePortalServiceLinkMediatorCommand, CreatePortalServiceLinkResult>
{
    public async Task<CreatePortalServiceLinkResult> Handle(CreatePortalServiceLinkMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        var service = await InventoryRules.RequireAsync(services, p.ServiceExternalId, token);
        if (!await associations.ExistsAsync(item => item.CustomerId == customer.Id && item.ManagedServiceId == service.Id && item.EndedAt == null, token)) return new() { Conflict = "Selecione um serviço vinculado a esta empresa." };
        InventoryResource? resource = null;
        if (p.HealthResourceExternalId.HasValue)
        {
            resource = await InventoryRules.RequireAsync(resources, p.HealthResourceExternalId.Value, token);
            var context = await graph.ReadContextAsync(resource.Id, token); InventoryRules.RequireGraph(context.LimitExceeded);
            if (!context.Customers.Any(item => item.Id == customer.Id)) return new() { Conflict = "A fonte de saúde não está vinculada a esta empresa." };
        }
        if (await links.ExistsAsync(item => item.CustomerId == customer.Id && item.ManagedServiceId == service.Id, token)) return new() { Conflict = "Este serviço já foi configurado para o portal." };
        var link = PortalServiceLink.Create(customer, service, resource, p.Label, p.PublicUrl, request.ActorUserId, DateTime.UtcNow);
        await links.AddAsync(link, token); if (!await InventoryRules.SaveAsync(links, token)) return new() { Conflict = "O serviço já foi configurado. Atualize a lista." };
        return new() { Response = new() { ExternalId = link.ExternalId } };
    }
}
public sealed class CreatePortalServiceLinkValidator : AbstractValidator<CreatePortalServiceLinkCommand>
{
    public CreatePortalServiceLinkValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.ServiceExternalId).NotEmpty(); RuleFor(item => item.HealthResourceExternalId).NotEqual(Guid.Empty); RuleFor(item => item.Label).NotEmpty().MaximumLength(120); RuleFor(item => item.PublicUrl).NotNull().MaximumLength(300).Must(InventoryResourceData.IsReferenceUrl).Must(value => value == "" || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)); }
}
public sealed class CreatePortalServiceLinkMediatorValidator : AbstractValidator<CreatePortalServiceLinkMediatorCommand>
{
    public CreatePortalServiceLinkMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePortalServiceLinkValidator()).OverridePropertyName(string.Empty); }
}
