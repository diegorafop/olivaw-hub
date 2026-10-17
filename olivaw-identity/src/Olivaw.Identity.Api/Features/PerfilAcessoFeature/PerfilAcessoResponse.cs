using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature;

public sealed class PerfilAcessoResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;

    internal static PerfilAcessoResponse From(PerfilAcesso entity)
    {
        return new PerfilAcessoResponse
        {
            Id = entity.IdExterno,
            DataCadastro = entity.DataCadastro,
            Nome = entity.Nome,
            Descricao = entity.Descricao,
            Ativo = entity.Ativo
        };
    }
}
