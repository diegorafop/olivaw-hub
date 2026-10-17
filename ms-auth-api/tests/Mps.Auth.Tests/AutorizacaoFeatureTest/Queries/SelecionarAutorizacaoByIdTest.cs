using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutorizacaoFeatureTest.Queries;

public class SelecionarAutorizacaoByIdTest
{
    private readonly Mock<IRepository<Autorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly FakerBuilder _fakerBuilder = new();
    private readonly SelecionarAutorizacaoByIdQueryValidator _validator = new();
    private readonly SelecionarAutorizacaoByIdQueryHandler _handler;

    public SelecionarAutorizacaoByIdTest()
    {
        _handler = new SelecionarAutorizacaoByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarAutorizacaoByIdQueryHandler(null!)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarAutorizacaoByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_filter_by_external_id_and_map_response(bool optionalFieldsAreNull)
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        var entity = _fakerBuilder.AutorizacaoFeature.Autorizacao.Generate();
        entity.IdExterno = request.Id;
        if (optionalFieldsAreNull)
        {
            entity.Nome = null;
            entity.Descricao = null;
            entity.DataAtualizacao = null;
            entity.DataRemocao = null;
            entity.Perfil = false;
        }

        using var cancellation = new CancellationTokenSource();
        Expression<Func<Autorizacao, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                cancellation.Token,
                It.Is<Expression<Func<Autorizacao, object>>[]>(joins => joins.Length == 0)))
            .Callback<Expression<Func<Autorizacao, bool>>, CancellationToken, Expression<Func<Autorizacao, object>>[]>
            ((predicate, token, joins) => capturedPredicate = predicate)
            .ReturnsAsync(entity);

        var response = await _handler.Handle(request, cancellation.Token);

        Assert.NotNull(capturedPredicate);
        var matches = capturedPredicate.Compile();
        Assert.True(matches(entity));
        var otherEntity = _fakerBuilder.AutorizacaoFeature.Autorizacao.Generate();
        otherEntity.Id = entity.Id;
        otherEntity.IdExterno = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        Assert.False(matches(otherEntity));
        Assert.Equal(entity.Tipo, response.Tipo);
        Assert.Equal(entity.Valor, response.Valor);
        Assert.Equal(entity.Nome, response.Nome);
        Assert.Equal(entity.Descricao, response.Descricao);
        Assert.Equal(entity.Perfil, response.Perfil);
        Assert.Equal(entity.DataCadastro, response.DataCadastro);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        Assert.Equal(entity.DataRemocao, response.DataRemocao);
        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), cancellation.Token,
            It.IsAny<Expression<Func<Autorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_propagate_repository_validation_failure()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        var expected = new ValidationException("Autorização não encontrada.");
        _repositoryMock.Setup(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None,
            It.IsAny<Expression<Func<Autorizacao, object>>[]>())).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<ValidationException>
        (
            () => _handler.Handle(request, CancellationToken.None)
        );

        Assert.Same(expected, exception);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None,
            It.IsAny<Expression<Func<Autorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_propagate_repository_cancellation()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _repositoryMock.Setup(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), cancellation.Token,
            It.IsAny<Expression<Func<Autorizacao, object>>[]>()))
            .Returns(Task.FromCanceled<Autorizacao>(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => _handler.Handle(request, cancellation.Token)
        );

        Assert.Equal(cancellation.Token, exception.CancellationToken);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), cancellation.Token,
            It.IsAny<Expression<Func<Autorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_accept_valid_id()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_reject_empty_id_with_expected_message()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        request.Id = Guid.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper
                .EmptyFor<SelecionarAutorizacaoByIdQuery, Guid>
                (item => item.Id))
            .Only();
    }
}