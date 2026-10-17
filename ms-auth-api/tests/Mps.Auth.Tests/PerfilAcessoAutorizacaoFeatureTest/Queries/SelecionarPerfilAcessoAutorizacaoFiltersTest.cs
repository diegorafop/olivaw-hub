using Moq;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
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

namespace Mps.Auth.Tests.PerfilAcessoAutorizacaoFeatureTest.Queries;

public class SelecionarPerfilAcessoAutorizacaoFiltersTest
{
    private readonly Mock<IRepository<PerfilAcessoAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarPerfilAcessoAutorizacaoByFilterQueryHandler _handler;

    public SelecionarPerfilAcessoAutorizacaoFiltersTest()
    {
        _handler = new SelecionarPerfilAcessoAutorizacaoByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarPerfilAcessoAutorizacaoByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Handle_should_apply_optional_filters(bool filterAuthorization, bool filterProfile)
    {
        var authorizationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var request = new SelecionarPerfilAcessoAutorizacaoByFilterQuery();
        if (filterAuthorization)
            request.AutorizacaoId = authorizationId;
        if (filterProfile)
            request.PerfilAcessoId = profileId;

        var entity = new PerfilAcessoAutorizacao
        {
            IdExterno = Guid.NewGuid(),
            Autorizacao = new Autorizacao { IdExterno = authorizationId },
            PerfilAcesso = new PerfilAcesso { IdExterno = profileId }
        };
        Expression<Func<PerfilAcessoAutorizacao, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<PerfilAcessoAutorizacao, object>>[]>()
            ))
            .Callback<Expression<Func<PerfilAcessoAutorizacao, bool>>, CancellationToken, Expression<Func<PerfilAcessoAutorizacao, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<PerfilAcessoAutorizacao> { entity });

        var response = (await _handler.Handle(request, CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
        Assert.Equal(authorizationId, response[0].AutorizacaoId);
        Assert.Equal(profileId, response[0].PerfilAcessoId);
    }

    [Fact]
    public void Validator_should_be_constructible_without_errors()
    {
        var validator = new SelecionarPerfilAcessoAutorizacaoByFilterQueryValidator();

        Assert.Empty(validator.Validate(new SelecionarPerfilAcessoAutorizacaoByFilterQuery()).Errors);
    }
}