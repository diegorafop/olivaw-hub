using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class UpdatePortalServiceLinkMediatorCommand : IRequest<UpdatePortalServiceLinkResult> { public Guid ExternalId { get; set; } public Guid ActorUserId { get; set; } public UpdatePortalServiceLinkCommand Payload { get; set; } = new(); }
public sealed class UpdatePortalServiceLinkResult { public string? Conflict { get; set; } public UpdatePortalServiceLinkResponse Response { get; set; } = new(); }
public sealed class UpdatePortalServiceLinkHandler(IRepository<PortalServiceLink> links, IRepository<InventoryResource> resources, IRepositoryInventoryResource graph) : IRequestHandler<UpdatePortalServiceLinkMediatorCommand, UpdatePortalServiceLinkResult>
{
    public async Task<UpdatePortalServiceLinkResult> Handle(UpdatePortalServiceLinkMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var link = await InventoryRules.RequireAsync(links, request.ExternalId, token);
        if (link.Revision != p.Revision) return new() { Conflict = "Este serviço mudou. Reabra a configuração." };
        InventoryResource? resource = null;
        if (p.HealthResourceExternalId.HasValue)
        {
            resource = await InventoryRules.RequireAsync(resources, p.HealthResourceExternalId.Value, token);
            var context = await graph.ReadContextAsync(resource.Id, token); InventoryRules.RequireGraph(context.LimitExceeded);
            if (!context.Customers.Any(item => item.Id == link.CustomerId)) return new() { Conflict = "A fonte de saúde não está vinculada a esta empresa." };
        }
        link.Update(resource, p.Label, p.PublicUrl, p.IsActive, request.ActorUserId, DateTime.UtcNow); await links.UpdateAsync(link, token);
        if (!await InventoryRules.SaveAsync(links, token)) return new() { Conflict = "Este serviço mudou. Reabra a configuração." };
        return new() { Response = new() { ExternalId = link.ExternalId, Revision = link.Revision } };
    }
}
public sealed class UpdatePortalServiceLinkValidator : AbstractValidator<UpdatePortalServiceLinkCommand>
{
    public UpdatePortalServiceLinkValidator() { RuleFor(item => item.Revision).GreaterThan(0); RuleFor(item => item.HealthResourceExternalId).NotEqual(Guid.Empty); RuleFor(item => item.Label).NotEmpty().MaximumLength(120); RuleFor(item => item.PublicUrl).NotNull().MaximumLength(300).Must(InventoryResourceData.IsReferenceUrl).Must(value => value == "" || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)); }
}
public sealed class UpdatePortalServiceLinkMediatorValidator : AbstractValidator<UpdatePortalServiceLinkMediatorCommand> { public UpdatePortalServiceLinkMediatorValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new UpdatePortalServiceLinkValidator()).OverridePropertyName(string.Empty); } }
