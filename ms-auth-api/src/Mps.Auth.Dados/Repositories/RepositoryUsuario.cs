using Microsoft.EntityFrameworkCore;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Dados.Repositories;

public class RepositoryUsuario
(
    WriteDbContext context
) : Repository<Usuario>(context),
    IRepositoryUsuario
{
    public override async Task<Usuario> GetFirstAsync
    (
        Expression<Func<Usuario, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<Usuario, object>>[] joins
    )
    {
        if (!HasJoin(joins))
            return await base.GetFirstAsync
            (
                lambda,
                cancellationToken
            );

        Usuario entity = await Query()
               .Include(item => item.Autorizacoes)
                   .ThenInclude(item => item.Autorizacao)
               .Include(item => item.Perfils)
                   .ThenInclude(item => item.PerfilAcesso)
                       .ThenInclude(item => item.Autorizacaos)
                            .ThenInclude(item => item.Autorizacao)
               .FirstOrDefaultAsync(lambda, cancellationToken);

        await _validator.NotFoundAsync
        (
            entity,
            cancellationToken
        );

        return entity;
    }

    public override async Task<IEnumerable<Usuario>> GetAsync
    (
        Expression<Func<Usuario, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<Usuario, object>>[] joins
    )
    {
        if (!HasJoin(joins))
            return await base.GetAsync
            (
                lambda,
                cancellationToken
            );

        return await Query()
            .Where(lambda)
                .Include(item => item.Autorizacoes)
                    .ThenInclude(item => item.Autorizacao)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Usuario> GetFirstOrDefaultAsync
    (
        Expression<Func<Usuario, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<Usuario, object>>[] joins
    )
    {
        if (!HasJoin(joins))
            return await base.GetFirstOrDefaultAsync
            (
                lambda,
                cancellationToken
            );

        return await Query()
               .Include(item => item.Autorizacoes)
                   .ThenInclude(item => item.Autorizacao)
               .Include(item => item.Perfils)
                   .ThenInclude(item => item.PerfilAcesso)
                       .ThenInclude(item => item.Autorizacaos)
                            .ThenInclude(item => item.Autorizacao)
               .FirstOrDefaultAsync(lambda, cancellationToken);
    }
}