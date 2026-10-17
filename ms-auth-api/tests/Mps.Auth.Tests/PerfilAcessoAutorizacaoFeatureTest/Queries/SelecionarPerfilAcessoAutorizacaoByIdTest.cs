using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoAutorizacaoFeatureTest.Queries;

public class SelecionarPerfilAcessoAutorizacaoByIdTest
{
    private readonly Mock<IRepository<PerfilAcessoAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarPerfilAcessoAutorizacaoByIdQueryValidator _validator = new();
    private readonly SelecionarPerfilAcessoAutorizacaoByIdQueryHandler _handler;

    public SelecionarPerfilAcessoAutorizacaoByIdTest()
    {
        _handler = new SelecionarPerfilAcessoAutorizacaoByIdQueryHandler
        (
            _repositoryMock.Object,
            _validator
        );
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SelecionarPerfilAcessoAutorizacaoByIdQueryHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() =>
            new SelecionarPerfilAcessoAutorizacaoByIdQueryHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarPerfilAcessoAutorizacaoByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_map_related_external_ids()
    {
        var request = new SelecionarPerfilAcessoAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        var entity = new PerfilAcessoAutorizacao
        {
            IdExterno = request.Id,
            Autorizacao = new Autorizacao { IdExterno = Guid.NewGuid() },
            PerfilAcesso = new PerfilAcesso { IdExterno = Guid.NewGuid() },
            DataCadastro = DateTime.UtcNow.AddDays(-1)
        };
        Expression<Func<PerfilAcessoAutorizacao, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, object>>[]>()
            ))
            .Callback<Expression<Func<PerfilAcessoAutorizacao, bool>>, CancellationToken, Expression<Func<PerfilAcessoAutorizacao, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(entity);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Equal(entity.Autorizacao.IdExterno, response.AutorizacaoId);
        Assert.Equal(entity.PerfilAcesso.IdExterno, response.PerfilAcessoId);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<PerfilAcessoAutorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_throw_not_found_when_repository_returns_null()
    {
        var request = new SelecionarPerfilAcessoAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, object>>[]>()
            ))
            .ReturnsAsync((PerfilAcessoAutorizacao)null!);

        await Assert.ThrowsAsync<ValidationException>
        (
            () => _handler.Handle(request, CancellationToken.None)
        );
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new SelecionarPerfilAcessoAutorizacaoByIdQuery());

        result.ShouldHaveValidationErrorFor(item => item.Id);
    }
}