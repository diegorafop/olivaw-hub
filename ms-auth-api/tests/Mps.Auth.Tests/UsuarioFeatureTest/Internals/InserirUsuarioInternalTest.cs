using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest;

public class InserirUsuarioInternalTest
{
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var handler = FeatureHandlerFactory.Create<InserirUsuarioInternalCommandHandler>();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => handler.Handle(null!, CancellationToken.None)
        );

        Assert.NotNull(exception);
    }

    [Fact]
    public void Validator_should_be_constructible()
    {
        var validator = new InserirUsuarioInternalCommandValidator();

        Assert.NotNull(validator);
    }
}

