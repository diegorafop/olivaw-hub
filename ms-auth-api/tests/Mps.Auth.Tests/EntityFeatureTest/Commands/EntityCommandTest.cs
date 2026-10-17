using Mps.Auth.Api.Features.EntityFeature.Commands;
using System;
using Xunit;

namespace Mps.Auth.Tests.EntityFeatureTest.Commands;

public class EntityCommandTest
{
    [Fact]
    public void Constructor_should_create_command_without_user()
    {
        var command = new TestEntityCommand();

        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_should_accept_logged_user_id()
    {
        var userId = Guid.NewGuid();

        var command = new TestEntityCommand(userId);

        Assert.NotNull(command);
    }

    private sealed class TestEntityCommand : EntityCommand
    {
        public TestEntityCommand() { }

        public TestEntityCommand(Guid usuarioLogadoId)
            : base(usuarioLogadoId) { }
    }
}