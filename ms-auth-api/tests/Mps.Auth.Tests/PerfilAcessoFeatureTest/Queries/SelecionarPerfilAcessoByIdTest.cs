using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoFeatureTest.Queries;

public class SelecionarPerfilAcessoByIdTest
{
    private readonly Mock<IRepository<PerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarPerfilAcessoByIdQueryHandler _handler;
    private readonly SelecionarPerfilAcessoByIdQueryValidator _validator = new();

    public SelecionarPerfilAcessoByIdTest()
    {
        _handler = new SelecionarPerfilAcessoByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarPerfilAcessoByIdQueryHandler(null!)
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

        Assert.Equal(MessageHelper.NullFor<SelecionarPerfilAcessoByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_filter_by_external_id_and_map_response()
    {
        var request = new SelecionarPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
        var entity = new PerfilAcesso
        {
            IdExterno = request.Id,
            Nome = "Administrador",
            Descricao = "Perfil administrativo",
            DataCadastro = DateTime.UtcNow.AddDays(-2),
            DataAtualizacao = DateTime.UtcNow.AddDays(-1),
            Autorizacaos = []
        };
        Expression<Func<PerfilAcesso, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcesso, object>>[]>()
            ))
            .Callback<Expression<Func<PerfilAcesso, bool>>, CancellationToken, Expression<Func<PerfilAcesso, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(entity);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Equal(entity.Nome, response.Nome);
        Assert.Equal(entity.Descricao, response.Descricao);
        Assert.Equal(entity.DataCadastro, response.DataCadastro);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<PerfilAcesso, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new SelecionarPerfilAcessoByIdQuery());

        result.ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper
                .EmptyFor<SelecionarPerfilAcessoByIdQuery, Guid>
                (item => item.Id));
    }

    [Fact]
    public void Validate_should_accept_external_id()
    {
        var result = _validator.TestValidate
        (
            new SelecionarPerfilAcessoByIdQuery
            {
                Id = Guid.NewGuid()
            }
        );

        result.ShouldNotHaveAnyValidationErrors();
    }
}