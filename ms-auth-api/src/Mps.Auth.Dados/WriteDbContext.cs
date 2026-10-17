using Microsoft.EntityFrameworkCore;
using Mps.Auth.Dados.Extensions.Facades;
using Mps.Auth.Domain;
using Mps.HolosCore.Dados.Extensions;
using System.Data.Common;

namespace Mps.Auth.Dados;

public class WriteDbContext
(
    DbContextOptions<WriteDbContext> options
) : DbContext(options),
    IWriteDbContext
{
    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Autorizacao> Autorizacao { get; set; }
    public DbSet<UsuarioAutorizacao> UsuarioAutorizacao { get; set; }
    public DbSet<UsuarioAutenticacao> UsuarioAutenticacao { get; set; }
    public DbSet<PerfilAcesso> PerfilAcesso { get; set; }
    public DbSet<PerfilAcessoAutorizacao> PerfilAcessoAutorizacao { get; set; }

    protected override void OnModelCreating
    (
        ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupConstraints();
        modelBuilder.SetupTables();
    }

    protected override void OnConfiguring
    (
        DbContextOptionsBuilder optionsBuilder
    )
    {
        optionsBuilder.UseLowerCaseNamingConvention();
    }

    public override async Task<int> SaveChangesAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await SaveChangesAsync
        (
            acceptAllChangesOnSuccess: true,
            cancellationToken
        );
    }

    public async Task<bool> CanConnectAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await base
            .Database
            .CanConnectAsync(cancellationToken);
    }

    public DbConnection GetDbConnection()
    {
        return base
            .Database
            .GetDbConnection();
    }
}