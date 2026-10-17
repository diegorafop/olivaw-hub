using Microsoft.EntityFrameworkCore;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Dados.Repositories;

public class RepositoryPerfilAcesso
(
    WriteDbContext context
) : Repository<PerfilAcesso>(context),
    IRepositoryPerfilAcesso
{
    public override async Task<PerfilAcesso> GetFirstAsync
    (
        Expression<Func<PerfilAcesso, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<PerfilAcesso, object>>[] joins
    )
    {
        if (joins == null || !(joins.Length > 0))
            return await base.GetFirstAsync(lambda, cancellationToken, joins);

        PerfilAcesso entity = await Query()
               .Include(item => item.Autorizacaos)
                   .ThenInclude(item => item.Autorizacao)
               .FirstOrDefaultAsync(lambda, cancellationToken);

        await _validator.NotFoundAsync(entity, cancellationToken);

        return entity;
    }
}