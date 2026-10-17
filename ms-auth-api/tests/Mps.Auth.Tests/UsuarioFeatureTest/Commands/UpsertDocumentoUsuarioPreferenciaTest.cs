using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class UpsertDocumentoUsuarioPreferenciaTest
{
    private readonly Mock<IServiceUsuario> _serviceMock = new(MockBehavior.Strict);
    private readonly UpsertDocumentoUsuarioPreferenciaExternalCommandHandler _handler;
    private readonly UpsertDocumentoUsuarioPreferenciaCommandValidator _validator = new();

    public UpsertDocumentoUsuarioPreferenciaTest() => _handler = new UpsertDocumentoUsuarioPreferenciaExternalCommandHandler(_serviceMock.Object);

    [Fact]
    public void Constructor_should_reject_null_service() => Assert.Throws<ArgumentNullException>(() => new UpsertDocumentoUsuarioPreferenciaExternalCommandHandler(null!));

    [Fact]
    public async Task Handle_should_reject_null_request_without_service_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<UpsertDocumentoUsuarioPreferenciaExternalCommand>(), exception.ParamName);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_call_user_service_and_map_response()
    {
        var request = new UpsertDocumentoUsuarioPreferenciaExternalCommand { UsuarioId = Guid.NewGuid(), FavoritarPrazo = true, SeguirAutomaticoGestor = true };
        var serviceResponse = new UpsertDocumentoUsuarioPreferenciaCommandResponse { Id = Guid.NewGuid(), UsuarioId = request.UsuarioId, FavoritarPrazo = true, SeguirAutomaticoGestor = true, DataCadastro = DateTime.UtcNow };
        _serviceMock.Setup(item => item.UpsertDocumentoUsuarioPreferenciaAsync(request, CancellationToken.None)).ReturnsAsync(serviceResponse);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        _serviceMock.Verify(item => item.UpsertDocumentoUsuarioPreferenciaAsync(request, CancellationToken.None), Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_user_id()
    {
        var result = _validator.TestValidate(new UpsertDocumentoUsuarioPreferenciaExternalCommand());
        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
    }
}