using System;
using System.Collections.Generic;

namespace Mps.Auth.Requests
{
    public class EnfileirarEnviarEmailByEnderecoCommand
    {
        public IEnumerable<string> Destinatarios { get; set; }
        public IEnumerable<string> ComCopias { get; set; }
        public IEnumerable<Guid> Anexos { get; set; }
        public string Assunto { get; set; }
        public string Mensagem { get; set; }
    }

    public class EnfileirarEnviarEmailByEnderecoCommandResponse
    {
        public Guid Id { get; set; }
        public string Assunto { get; set; }
    }
}