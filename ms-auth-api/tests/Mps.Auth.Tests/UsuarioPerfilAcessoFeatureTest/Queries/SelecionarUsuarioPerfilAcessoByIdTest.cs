using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioPerfilAcessoFeatureTest.Queries;

public class SelecionarUsuarioPerfilAcessoByIdTest
{
    private readonly Mock<IRepository<UsuarioPerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly SelecionarUsuarioPerfilAcessoByIdQueryValidator _validator = new();
    private readonly SelecionarUsuarioPerfilAcessoByIdQueryHandler _handler;

    public SelecionarUsuarioPerfilAcessoByIdTest()
    {
        _handler = new SelecionarUsuarioPerfilAcessoByIdQueryHandler
        (
            _repositoryMock.Object,
            _validator
        );
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SelecionarUsuarioPerfilAcessoByIdQueryHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() =>
            new SelecionarUsuarioPerfilAcessoByIdQueryHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_querying_repository()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarUsuarioPerfilAcessoByIdQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_map_related_external_ids()
    {
        var request = new SelecionarUsuarioPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
        var entity = new UsuarioPerfilAcesso
        {
            IdExterno = request.Id,
            Usuario = new Usuario { IdExterno = Guid.NewGuid() },
            PerfilAcesso = new PerfilAcesso { IdExterno = Guid.NewGuid() },
            DataCadastro = DateTime.UtcNow.AddDays(-1)
        };
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, object>>[]>()
            ))
            .ReturnsAsync(entity);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(entity.Usuario.IdExterno, response.UsuarioId);
        Assert.Equal(entity.PerfilAcesso.IdExterno, response.PerfilAcessoId);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(),
            CancellationToken.None,
            It.IsAny<Expression<Func<UsuarioPerfilAcesso, object>>[]>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_throw_not_found_when_repository_returns_null()
    {
        var request = new SelecionarUsuarioPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(),
                CancellationToken.None,
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, object>>[]>()
            ))
            .ReturnsAsync((UsuarioPerfilAcesso)null!);

        await Assert.ThrowsAsync<ValidationException>
        (
            () => _handler.Handle(request, CancellationToken.None)
        );
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new SelecionarUsuarioPerfilAcessoByIdQuery());

        result.ShouldHaveValidationErrorFor(item => item.Id);
    }
}