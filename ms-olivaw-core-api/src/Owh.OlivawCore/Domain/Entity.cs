using System;

namespace Owh.OlivawCore.Domain;

public abstract class Entity
{
    public int Id { get; set; }
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
