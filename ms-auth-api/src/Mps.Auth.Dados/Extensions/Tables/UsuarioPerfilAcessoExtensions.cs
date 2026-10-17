using Microsoft.EntityFrameworkCore;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

internal static class UsuarioPerfilAcessoExtensions
{
    internal static void SetupUsuarioPerfilAcesso(this ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UsuarioPerfilAcesso>()
            .HasIndex(item => item.IdExterno);

        modelBuilder
            .Entity<UsuarioPerfilAcesso>()
            .HasOne(item => item.PerfilAcesso);

        modelBuilder
            .Entity<UsuarioPerfilAcesso>()
            .HasOne(item => item.Usuario);
    }
}