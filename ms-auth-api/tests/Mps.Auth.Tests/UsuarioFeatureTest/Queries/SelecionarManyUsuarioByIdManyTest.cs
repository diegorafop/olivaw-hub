using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
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

namespace Mps.Auth.Tests.UsuarioFeatureTest.Queries;

public class SelecionarManyUsuarioByIdManyTest
{
    private readonly Mock<IRepository<UsuarioAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarUsuarioByIdManyQueryHandler _handler;

    public SelecionarManyUsuarioByIdManyTest()
    {
        _handler = new SelecionarUsuarioByIdManyQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioByIdManyQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_filter_users_and_map_only_matricula_authorizations()
    {
        var userId = Guid.NewGuid();
        var entity = new UsuarioAutorizacao
        {
            Usuario = new Usuario
            {
                IdExterno = userId,
                Login = "usuario",
                Nome = "Usuário",
                Email = "usuario@teste.local",
                Autorizacoes = []
            },
            Autorizacao = new Autorizacao { Tipo = "matricula", Valor = "12345" }
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
            .ReturnsAsync(new List<UsuarioAutorizacao>
            {
                entity,
                new()
                {
                    Usuario = entity.Usuario,
                    Autorizacao = new Autorizacao { Tipo = "role", Valor = "admin" }
                }
            });

        var response = (await _handler.Handle(
            new SelecionarUsuarioByIdManyQuery { IdMany = new[] { userId } },
            CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
        Assert.Equal(userId, response[0].IdExterno);
        Assert.Equal(12345, response[0].Matricula);
        Assert.Equal("usuario", response[0].Login);
        _repositoryMock.Verify(item => item.GetAsync(
            It.IsAny<Expression<Func<UsuarioAutorizacao, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<UsuarioAutorizacao, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_keep_all_users_when_id_filter_is_null()
    {
        var entity = new UsuarioAutorizacao
        {
            Usuario = new Usuario { IdExterno = Guid.NewGuid(), Autorizacoes = [] },
            Autorizacao = new Autorizacao { Tipo = "matricula", Valor = "1" }
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
            .ReturnsAsync(new[] { entity });

        var response = await _handler.Handle(new SelecionarUsuarioByIdManyQuery(), CancellationToken.None);

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
    }
}