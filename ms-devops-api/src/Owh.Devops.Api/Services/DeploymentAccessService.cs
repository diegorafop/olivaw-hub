using Owh.Devops.Services;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Owh.Devops.Api.Services;

public sealed class DeploymentAccessService(JwtIdentitySession session) : IDeploymentAccessService
{
    public void EnsureCanDeploy(string folder)
    {
        var access = session.Access;
        if (access is null) throw new IdentityFailure(401, "Autenticação obrigatória.");
        if (access.IsRoot) return;
        if (!access.Permissions.Contains("devops.deploy", StringComparer.Ordinal)) throw Denied();
        if (string.IsNullOrWhiteSpace(folder)) throw Denied();
        if (!access.Permissions.Contains($"devops.deploy:{folder}", StringComparer.Ordinal)) throw Denied();
    }

    private static IdentityFailure Denied() => new(403, "Sem permissão de deploy para este serviço.");
}
