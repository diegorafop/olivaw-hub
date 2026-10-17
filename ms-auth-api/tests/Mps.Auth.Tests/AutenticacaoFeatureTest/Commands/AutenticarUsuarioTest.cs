using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Api.Helpers;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest.Commands;

public class AutenticarUsuarioTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutenticacao>> _authRepositoryMock = new(MockBehavior.Strict);
    private readonly AutenticarUsuarioExternalCommandValidator _validator = new();
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;
    private readonly AutenticarUsuarioExternalCommandHandler _handler;

    public AutenticarUsuarioTest() => _handler = new AutenticarUsuarioExternalCommandHandler(_configuration, _repositoryMock.Object, _authRepositoryMock.Object, _validator);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioExternalCommandHandler(null!, _repositoryMock.Object, _authRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioExternalCommandHandler(_configuration, null!, _authRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioExternalCommandHandler(_configuration, _repositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioExternalCommandHandler(_configuration, _repositoryMock.Object, _authRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AutenticarUsuarioExternalCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_authenticate_user_and_persist_refresh_token()
    {
        var request = new AutenticarUsuarioExternalCommand { Login = "usuario", Senha = "Senha123" };
        var entity = new Usuario { Id = 5, IdExterno = Guid.NewGuid(), Login = request.Login, Senha = CryptographyHelper.EnhancedHashPassword(request.Senha), Nome = "Usuário", Email = "usuario@teste.local", Autorizacoes = Array.Empty<UsuarioAutorizacao>(), Perfils = Array.Empty<UsuarioPerfilAcesso>() };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>())).ReturnsAsync(entity);
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _authRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(entity.IdExterno, response.UsuarioId);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.False(string.IsNullOrWhiteSpace(response.TokenRefresh));
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>()), Times.Once);
        _authRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None), Times.Once);
        _authRepositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_invalid_password()
    {
        var request = new AutenticarUsuarioExternalCommand { Login = "usuario", Senha = "errada" };
        var entity = new Usuario { Login = request.Login, Senha = CryptographyHelper.EnhancedHashPassword("correta"), Autorizacoes = Array.Empty<UsuarioAutorizacao>(), Perfils = Array.Empty<UsuarioPerfilAcesso>() };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>())).ReturnsAsync(entity);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>()), Times.Once);
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_login_or_email_and_password()
    {
        var result = _validator.TestValidate(new AutenticarUsuarioExternalCommand());
        result.ShouldHaveValidationErrorFor(item => item.Login);
        result.ShouldHaveValidationErrorFor(item => item.Email);
        result.ShouldHaveValidationErrorFor(item => item.Senha);
    }
}