using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Queries;

public class SelecionarUsuarioByIdTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceUsuario> _serviceMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;
    private readonly SelecionarUsuarioByIdQueryValidator _validator = new();

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new SelecionarUsuarioByIdQueryHandler
        (
            null!, _serviceMock.Object, _configuration
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarUsuarioByIdQueryHandler
        (
            _repositoryMock.Object, null!, _configuration
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarUsuarioByIdQueryHandler
        (
            _repositoryMock.Object, _serviceMock.Object, null!
        ));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_calling_dependencies()
    {
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_use_empty_cargo_when_service_is_not_configured()
    {
        var request = new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() };
        var entity = CreateUsuario(request.Id);
        SetupRepository(entity);
        _serviceMock.SetupGet(item => item.Configured).Returns(false);
        var handler = CreateHandler();

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(entity.IdExterno, response.Id);
        Assert.Equal(entity.Login, response.Login);
        Assert.Equal(entity.IntegracaoId, response.Matricula);
        Assert.Equal(Guid.Empty, response.CargoId);
        Assert.Equal("Sem cargo", response.CargoNome);
        _serviceMock.VerifyGet(item => item.Configured, Times.Once);
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_map_cargo_when_service_is_configured()
    {
        var request = new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() };
        var entity = CreateUsuario(request.Id);
        var cargo = new SelecionarUsuarioCargoByUsuarioIdQueryResponse
        {
            UsuarioId = request.Id,
            CargoId = Guid.NewGuid(),
            CargoNome = "Analista"
        };
        SetupRepository(entity);
        _serviceMock.SetupGet(item => item.Configured).Returns(true);
        _serviceMock
            .Setup(item => item.SelecionarUsuarioCargoByUsuarioIdAsync(request.Id, CancellationToken.None))
            .ReturnsAsync(cargo);
        var handler = CreateHandler();

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(cargo.CargoId, response.CargoId);
        Assert.Equal(cargo.CargoNome, response.CargoNome);
        _serviceMock.Verify(item => item.SelecionarUsuarioCargoByUsuarioIdAsync(request.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fallback_to_empty_cargo_when_service_fails()
    {
        var request = new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() };
        SetupRepository(CreateUsuario(request.Id));
        _serviceMock.SetupGet(item => item.Configured).Returns(true);
        _serviceMock
            .Setup(item => item.SelecionarUsuarioCargoByUsuarioIdAsync(request.Id, CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("service unavailable"));
        var handler = CreateHandler();

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(Guid.Empty, response.CargoId);
        Assert.Equal("Sem cargo", response.CargoNome);
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new SelecionarUsuarioByIdQuery());

        result.ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioByIdQuery, Guid>
                (item => item.Id));
    }

    [Fact]
    public void Validate_should_accept_external_id()
    {
        var result = _validator.TestValidate(new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() });

        result.ShouldNotHaveAnyValidationErrors();
    }

    private SelecionarUsuarioByIdQueryHandler CreateHandler
    (
        IRepositoryUsuario? repository = null,
        IServiceUsuario? service = null,
        IConfiguration? configuration = null
    )
    {
        return new SelecionarUsuarioByIdQueryHandler
        (
            repository ?? _repositoryMock.Object,
            service ?? _serviceMock.Object,
            configuration ?? _configuration
        );
    }

    private void SetupRepository(Usuario entity)
    {
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<Usuario, object>>[]>()
            ))
            .ReturnsAsync(entity);
    }

    private static Usuario CreateUsuario(Guid id)
    {
        return new Usuario
        {
            IdExterno = id,
            Login = "usuario.teste",
            Nome = "Usuário Teste",
            Email = "usuario@teste.local",
            IntegracaoId = "12345",
            Autorizacoes = []
        };
    }
}