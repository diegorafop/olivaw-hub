using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Owh.Ltmj.Domain;
using Owh.OlivawCore.Repositories;
using Owh.Ltmj.Tests.Mocks;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest;

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
