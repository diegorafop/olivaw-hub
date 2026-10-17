using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.UsuarioFeature;

public sealed class UsuarioResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    internal static UsuarioResponse From(Usuario entity)
    {
        return new UsuarioResponse
        {
            Id = entity.IdExterno,
            DataCadastro = entity.DataCadastro,
            Login = entity.Login,
            Nome = entity.Nome,
            Email = entity.Email,
            Ativo = entity.Ativo
        };
    }
}
