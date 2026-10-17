using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioPerfilAcessoFeatureTest.Commands;

public class AtualizarUsuarioPerfilAcessoTest
{
    private readonly Mock<IRepository<UsuarioPerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<PerfilAcesso>> _perfilAcessoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _usuarioRepositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarUsuarioPerfilAcessoCommandValidator _validator = new();
    private readonly AtualizarUsuarioPerfilAcessoCommandHandler _handler;

    public AtualizarUsuarioPerfilAcessoTest()
    {
        _handler = new AtualizarUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioPerfilAcessoCommandHandler(null!, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, null!, _usuarioRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AtualizarUsuarioPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_relationship_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarUsuarioPerfilAcessoCommand(usuarioLogadoId) { Id = Guid.NewGuid(), PerfilAcessoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid() };
        var entity = new UsuarioPerfilAcesso { IdExterno = request.Id, UsuarioLogadoId = Guid.NewGuid() };
        var perfilAcesso = new PerfilAcesso { Id = 32, IdExterno = request.PerfilAcessoId };
        var usuario = new Usuario { Id = 42, IdExterno = request.UsuarioId };

        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _perfilAcessoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(perfilAcesso);
        _usuarioRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(usuario);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(perfilAcesso.Id, entity.PerfilAcessoId);
        Assert.Equal(usuario.Id, entity.UsuarioId);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _perfilAcessoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _usuarioRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_relationship()
    {
        var request = new AtualizarUsuarioPerfilAcessoCommand { Id = Guid.NewGuid(), PerfilAcessoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid() };
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new AtualizarUsuarioPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.PerfilAcessoId);
        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
    }
}