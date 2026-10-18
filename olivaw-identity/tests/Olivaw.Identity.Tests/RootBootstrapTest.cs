using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Xunit;

namespace Olivaw.Identity.Tests;

public class RootBootstrapTest
{
    private static IConfiguration Config(string password) => new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?> { ["Identity:Bootstrap:Enabled"] = "true", ["Identity:Bootstrap:Email"] = "root@example.test", ["Identity:Bootstrap:Password"] = password }).Build();

    [Theory]
    [InlineData("x")]
    [InlineData("short ")]
    public async Task EnsureAsync_should_accept_short_password_and_hash_without_trimming(string password)
    {
        var store = new Mock<IIdentityStore>();
        var hasher = new PasswordHasher<Usuario>();
        Usuario? created = null;
        store.Setup(item => item.Add(It.IsAny<Usuario>())).Callback<Usuario>(item => created = item);
        await new RootBootstrap(store.Object, hasher).EnsureAsync(Config(password), Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.True(created.Root);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(created, created.SenhaHash, password));
        store.Verify(item => item.SaveAsync(Xunit.TestContext.Current.CancellationToken), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(129)]
    public async Task EnsureAsync_should_reject_empty_or_oversized_password(int length)
    {
        var store = new Mock<IIdentityStore>();
        await Assert.ThrowsAsync<ValidationException>(() => new RootBootstrap(store.Object, new PasswordHasher<Usuario>())
            .EnsureAsync(Config(new string('x', length)), Xunit.TestContext.Current.CancellationToken));
        store.Verify(item => item.Add(It.IsAny<Usuario>()), Times.Never);
        store.Verify(item => item.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
