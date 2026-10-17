using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutorizacaoFeatureTest.Queries;

public class SelecionarAutorizacaoByFilterTest
{
    private readonly Mock<IRepository<Autorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly FakerBuilder _fakerBuilder = new();
    private readonly SelecionarAutorizacaoByFilterQueryHandler _handler;
    private readonly SelecionarAutorizacaoByFilterQueryValidator _validator = new();

    public SelecionarAutorizacaoByFilterTest()
    {
        _handler = new SelecionarAutorizacaoByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarAutorizacaoByFilterQueryHandler(null!)
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

        Assert.Equal(MessageHelper.NullFor<SelecionarAutorizacaoByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_query_and_map_all_matching_authorizations()
    {
        var request = new SelecionarAutorizacaoByFilterQuery
        {
            Tipo = "role",
            Valor = "admin"
        };
        var entity = _fakerBuilder.AutorizacaoFeature.Autorizacao.Generate();
        entity.Tipo = request.Tipo;
        entity.Valor = request.Valor;
        Expression<Func<Autorizacao, bool>>? capturedPredicate = null;
        using var cancellation = new CancellationTokenSource();

        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                cancellation.Token))
            .Callback<Expression<Func<Autorizacao, bool>>, CancellationToken, Expression<Func<Autorizacao, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<Autorizacao> { entity });

        var response = (await _handler.Handle(request, cancellation.Token)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
        Assert.Equal(entity.IdExterno, response[0].Id);
        Assert.Equal(entity.Tipo, response[0].Tipo);
        Assert.Equal(entity.Valor, response[0].Valor);
        Assert.Equal(entity.Nome, response[0].Nome);
        Assert.Equal(entity.Descricao, response[0].Descricao);
        Assert.Equal(entity.Perfil, response[0].Perfil);
        _repositoryMock.Verify(item => item.GetAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), cancellation.Token), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("role", null)]
    [InlineData(null, "admin")]
    public void Validate_should_require_at_least_one_filter(string? tipo, string? valor)
    {
        var request = new SelecionarAutorizacaoByFilterQuery
        {
            Tipo = tipo,
            Valor = valor
        };

        var result = _validator.TestValidate(request);

        if (tipo is null)
        {
            if (valor is null)
            {
                result.ShouldHaveValidationErrorFor(item => item.Tipo);
                result.ShouldHaveValidationErrorFor(item => item.Valor);
                return;
            }
        }

        result.ShouldNotHaveAnyValidationErrors();
    }
}