using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioAutorizacaoFeatureTest.Queries;

public class SelecionarUsuarioAutorizacaoByIdTest
{
    private readonly Mock<IRepository<UsuarioAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarUsuarioAutorizacaoByIdQueryHandler _handler;
    private readonly SelecionarUsuarioAutorizacaoByIdQueryValidator _validator = new();

    public SelecionarUsuarioAutorizacaoByIdTest()
    {
        _handler = new SelecionarUsuarioAutorizacaoByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarUsuarioAutorizacaoByIdQueryHandler(null!)
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

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioAutorizacaoByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_filter_by_external_id_and_map_related_ids()
    {
        var request = new SelecionarUsuarioAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        var usuario = new Usuario { IdExterno = Guid.NewGuid() };
        var autorizacao = new Autorizacao { IdExterno = Guid.NewGuid(), Tipo = "role", Valor = "admin" };
        var entity = new UsuarioAutorizacao
        {
            IdExterno = request.Id,
            Usuario = usuario,
            Autorizacao = autorizacao,
            DataCadastro = DateTime.UtcNow.AddDays(-1)
        };
        Expression<Func<UsuarioAutorizacao, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioAutorizacao, object>>[]>()
            ))
            .Callback<Expression<Func<UsuarioAutorizacao, bool>>, CancellationToken, Expression<Func<UsuarioAutorizacao, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(entity);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Equal(usuario.IdExterno, response.UsuarioId);
        Assert.Equal(autorizacao.IdExterno, response.AutorizacaoId);
        Assert.Equal(entity.DataCadastro, response.DataCadastro);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<UsuarioAutorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new SelecionarUsuarioAutorizacaoByIdQuery());

        result.ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioAutorizacaoByIdQuery, Guid>
                (item => item.Id));
    }

    [Fact]
    public void Validate_should_accept_external_id()
    {
        var result = _validator.TestValidate
        (
            new SelecionarUsuarioAutorizacaoByIdQuery { Id = Guid.NewGuid() }
        );

        result.ShouldNotHaveAnyValidationErrors();
    }
}