using FluentValidation.TestHelper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Contracts;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class InserirUsuarioTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<IPublishEndpoint> _publisherMock = new(MockBehavior.Loose);
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;
    private readonly InserirUsuarioCommandHandler _handler;
    private readonly InserirUsuarioCommandValidator _validator = new();

    public InserirUsuarioTest() => _handler = new InserirUsuarioCommandHandler(_repositoryMock.Object, _mediatorMock.Object, _publisherMock.Object, _configuration);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioCommandHandler(null!, _mediatorMock.Object, _publisherMock.Object, _configuration));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioCommandHandler(_repositoryMock.Object, null!, _publisherMock.Object, _configuration));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioCommandHandler(_repositoryMock.Object, _mediatorMock.Object, null!, _configuration));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioCommandHandler(_repositoryMock.Object, _mediatorMock.Object, _publisherMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<InserirUsuarioCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_send_internal_command_save_and_publish()
    {
        var request = new InserirUsuarioCommand { Login = "novo.login", Senha = "senha", Nome = "Novo", Email = "novo@teste.local" };
        var usuario = new Usuario { IdExterno = Guid.NewGuid(), Login = request.Login, Nome = request.Nome, Email = request.Email };
        _mediatorMock.Setup(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None)).ReturnsAsync(new InserirUsuarioInternalCommandResponse { Usuario = usuario });
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(usuario.IdExterno, response.Id);
        _mediatorMock.Verify(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _publisherMock.Verify(item => item.Publish(It.IsAny<EnfileirarUpsertDocumentoUsuarioPreferenciaContract>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_login_password_name_and_email()
    {
        var result = _validator.TestValidate(new InserirUsuarioCommand());
        result.ShouldHaveValidationErrorFor(item => item.Login);
        result.ShouldHaveValidationErrorFor(item => item.Senha);
        result.ShouldHaveValidationErrorFor(item => item.Nome);
        result.ShouldHaveValidationErrorFor(item => item.Email);
    }

    [Fact]
    public async Task Validator_should_reject_duplicate_login_and_name()
    {
        var request = new InserirUsuarioCommand
        {
            Login = "duplicado",
            Senha = "senha",
            Nome = "Duplicado",
            Email = "email@teste.local"
        };

        await Assert.ThrowsAsync<FluentValidation.ValidationException>
        (
            () => _validator.ValidateDataAsync
            (
                request,
                true,
                CancellationToken.None
            )
        );
    }
}