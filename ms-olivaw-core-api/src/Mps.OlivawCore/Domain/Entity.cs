using System;

namespace Mps.OlivawCore.Domain;

public abstract class Entity
{
    public int Id { get; set; }
    public Guid ExternalId { get; set; }
    public Guid UsuarioLogadoId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
