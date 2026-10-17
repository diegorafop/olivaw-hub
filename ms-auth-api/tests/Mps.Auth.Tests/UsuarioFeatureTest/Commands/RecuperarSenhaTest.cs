using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class RecuperarSenhaTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceNotificacao> _serviceMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["services:holos-ng:baseAddress"] = "https://teste.local" }).Build();
    private readonly RecuperarSenhaCommandValidator _validator = new();
    private readonly RecuperarSenhaCommandHandler _handler;

    public RecuperarSenhaTest() => _handler = new RecuperarSenhaCommandHandler(_repositoryMock.Object, _serviceMock.Object, _configuration);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new RecuperarSenhaCommandHandler(null!, _serviceMock.Object, _configuration));
        Assert.Throws<ArgumentNullException>(() => new RecuperarSenhaCommandHandler(_repositoryMock.Object, null!, _configuration));
        Assert.Throws<ArgumentNullException>(() => new RecuperarSenhaCommandHandler(_repositoryMock.Object, _serviceMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<RecuperarSenhaCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_return_message_without_enqueue_when_service_is_not_configured()
    {
        var request = new RecuperarSenhaCommand { Email = "usuario@teste.local" };
        var entity = new Usuario { IdExterno = Guid.NewGuid(), Email = request.Email };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _serviceMock.SetupGet(item => item.Configured).Returns(false);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal("Verifique seu email para redefinir a senha.", response.MensagemValidacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _serviceMock.VerifyGet(item => item.Configured, Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_enqueue_email_when_service_is_configured()
    {
        var request = new RecuperarSenhaCommand { Email = "usuario@teste.local" };
        var entity = new Usuario { IdExterno = Guid.NewGuid(), Email = request.Email };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _serviceMock.SetupGet(item => item.Configured).Returns(true);
        _serviceMock.Setup(item => item.EnfileirarEnviarEmailByEnderecoAsync(It.IsAny<EnfileirarEnviarEmailByEnderecoCommand>(), CancellationToken.None)).ReturnsAsync(new EnfileirarEnviarEmailByEnderecoCommandResponse());

        await _handler.Handle(request, CancellationToken.None);

        _serviceMock.Verify(item => item.EnfileirarEnviarEmailByEnderecoAsync(It.IsAny<EnfileirarEnviarEmailByEnderecoCommand>(), CancellationToken.None), Times.Once);
        _serviceMock.VerifyGet(item => item.Configured, Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_and_invalid_email()
    {
        var result = _validator.TestValidate(new RecuperarSenhaCommand { Email = "invalido" });
        result.ShouldHaveValidationErrorFor(item => item.Email);
    }
}