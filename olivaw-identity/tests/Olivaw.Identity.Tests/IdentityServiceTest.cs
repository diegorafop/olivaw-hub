using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Xunit;

namespace Olivaw.Identity.Tests;

public class IdentityServiceTest
{
    [Fact]
    public async Task Find_should_propagate_cancellation_and_public_identifier()
    {
        var store = new Mock<IIdentityStore>();
        using var source = new CancellationTokenSource();
        var id = Guid.NewGuid();
        var usuario = new Usuario { ExternalId = id };
        Expression<Func<Usuario, bool>>? predicate = null;
        store.Setup(item => item.FindAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), source.Token))
            .Callback<Expression<Func<Usuario, bool>>, CancellationToken>((filter, token) => predicate = filter).ReturnsAsync(usuario);
        var service = new IdentityService(store.Object, new PasswordHasher<Usuario>());
        Assert.Same(usuario, await service.FindAsync<Usuario>(id, source.Token));
        Assert.NotNull(predicate);
        Assert.True(predicate.Compile()(usuario));
        Assert.False(predicate.Compile()(new Usuario { ExternalId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task Find_should_report_not_found_without_writes()
    {
        var store = new Mock<IIdentityStore>();
        var service = new IdentityService(store.Object, new PasswordHasher<Usuario>());
        var failure = await Assert.ThrowsAsync<IdentityFailure>(() => service.FindAsync<Usuario>(Guid.NewGuid(), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(404, failure.Status);
        store.Verify(item => item.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(1000001, 10)]
    public async Task List_should_reject_invalid_bounds_before_accessing_store(int page, int size)
    {
        var store = new Mock<IIdentityStore>(MockBehavior.Strict);
        var service = new IdentityService(store.Object, new PasswordHasher<Usuario>());
        var failure = await Assert.ThrowsAsync<IdentityFailure>(() => service.ListAsync<Usuario>(page, size, Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(400, failure.Status);
        store.VerifyNoOtherCalls();
    }
}
