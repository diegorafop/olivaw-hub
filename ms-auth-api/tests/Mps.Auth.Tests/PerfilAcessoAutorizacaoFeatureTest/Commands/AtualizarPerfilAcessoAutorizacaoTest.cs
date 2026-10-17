using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoAutorizacaoFeatureTest.Commands;

public class AtualizarPerfilAcessoAutorizacaoTest
{
    private readonly Mock<IRepository<PerfilAcessoAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<PerfilAcesso>> _perfilAcessoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarPerfilAcessoAutorizacaoCommandValidator _validator = new();
    private readonly AtualizarPerfilAcessoAutorizacaoCommandHandler _handler;

    public AtualizarPerfilAcessoAutorizacaoTest()
    {
        _handler = new AtualizarPerfilAcessoAutorizacaoCommandHandler(
            _repositoryMock.Object,
            _perfilAcessoRepositoryMock.Object,
            _autorizacaoRepositoryMock.Object,
            _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarPerfilAcessoAutorizacaoCommandHandler(null!, _perfilAcessoRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, null!, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _autorizacaoRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AtualizarPerfilAcessoAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_relationship_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarPerfilAcessoAutorizacaoCommand(usuarioLogadoId) { Id = Guid.NewGuid(), PerfilAcessoId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        var entity = new PerfilAcessoAutorizacao { IdExterno = request.Id, UsuarioLogadoId = Guid.NewGuid() };
        var perfilAcesso = new PerfilAcesso { Id = 31, IdExterno = request.PerfilAcessoId };
        var autorizacao = new Autorizacao { Id = 41, IdExterno = request.AutorizacaoId };

        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _perfilAcessoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(perfilAcesso);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(autorizacao);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(perfilAcesso.Id, entity.PerfilAcessoId);
        Assert.Equal(autorizacao.Id, entity.AutorizacaoId);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _perfilAcessoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_relationship()
    {
        var request = new AtualizarPerfilAcessoAutorizacaoCommand { Id = Guid.NewGuid(), PerfilAcessoId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new AtualizarPerfilAcessoAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.PerfilAcessoId);
        result.ShouldHaveValidationErrorFor(item => item.AutorizacaoId);
    }
}