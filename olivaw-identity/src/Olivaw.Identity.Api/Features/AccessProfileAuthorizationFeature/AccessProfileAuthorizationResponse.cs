using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature;

public sealed class AccessProfileAuthorizationResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid AccessProfileId { get; set; }
    public Guid AuthorizationId { get; set; }

    internal static AccessProfileAuthorizationResponse From(AccessProfileAuthorization entity)
    {
        return new AccessProfileAuthorizationResponse
        {
            Id = entity.ExternalId,
            CreatedAt = entity.CreatedAt,
            AccessProfileId = entity.AccessProfile?.ExternalId ?? Guid.Empty,
            AuthorizationId = entity.Authorization?.ExternalId ?? Guid.Empty
        };
    }
}
