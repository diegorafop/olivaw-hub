using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature;

public sealed class AuthorizationResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    internal static AuthorizationResponse From(Authorization entity)
    {
        return new AuthorizationResponse
        {
            Id = entity.ExternalId,
            CreatedAt = entity.CreatedAt,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}
