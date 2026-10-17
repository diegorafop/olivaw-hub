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

public class InserirUsuarioPerfilAcessoTest
{
    private readonly Mock<IRepository<UsuarioPerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<PerfilAcesso>> _perfilAcessoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _usuarioRepositoryMock = new(MockBehavior.Strict);
    private readonly InserirUsuarioPerfilAcessoCommandValidator _validator = new();
    private readonly InserirUsuarioPerfilAcessoCommandHandler _handler;

    public InserirUsuarioPerfilAcessoTest()
    {
        _handler = new InserirUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioPerfilAcessoCommandHandler(null!, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, null!, _usuarioRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _usuarioRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<InserirUsuarioPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_persist_new_relationship()
    {
        var request = new InserirUsuarioPerfilAcessoCommand { PerfilAcessoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid() };
        var perfilAcesso = new PerfilAcesso { Id = 14, IdExterno = request.PerfilAcessoId };
        var usuario = new Usuario { Id = 25, IdExterno = request.UsuarioId };
        UsuarioPerfilAcesso? capturedEntity = null;

        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _perfilAcessoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(perfilAcesso);
        _usuarioRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(usuario);
        _repositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioPerfilAcesso>(), CancellationToken.None)).Callback<UsuarioPerfilAcesso, CancellationToken>((entity, _) => capturedEntity = entity).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedEntity);
        Assert.Equal(perfilAcesso.Id, capturedEntity!.PerfilAcessoId);
        Assert.Equal(usuario.Id, capturedEntity.UsuarioId);
        Assert.Equal(capturedEntity.IdExterno, response.Id);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _perfilAcessoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _usuarioRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioPerfilAcesso>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_relationship()
    {
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(true);
        var request = new InserirUsuarioPerfilAcessoCommand { PerfilAcessoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid() };

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new InserirUsuarioPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.PerfilAcessoId);
        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
    }
}