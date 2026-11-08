using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Tests.Mocks;
internal static class InventoryFakerBuilder
{
    internal static readonly Guid Actor = Guid.Parse("20000000-0000-0000-0000-000000000020");
    internal static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    internal static InventoryResourceData Data(string type = "compute") => new() { Name = "Recurso exemplo", Type = type };
    internal static InventoryResource Resource(int id = 1, string type = "compute")
    {
        var item = InventoryResource.Create("recurso-" + id, Data(type), Actor, "Operador", Now);
        item.Id = id;
        return item;
    }
    internal static ServiceDeployment Deployment()
    {
        var service = ManagedServiceFakerBuilder.Create(); service.Id = 10;
        var item = ServiceDeployment.Create(service, "production", "API produção", "VM", "https://example.test", "active", Actor, "Operador", Now);
        item.Id = 20;
        return item;
    }
    internal static Mock<IRepository<T>> Repository<T>(params T[] values) where T : Entity
    {
        var rows = new List<T>(values);
        var mock = new Mock<IRepository<T>>();
        mock.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<T, object>>[]>())).ReturnsAsync((Expression<Func<T, bool>> filter, CancellationToken token, Expression<Func<T, object>>[] joins) => rows.FirstOrDefault(filter.Compile())!);
        mock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Expression<Func<T, bool>> filter, CancellationToken token) => rows.Any(filter.Compile()));
        mock.Setup(item => item.GetAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<T, object>>[]>())).ReturnsAsync((Expression<Func<T, bool>> filter, CancellationToken token, Expression<Func<T, object>>[] joins) => rows.Where(filter.Compile()).ToArray());
        mock.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<int>(), It.IsAny<Expression<Func<T, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<T, object>>[]>())).ReturnsAsync((Expression<Func<T, bool>> filter, int count, Expression<Func<T, object>> order, CancellationToken token, Expression<Func<T, object>>[] joins) => rows.Where(filter.Compile()).OrderByDescending(order.Compile()).Take(count).ToArray());
        mock.Setup(item => item.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>())).Callback<T, CancellationToken>((item, token) => rows.Add(item)).Returns(Task.CompletedTask);
        mock.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mock;
    }
}
