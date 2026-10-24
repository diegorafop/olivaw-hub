using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Dados;
using Owh.OlivawCore.Dados.Extensions;
namespace Owh.Olivaw.Dados;
// Separate authority/store; shared identity entities are never copied into the application domain.
public sealed class PortalIdentityDbContext(DbContextOptions<PortalIdentityDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.AddOlivawIdentity();
        foreach (var entity in builder.Model.GetEntityTypes()) entity.SetTableName("portal" + entity.GetTableName());
        builder.SetupConstraints();
    }
}
