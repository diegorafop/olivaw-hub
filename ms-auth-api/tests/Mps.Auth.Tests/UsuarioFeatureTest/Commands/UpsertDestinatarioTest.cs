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

public class UpsertDestinatarioTest
{
    private readonly Mock<IServiceNotificacao> _serviceMock = new(MockBehavior.Strict);
    private readonly UpsertDestinatarioExternalCommandHandler _handler;
    private readonly UpsertDestinatarioCommandValidator _validator = new();

    public UpsertDestinatarioTest() => _handler = new UpsertDestinatarioExternalCommandHandler(_serviceMock.Object);

    [Fact]
    public void Constructor_should_reject_null_service() => Assert.Throws<ArgumentNullException>(() => new UpsertDestinatarioExternalCommandHandler(null!));

    [Fact]
    public async Task Handle_should_reject_null_request_without_service_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<UpsertDestinatarioExternalCommand>(), exception.ParamName);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_call_notification_service_and_map_response()
    {
        var request = new UpsertDestinatarioExternalCommand { Nome = "Principal", Endereco = "principal@teste.local", UsuarioLogadoId = Guid.NewGuid() };
        var serviceResponse = new UpsertDestinatarioCommandResponse { Id = Guid.NewGuid(), DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
        _serviceMock.Setup(item => item.UpsertDestinatarioAsync(request, CancellationToken.None)).ReturnsAsync(serviceResponse);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        _serviceMock.Verify(item => item.UpsertDestinatarioAsync(request, CancellationToken.None), Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_name_and_address()
    {
        var result = _validator.TestValidate(new UpsertDestinatarioExternalCommand());
        result.ShouldHaveValidationErrorFor(item => item.Nome);
        result.ShouldHaveValidationErrorFor(item => item.Endereco);
    }
}