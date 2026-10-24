using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Internals;
public sealed class PortalAccess(IRepository<PortalMembership> members)
{
    public async Task<PortalMembership> RequireAsync(Guid user, Guid customer, bool write, bool billing, CancellationToken token)
    {
        var member = await members.GetFirstOrDefaultAsync(item => item.PortalUserExternalId == user && item.Customer.ExternalId == customer && item.IsActive && item.DeletedAt == null && item.Customer.DeletedAt == null, token, item => item.Customer);
        if (member == null) throw new IdentityFailure(403, "Acesso não autorizado a esta empresa.");
        if (write && !member.CanRequest) throw new IdentityFailure(403, "Este acesso permite somente consulta.");
        if (billing && !member.CanReadBilling) throw new IdentityFailure(403, "Este acesso não permite consultar cobranças.");
        return member;
    }
}
