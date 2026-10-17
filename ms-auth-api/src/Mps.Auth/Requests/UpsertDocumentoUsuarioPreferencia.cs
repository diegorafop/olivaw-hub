using System;

namespace Mps.Auth.Requests
{
    public class UpsertDocumentoUsuarioPreferenciaCommand
    {
        public Guid UsuarioId { get; set; }

        public bool SeguirAutomaticoInteressado { get; set; }
        public bool SeguirAutomaticoGestor { get; set; }

        public bool FavoritarArquivar { get; set; }
        public bool FavoritarFluxoResumido { get; set; }
        public bool FavoritarPrazo { get; set; }
        public bool FavoritarSeguir { get; set; }

        public bool VisualizarReferencia { get; set; }

        public Guid UsuarioLogadoIdId { get; set; }
    }

    public class UpsertDocumentoUsuarioPreferenciaCommandResponse
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataRemocao { get; set; }
        public Guid UsuarioLogadoId { get; set; }

        public Guid UsuarioId { get; set; }

        public bool SeguirAutomaticoInteressado { get; set; }
        public bool SeguirAutomaticoGestor { get; set; }

        public bool FavoritarArquivar { get; set; }
        public bool FavoritarFluxoResumido { get; set; }
        public bool FavoritarPrazo { get; set; }
        public bool FavoritarSeguir { get; set; }
    }
}