using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoFeatureTest.Queries;

public class SelecionarPerfilAcessoByFilterTest
{
    private readonly Mock<IRepository<PerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarPerfilAcessoByFilterQueryHandler _handler;
    private readonly SelecionarPerfilAcessoByFilterQueryValidator _validator = new();

    public SelecionarPerfilAcessoByFilterTest()
    {
        _handler = new SelecionarPerfilAcessoByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_retain_repository()
    {
        Assert.NotNull(_handler._repository);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarPerfilAcessoByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_apply_name_filter_and_map_entities()
    {
        var request = new SelecionarPerfilAcessoByFilterQuery { Nome = "Admin" };
        var entity = new PerfilAcesso
        {
            IdExterno = Guid.NewGuid(),
            Nome = "Administrador",
            Descricao = "Descrição",
            DataCadastro = DateTime.UtcNow.AddDays(-1),
            Autorizacaos = []
        };
        Expression<Func<PerfilAcesso, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcesso, object>>[]>()
            ))
            .Callback<Expression<Func<PerfilAcesso, bool>>, CancellationToken, Expression<Func<PerfilAcesso, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<PerfilAcesso> { entity });

        var response = (await _handler.Handle(request, CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.False(capturedPredicate.Compile()(new PerfilAcesso { Nome = "Usuário" }));
        Assert.Single(response);
        Assert.Equal(entity.IdExterno, response[0].Id);
        Assert.Equal(entity.Nome, response[0].Nome);
        Assert.Equal(entity.Descricao, response[0].Descricao);
        _repositoryMock.Verify(item => item.GetAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<PerfilAcesso, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_return_all_entities_when_name_is_empty()
    {
        var entities = new List<PerfilAcesso>
        {
            new() { IdExterno = Guid.NewGuid(), Nome = "Admin", Autorizacaos = [] },
            new() { IdExterno = Guid.NewGuid(), Nome = "Usuário", Autorizacaos = [] }
        };
        Expression<Func<PerfilAcesso, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcesso, object>>[]>()
            ))
            .Callback<Expression<Func<PerfilAcesso, bool>>, CancellationToken, Expression<Func<PerfilAcesso, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(entities);

        var response = (await _handler.Handle(new SelecionarPerfilAcessoByFilterQuery(), CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entities[0]));
        Assert.Equal(2, response.Count);
    }

    [Fact]
    public void Validate_should_accept_empty_filter()
    {
        var result = _validator.TestValidate(new SelecionarPerfilAcessoByFilterQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }
}