using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;
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

namespace Mps.Auth.Tests.UsuarioAutorizacaoFeatureTest.Queries;

public class SelecionarUsuarioAutorizacaoByFilterTest
{
    private readonly Mock<IRepository<UsuarioAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarUsuarioAutorizacaoByFilterQueryHandler _handler;
    private readonly SelecionarUsuarioAutorizacaoByFilterQueryValidator _validator = new();

    public SelecionarUsuarioAutorizacaoByFilterTest()
    {
        _handler = new SelecionarUsuarioAutorizacaoByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioAutorizacaoByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Handle_should_build_predicate_for_each_filter(bool filterUser, bool filterAuthorization)
    {
        var usuarioId = Guid.NewGuid();
        var autorizacaoId = Guid.NewGuid();
        var request = new SelecionarUsuarioAutorizacaoByFilterQuery();
        if (filterUser)
            request.UsuarioId = usuarioId;
        if (filterAuthorization)
            request.AutorizacaoId = autorizacaoId;
        var entity = new UsuarioAutorizacao
        {
            IdExterno = Guid.NewGuid(),
            Usuario = new Usuario { IdExterno = usuarioId },
            Autorizacao = new Autorizacao { IdExterno = autorizacaoId },
            DataCadastro = DateTime.UtcNow.AddDays(-1)
        };
        Expression<Func<UsuarioAutorizacao, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioAutorizacao, object>>[]>()
            ))
            .Callback<Expression<Func<UsuarioAutorizacao, bool>>, CancellationToken, Expression<Func<UsuarioAutorizacao, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<UsuarioAutorizacao> { entity });

        var response = (await _handler.Handle(request, CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        var matches = capturedPredicate!.Compile()(entity);
        Assert.True(matches);
        Assert.Single(response);
        Assert.Equal(usuarioId, response[0].UsuarioId);
        Assert.Equal(autorizacaoId, response[0].AutorizacaoId);
    }

    [Fact]
    public void Validate_should_require_user_or_authorization_id()
    {
        var result = _validator.TestValidate(new SelecionarUsuarioAutorizacaoByFilterQuery());

        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
        result.ShouldHaveValidationErrorFor(item => item.AutorizacaoId);
    }

    [Fact]
    public void Validate_should_accept_either_filter()
    {
        var result = _validator.TestValidate
        (
            new SelecionarUsuarioAutorizacaoByFilterQuery { UsuarioId = Guid.NewGuid() }
        );

        result.ShouldNotHaveAnyValidationErrors();
    }
}