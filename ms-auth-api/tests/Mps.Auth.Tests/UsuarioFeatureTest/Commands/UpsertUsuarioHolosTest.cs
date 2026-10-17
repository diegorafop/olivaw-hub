using MediatR;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class UpsertUsuarioHolosTest
{
    private readonly Mock<IRepositoryUsuario> _usuarioRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutorizacao>> _usuarioAutorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly UpsertUsuarioHolosCommandHandler _handler;

    public UpsertUsuarioHolosTest() => _handler = new UpsertUsuarioHolosCommandHandler(_usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _usuarioAutorizacaoRepositoryMock.Object, _mediatorMock.Object);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioHolosCommandHandler(null!, _autorizacaoRepositoryMock.Object, _usuarioAutorizacaoRepositoryMock.Object, _mediatorMock.Object));
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioHolosCommandHandler(_usuarioRepositoryMock.Object, null!, _usuarioAutorizacaoRepositoryMock.Object, _mediatorMock.Object));
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioHolosCommandHandler(_usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, null!, _mediatorMock.Object));
        Assert.Throws<ArgumentNullException>(() => new UpsertUsuarioHolosCommandHandler(_usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _usuarioAutorizacaoRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<UpsertUsuarioHolosCommand>(), exception.ParamName);
        _usuarioRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
        _usuarioAutorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_insert_user_and_requested_authorizations()
    {
        var request = new UpsertUsuarioHolosCommand { Login = "login", Nome = "Nome", Email = "email@teste.local", Autorizacoes = new[] { "role" } };
        var usuario = new Usuario { Id = 5, IdExterno = Guid.NewGuid(), Login = request.Login, Nome = request.Nome, Email = request.Email };
        var autorizacao = new Autorizacao { Id = 8, IdExterno = Guid.NewGuid(), Nome = "role", Valor = "role" };
        _usuarioRepositoryMock.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>())).ReturnsAsync((Usuario)null!);
        _mediatorMock.Setup(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None)).ReturnsAsync(new InserirUsuarioInternalCommandResponse { Usuario = usuario });
        _usuarioRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _autorizacaoRepositoryMock.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(new[] { autorizacao });
        _usuarioAutorizacaoRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutorizacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _usuarioAutorizacaoRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _autorizacaoRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(usuario.IdExterno, response.Id);
        _mediatorMock.Verify(item => item.Send(It.IsAny<InserirUsuarioInternalCommand>(), CancellationToken.None), Times.Once);
        _usuarioAutorizacaoRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutorizacao>(), CancellationToken.None), Times.Once);
        _usuarioAutorizacaoRepositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validate_should_require_login_name_and_email()
    {
        var validator = new UpsertUsuarioHolosCommandValidator();
        Assert.NotEmpty(validator.Validate(new UpsertUsuarioHolosCommand()).Errors);
    }
}