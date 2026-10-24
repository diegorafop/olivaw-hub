using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature;

public sealed class UserProfileAccessProfileResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid AccessProfileId { get; set; }

    internal static UserProfileAccessProfileResponse From(UserProfileAccessProfile entity)
    {
        return new UserProfileAccessProfileResponse
        {
            Id = entity.ExternalId,
            CreatedAt = entity.CreatedAt,
            UserProfileId = entity.UserProfile?.ExternalId ?? Guid.Empty,
            AccessProfileId = entity.AccessProfile?.ExternalId ?? Guid.Empty
        };
    }
}
