using System;

namespace Mps.OlivawCore.Domain;

public abstract class Entity
{
    public int Id { get; set; }
    public Guid IdExterno { get; set; }
    public Guid UsuarioLogadoId { get; set; }

    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}