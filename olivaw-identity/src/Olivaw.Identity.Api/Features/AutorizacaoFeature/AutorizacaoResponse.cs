using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature;

public sealed class AutorizacaoResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    internal static AutorizacaoResponse From(Autorizacao entity)
    {
        return new AutorizacaoResponse
        {
            Id = entity.IdExterno,
            DataCadastro = entity.DataCadastro,
            Chave = entity.Chave,
            Nome = entity.Nome,
            Descricao = entity.Descricao
        };
    }
}
