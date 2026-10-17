using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Queries;

public class VerificarExistenciaUsuarioTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly VerificarExistenciaUsuarioCommandHandler _handler;

    public VerificarExistenciaUsuarioTest()
    {
        _handler = new VerificarExistenciaUsuarioCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new VerificarExistenciaUsuarioCommandHandler(null!)
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

        Assert.Equal(MessageHelper.NullFor<VerificarExistenciaUsuarioQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_report_each_requested_property()
    {
        var id = Guid.NewGuid();
        var request = new VerificarExistenciaUsuarioQuery
        {
            Id = id,
            Login = "usuario",
            Email = "usuario@teste.local"
        };
        var entity = new Usuario
        {
            IdExterno = id,
            Login = request.Login,
            Email = request.Email
        };
        Expression<Func<Usuario, bool>>? capturedPredicate = null;
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                CancellationToken.None
            ))
            .Callback<Expression<Func<Usuario, bool>>, CancellationToken, Expression<Func<Usuario, object>>[]>
            ((predicate, _, _) => capturedPredicate = predicate)
            .ReturnsAsync(new List<Usuario> { entity });

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedPredicate);
        Assert.True(capturedPredicate!.Compile()(entity));
        Assert.True(response.ExisteId);
        Assert.True(response.ExisteLogin);
        Assert.True(response.ExisteEmail);
        _repositoryMock.Verify(item => item.GetAsync(
            It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_return_false_for_missing_property_and_null_for_unrequested_property()
    {
        var request = new VerificarExistenciaUsuarioQuery { Login = "ausente" };
        var entity = new Usuario { Login = "outro" };
        _repositoryMock
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                CancellationToken.None
            ))
            .ReturnsAsync(new[] { entity });

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Null(response.ExisteId);
        Assert.False(response.ExisteLogin);
        Assert.Null(response.ExisteEmail);
    }
}