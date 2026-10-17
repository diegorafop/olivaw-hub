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

public class InserirPerfilAcessoAutorizacaoTest
{
    private readonly Mock<IRepository<PerfilAcessoAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<PerfilAcesso>> _perfilAcessoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly InserirPerfilAcessoAutorizacaoCommandValidator _validator = new();
    private readonly InserirPerfilAcessoAutorizacaoCommandHandler _handler;

    public InserirPerfilAcessoAutorizacaoTest()
    {
        _handler = new InserirPerfilAcessoAutorizacaoCommandHandler(
            _repositoryMock.Object,
            _perfilAcessoRepositoryMock.Object,
            _autorizacaoRepositoryMock.Object,
            _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new InserirPerfilAcessoAutorizacaoCommandHandler(null!, _perfilAcessoRepositoryMock.Object, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, null!, _autorizacaoRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, _perfilAcessoRepositoryMock.Object, _autorizacaoRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<InserirPerfilAcessoAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_persist_new_relationship()
    {
        var request = new InserirPerfilAcessoAutorizacaoCommand { PerfilAcessoId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };
        var perfilAcesso = new PerfilAcesso { Id = 12, IdExterno = request.PerfilAcessoId };
        var autorizacao = new Autorizacao { Id = 21, IdExterno = request.AutorizacaoId };
        PerfilAcessoAutorizacao? capturedEntity = null;

        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _perfilAcessoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(perfilAcesso);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(autorizacao);
        _repositoryMock.Setup(item => item.AddAsync(It.IsAny<PerfilAcessoAutorizacao>(), CancellationToken.None)).Callback<PerfilAcessoAutorizacao, CancellationToken>((entity, _) => capturedEntity = entity).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedEntity);
        Assert.Equal(perfilAcesso.Id, capturedEntity!.PerfilAcessoId);
        Assert.Equal(autorizacao.Id, capturedEntity.AutorizacaoId);
        Assert.Equal(capturedEntity.IdExterno, response.Id);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _perfilAcessoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _autorizacaoRepositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<PerfilAcessoAutorizacao>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_relationship()
    {
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(true);
        var request = new InserirPerfilAcessoAutorizacaoCommand { PerfilAcessoId = Guid.NewGuid(), AutorizacaoId = Guid.NewGuid() };

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _perfilAcessoRepositoryMock.VerifyNoOtherCalls();
        _autorizacaoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_ids()
    {
        var result = _validator.TestValidate(new InserirPerfilAcessoAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.PerfilAcessoId);
        result.ShouldHaveValidationErrorFor(item => item.AutorizacaoId);
    }
}