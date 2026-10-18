using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Portfolio;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest;

internal static class PortfolioTestData
{
    internal static Mock<IRepository<PortfolioEvent>> Repository(PortfolioEvent item)
    {
        var repository = new Mock<IRepository<PortfolioEvent>>();
        repository.Setup(service => service.GetFirstAsync(It.IsAny<Expression<Func<PortfolioEvent, bool>>>(),
            It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<PortfolioEvent, object>>[]>())).ReturnsAsync(item);
        return repository;
    }
}
