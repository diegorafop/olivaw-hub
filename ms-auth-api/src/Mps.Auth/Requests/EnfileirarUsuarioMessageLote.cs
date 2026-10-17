using Mps.Auth.Domain.Enum;
using System;

namespace Mps.Auth.Requests
{
    public class EnfileirarUsuarioMessageCommand
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataRemocao { get; set; }

        public string Login { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int Matricula { get; set; }
        public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }
    }

    public class EnfileirarUsuarioMessageCommandResponse { }
}