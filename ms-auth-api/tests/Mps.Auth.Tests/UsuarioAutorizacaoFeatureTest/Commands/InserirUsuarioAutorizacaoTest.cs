using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioAutorizacaoFeatureTest.Commands;

public class InserirUsuarioAutorizacaoTest
{
    private readonly Mock<IRepository<UsuarioAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _usuarioRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly InserirUsuarioAutorizacaoCommandValidator _validator = new();
    private readonly InserirUsuarioAutorizacaoCommandHandler _handler;

    public InserirUsuarioAutorizacaoTest()
    {
        _handler = new InserirUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, _usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioAutorizacaoCommandHandler(null!, _usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, null!, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, _usuarioRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, _usuarioRepositoryMock.Object, _autorizacaoRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<InserirUsuarioAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_persist_new_relationship()
    {
        var request = new InserirUsuarioAutorizacaoCommand { UsuarioId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        var usuario = new Usuario { Id = 16, IdExterno = request.UsuarioId };
        var autorizacao = new Autorizacao { Id = 26, IdExterno = request.AutorizacaoId };
        UsuarioAutorizacao? capturedEntity = null;

        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _usuarioRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(usuario);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(autorizacao);
        _repositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutorizacao>(), CancellationToken.None)).Callback<UsuarioAutorizacao, CancellationToken>((entity, _) => capturedEntity = entity).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedEntity);
        Assert.Equal(usuario.Id, capturedEntity!.UsuarioId);
        Assert.Equal(autorizacao.Id, capturedEntity.AutorizacaoId);
        Assert.Equal(capturedEntity.IdExterno, response.Id);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _usuarioRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutorizacao>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_relationship()
    {
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(true);
        var request = new InserirUsuarioAutorizacaoCommand { UsuarioId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _usuarioRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new InserirUsuarioAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
        result.ShouldHaveValidationErrorFor(item => item.AutorizacaoId);
    }
}