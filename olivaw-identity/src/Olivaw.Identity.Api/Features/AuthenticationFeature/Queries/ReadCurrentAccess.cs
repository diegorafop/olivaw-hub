using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Features.AuthenticationFeature.Queries;

public sealed class ReadCurrentAccessQuery : IRequest<ReadCurrentAccessResponse> { }

public sealed class ReadCurrentAccessResponse
{
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsServiceAccount { get; set; }
    public bool IsRoot { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}

public sealed class ReadCurrentAccessHandler(IIdentityStore store, CurrentActor actor)
    : IRequestHandler<ReadCurrentAccessQuery, ReadCurrentAccessResponse>
{
    public async Task<ReadCurrentAccessResponse> Handle(ReadCurrentAccessQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var id = actor.Id;
        var usuario = await store.FindAsync<UserProfile>(item => item.ExternalId == id, cancellationToken)
            ?? throw new IdentityFailure(401, "Autenticação obrigatória.");
        return new ReadCurrentAccessResponse
        {
            UserProfileId = usuario.ExternalId,
            Name = usuario.Name,
            IsServiceAccount = usuario.IsServiceAccount,
            IsRoot = usuario.IsRoot,
            Permissions = await store.PermissionsAsync(usuario.Id, cancellationToken)
        };
    }
}
