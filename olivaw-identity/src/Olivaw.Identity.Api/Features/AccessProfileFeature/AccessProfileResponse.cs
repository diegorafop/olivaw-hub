using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature;

public sealed class AccessProfileResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    internal static AccessProfileResponse From(AccessProfile entity)
    {
        return new AccessProfileResponse
        {
            Id = entity.ExternalId,
            CreatedAt = entity.CreatedAt,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
