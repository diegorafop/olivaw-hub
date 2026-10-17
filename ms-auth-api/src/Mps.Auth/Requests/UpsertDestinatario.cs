using System;

namespace Mps.Auth.Requests
{
    public class UpsertDestinatarioCommand
    {
        public string? Nome { get; set; }
        public string Endereco { get; set; }
        public Guid UsuarioLogadoId { get; set; }
    }

    public class UpsertDestinatarioCommandResponse
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}