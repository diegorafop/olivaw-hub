using MediatR;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class UpsertUsuarioTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceNotificacao> _serviceMock = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly UpsertUsuarioCommandHandler _handler;

    public UpsertUsuarioTest() => _handler = new UpsertUsuarioCommandHandler(_repositoryMock.Object, _serviceMock.Object, _mediatorMock.Object);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioCommandHandler(null!, _serviceMock.Object, _mediatorMock.Object));
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioCommandHandler(_repositoryMock.Object, null!, _mediatorMock.Object));
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioCommandHandler(_repositoryMock.Object, _serviceMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<UpsertUsuarioCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_insert_new_users_and_save_without_notification_when_service_is_disabled()
    {
        var request = new UpsertUsuarioCommand
        {
            Itens = new[] { new UpsertUsuarioItemCommand { Login = "login", Nome = "Nome", Email = "email@teste.local" } }
        };
        var usuario = new Usuario { IdExterno = Guid.NewGuid(), Login = "login", Nome = "Nome", Email = "email@teste.local" };
        _repositoryMock.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(Array.Empty<Usuario>());
        _mediatorMock.Setup(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None)).ReturnsAsync(new InserirUsuarioInternalCommandResponse { Usuario = usuario });
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _serviceMock.SetupGet(item => item.Configured).Returns(false);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Single(response);
        _mediatorMock.Verify(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _serviceMock.VerifyGet(item => item.Configured, Times.Once);
    }
}