using Moq;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;
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

namespace Mps.Auth.Tests.UsuarioPerfilAcessoFeatureTest.Queries;

public class SelecionarUsuarioPerfilAcessoByFilterTest
{
    private readonly Mock<IRepository<UsuarioPerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarUsuarioPerfilAcessoByFilterQueryHandler _handler;

    public SelecionarUsuarioPerfilAcessoByFilterTest()
    {
        _handler = new SelecionarUsuarioPerfilAcessoByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarUsuarioPerfilAcessoByFilterQueryHandler(null!)
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

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioPerfilAcessoByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Handle_should_apply_optional_filters(bool filterUser, bool filterProfile)
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var request = new SelecionarUsuarioPerfilAcessoByFilterQuery();
        if (filterUser)
            request.UsuarioId = userId;
        if (filterProfile)
            request.PerfilAcessoId = profileId;

        var entity = new UsuarioPerfilAcesso
        {
            IdExterno = Guid.NewGuid(),
            Usuario = new Usuario { IdExterno = userId },
            PerfilAcesso = new PerfilAcesso { IdExterno = profileId }
        };
        Expression<Func<UsuarioPerfilAcesso, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, object>>[]>()
            ))
            .Callback<Expression<Func<UsuarioPerfilAcesso, bool>>, CancellationToken, Expression<Func<UsuarioPerfilAcesso, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<UsuarioPerfilAcesso> { entity });

        var response = (await _handler.Handle(request, CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
        Assert.Equal(userId, response[0].UsuarioId);
        Assert.Equal(profileId, response[0].PerfilAcessoId);
    }

    [Fact]
    public async Task Handle_should_map_null_profile_as_empty_id()
    {
        var entity = new UsuarioPerfilAcesso
        {
            Usuario = new Usuario { IdExterno = Guid.NewGuid() },
            PerfilAcesso = null
        };
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, object>>[]>()
            ))
            .ReturnsAsync(new List<UsuarioPerfilAcesso> { entity });

        var response = (await _handler.Handle(new SelecionarUsuarioPerfilAcessoByFilterQuery(), CancellationToken.None)).Single();

        Assert.Null(response.PerfilAcessoId);
        Assert.Equal(entity.Usuario.IdExterno, response.UsuarioId);
    }
}