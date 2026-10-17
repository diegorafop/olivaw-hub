using System;

namespace Mps.Auth.Requests
{
    public class SelecionarUsuarioCargoByUsuarioIdQuery
    {
        public Guid Id { get; set; }
    }

    public class SelecionarUsuarioCargoByUsuarioIdQueryResponse
    {
        public Guid UsuarioId { get; set; }
        public Guid CargoId { get; set; }
        public string CargoNome { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataRemocao { get; set; }
    }
}