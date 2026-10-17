using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.Auth.Services;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Queries;

public class SelecionarUsuarioByFilterTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceUsuario> _serviceMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;
    private readonly SelecionarUsuarioByFilterQueryHandler _handler;

    public SelecionarUsuarioByFilterTest()
    {
        _handler = new SelecionarUsuarioByFilterQueryHandler
        (
            _repositoryMock.Object,
            _serviceMock.Object,
            _configuration
        );
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_calling_dependencies()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioByFilterQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_apply_all_filters_when_service_is_not_configured()
    {
        var userId = Guid.NewGuid();
        var request = new SelecionarUsuarioByFilterQuery
        {
            Login = "usuario",
            Ativo = true,
            Ids = new[] { userId },
            Matriculas = new[] { 12345 },
            StatusIntegracaoEnum = StatusIntegracaoEnum.NaoIntegrar
        };
        var entity = new Usuario
        {
            IdExterno = userId,
            Login = "usuario.teste",
            Nome = "Usuário",
            Email = "usuario@teste.local",
            IntegracaoId = "12345",
            StatusIntegracaoEnum = StatusIntegracaoEnum.NaoIntegrar,
            DataRemocao = null,
            Autorizacoes = []
        };
        Expression<Func<Usuario, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<Usuario, object>>[]>()
            ))
            .Callback<Expression<Func<Usuario, bool>>, CancellationToken, Expression<Func<Usuario, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<Usuario> { entity });
        _serviceMock.SetupGet(item => item.Configured).Returns(false);

        var response = (await _handler.Handle(request, CancellationToken.None)).ToList();

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.Single(response);
        Assert.Equal(userId, response[0].Id);
        Assert.Equal(entity.Login, response[0].Login);
        Assert.Equal(12345, response[0].Matricula);
        Assert.Equal(Guid.Empty, response[0].CargoId);
        Assert.Equal("Sem cargo", response[0].CargoNome);
        _serviceMock.VerifyGet(item => item.Configured, Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_use_external_cargo_service_when_configured()
    {
        var userId = Guid.NewGuid();
        var entity = new Usuario
        {
            IdExterno = userId,
            Login = "usuario",
            Nome = "Usuário",
            Email = "usuario@teste.local",
            Autorizacoes = []
        };
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<Usuario, object>>[]>()
            ))
            .ReturnsAsync(new List<Usuario> { entity });
        _serviceMock.SetupGet(item => item.Configured).Returns(true);
        _serviceMock
            .Setup(item => item.SelecionarUsuarioCargoByFilterAsync(
                It.IsAny<Mps.Auth.Requests.SelecionarUsuarioCargoByFilterQuery>(),
                CancellationToken.None))
            .ReturnsAsync(new List<Mps.Auth.Requests.SelecionarUsuarioCargoByFilterQueryResponse>
            {
                new() { UsuarioId = userId, CargoId = Guid.NewGuid(), CargoNome = "Analista" }
            });

        var response = (await _handler.Handle(new SelecionarUsuarioByFilterQuery(), CancellationToken.None)).Single();

        Assert.Equal("Analista", response.CargoNome);
        Assert.NotEqual(Guid.Empty, response.CargoId);
    }
}