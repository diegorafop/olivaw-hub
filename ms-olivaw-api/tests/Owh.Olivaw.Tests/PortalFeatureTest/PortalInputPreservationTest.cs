using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.PortalFeatureTest;
public sealed class PortalInputPreservationTest
{
    [Fact]
    public async Task Pipeline_should_validate_without_trimming_password_bytes()
    {
        const string password = "  password with spaces  ";
        var request = new CreatePortalSessionMediatorCommand { Payload = new() { Email = "user@example.test", Password = password } };
        var behavior = new TemplateValidationBehavior<CreatePortalSessionMediatorCommand, CreatePortalSessionResponse>(new[] { new CreatePortalSessionMediatorValidator() });
        await behavior.Handle(request, token => { Assert.Equal(password, request.Payload.Password); return Task.FromResult(new CreatePortalSessionResponse()); }, CancellationToken.None);
        request.Payload.Email = "invalid";
        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(request, token => Task.FromResult(new CreatePortalSessionResponse()), CancellationToken.None));
    }
}
