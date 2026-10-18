using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Owh.Ltmj.Dados;

public interface IWriteDbContext
{
    Task<bool> CanConnectAsync
    (
        CancellationToken cancellationToken
    );

    Task<int> SaveChangesAsync
    (
        CancellationToken cancellationToken = default
    );

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    DbConnection GetDbConnection();
}