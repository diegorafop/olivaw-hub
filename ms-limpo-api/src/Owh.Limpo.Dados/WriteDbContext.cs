using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Dados;
using Microsoft.Extensions.Configuration;
using Owh.OlivawCore.Dados.Extensions;
using Owh.Limpo.Dados.Extensions.Facades;
using System.Data.Common;

namespace Owh.Limpo.Dados;

public class WriteDbContext
(
    DbContextOptions<WriteDbContext> options,
    IConfiguration configuration
) : DbContext(options),
    IWriteDbContext
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    protected override void OnModelCreating
    (
        ModelBuilder modelBuilder
    )
    {
        modelBuilder.AddOlivawIdentity();
        modelBuilder.SetupTables();
        modelBuilder.SetupConstraints();
    }

    protected override void OnConfiguring
    (
        DbContextOptionsBuilder optionsBuilder
    )
    {
        optionsBuilder.OnConfiguring
        (
            _configuration["DatabaseProvider"] ?? string.Empty
        );
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
