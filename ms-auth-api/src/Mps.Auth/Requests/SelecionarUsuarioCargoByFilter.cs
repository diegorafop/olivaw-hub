using Mps.Auth.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Mps.Auth.Requests
{
    public class SelecionarUsuarioCargoByFilterQuery
    {
        public Guid? CargoId { get; set; }
        public Guid? UsuarioId { get; set; }

        public TipoBoleanoEnum? Gestor { get; set; }

        public IEnumerable<Guid>? UsuarioIds { get; set; }
        public IEnumerable<Guid>? CargoIds { get; set; }
    }

    public class SelecionarUsuarioCargoByFilterQueryResponse
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataRemocao { get; set; }
        public Guid UsuarioLogadoId { get; set; }

        public Guid CargoId { get; set; }
        public Guid UsuarioId { get; set; }

        public string CargoNome { get; set; }

        public TipoBoleanoEnum Gestor { get; set; }
    }
}