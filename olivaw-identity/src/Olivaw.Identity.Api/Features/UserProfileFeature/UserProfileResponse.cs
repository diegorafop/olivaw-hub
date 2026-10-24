using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.UserProfileFeature;

public sealed class UserProfileResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsServiceAccount { get; set; }
    public bool IsActive { get; set; } = true;

    internal static UserProfileResponse From(UserProfile entity)
    {
        return new UserProfileResponse
        {
            Id = entity.ExternalId,
            CreatedAt = entity.CreatedAt,
            Login = entity.Login,
            Name = entity.Name,
            Email = entity.Email,
            IsServiceAccount = entity.IsServiceAccount,
            IsActive = entity.IsActive
        };
    }
}
