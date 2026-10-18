using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mps.Ltmj.Domain;

namespace Mps.Ltmj.Portfolio;

public interface IPortfolioReader
{
    Task<IReadOnlyList<PortfolioEvent>> ListAsync(bool publicOnly, int page, int pageSize, CancellationToken cancellationToken);
}
