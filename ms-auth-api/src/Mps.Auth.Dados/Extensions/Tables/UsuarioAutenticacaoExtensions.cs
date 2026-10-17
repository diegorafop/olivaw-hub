using Microsoft.EntityFrameworkCore;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

internal static class UsuarioAutenticacaoExtensions
{
    internal static void SetupUsuarioAutenticacao(this ModelBuilder modelBuilder)
    {
        modelBuilder
           .Entity<UsuarioAutenticacao>()
           .HasOne(item => item.Usuario);

        modelBuilder
            .Entity<UsuarioAutenticacao>()
            .Property(item => item.Token)
            .IsRequired()
            .HasMaxLength(100);
    }
}