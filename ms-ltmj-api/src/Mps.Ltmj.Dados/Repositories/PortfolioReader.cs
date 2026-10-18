using Microsoft.EntityFrameworkCore;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Portfolio;

namespace Mps.Ltmj.Dados.Repositories;

public sealed class PortfolioReader(WriteDbContext context) : IPortfolioReader
{
    public async Task<IReadOnlyList<PortfolioEvent>> ListAsync(bool publicOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Set<PortfolioEvent>().AsNoTracking();
        if (publicOnly) query = query.Where(item => item.IsVisible);
        return await query.OrderByDescending(item => item.DataCadastro).ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }
}
