using FluentValidation;
using Mps.OlivawCore.Dados.Repositories;
using Mps.OlivawCore.Domain;
using Xunit;

namespace Mps.OlivawCore.Tests.RepositoriesTest;

public class EntityValidatorTest
{
    [Fact]
    public async Task NotFoundAsync_should_accept_existing_entity()
    {
        await new EntityValidator<TestEntity>().NotFoundAsync(new TestEntity { Id = 7 }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NotFoundAsync_should_report_missing_entity_and_allow_reuse()
    {
        var validator = new EntityValidator<TestEntity>();
        var exception = await Assert.ThrowsAsync<ValidationException>(() => validator.NotFoundAsync(null!, TestContext.Current.CancellationToken));
        var error = Assert.Single(exception.Errors);
        Assert.Equal("Id", error.PropertyName);
        Assert.Equal("TestEntity não encontrado(a).", error.ErrorMessage);
        await validator.NotFoundAsync(new TestEntity { Id = 1 }, TestContext.Current.CancellationToken);
        Assert.Equal("TestEntity não encontrado(a).", validator.NotFoundMessage(new TestEntity()));
    }

    [Fact]
    public async Task NotFoundAsync_should_propagate_cancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new EntityValidator<TestEntity>().NotFoundAsync(new TestEntity(), source.Token));
    }

    public class TestEntity : Entity { }
}
