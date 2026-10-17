using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioAutorizacaoFeatureTest.Commands;

public class AtualizarUsuarioAutorizacaoTest
{
    private readonly Mock<IRepository<UsuarioAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarUsuarioAutorizacaoCommandValidator _validator = new();
    private readonly AtualizarUsuarioAutorizacaoCommandHandler _handler;

    public AtualizarUsuarioAutorizacaoTest()
    {
        _handler = new AtualizarUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, _autorizacaoRepositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioAutorizacaoCommandHandler(null!, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioAutorizacaoCommandHandler(_repositoryMock.Object, _autorizacaoRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AtualizarUsuarioAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_relationship_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarUsuarioAutorizacaoCommand(usuarioLogadoId) { Id = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        var entity = new UsuarioAutorizacao { IdExterno = request.Id, UsuarioId = 7, UsuarioLogadoId = Guid.NewGuid() };
        var autorizacao = new Autorizacao { Id = 28, IdExterno = request.AutorizacaoId };

        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(autorizacao);
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(autorizacao.Id, entity.AutorizacaoId);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_authorization()
    {
        var request = new AtualizarUsuarioAutorizacaoCommand { Id = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        var entity = new UsuarioAutorizacao { IdExterno = request.Id, UsuarioId = 7 };
        var autorizacao = new Autorizacao { Id = 28, IdExterno = request.AutorizacaoId };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(autorizacao);
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new AtualizarUsuarioAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.AutorizacaoId);
    }
}