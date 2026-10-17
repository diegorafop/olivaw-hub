using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest.Commands;

public class AutenticarUsuarioComGovAutorizacaoManyTest
{
    private readonly Mock<IPublishEndpoint> _publisherMock = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutorizacao>> _userAuthRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutenticacao>> _authRepositoryMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;
    private readonly AutenticarUsuarioComGovAutorizacaoManyCommandHandler _handler;

    public AutenticarUsuarioComGovAutorizacaoManyTest() => _handler = new AutenticarUsuarioComGovAutorizacaoManyCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, _repositoryMock.Object, _autorizacaoRepositoryMock.Object, _userAuthRepositoryMock.Object, _authRepositoryMock.Object);

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AutenticarUsuarioComGovAutorizacaoManyCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertUsuarioAsync_should_update_existing_user_and_save_authentication()
    {
        var request = new AutenticarUsuarioComGovAutorizacaoManyCommand { AutorizacaoMany = Array.Empty<string>() };
        var entity = new Usuario { Id = 0, IdExterno = Guid.NewGuid(), Login = "login", Nome = "Nome", Email = "email@teste.local", Autorizacoes = Array.Empty<UsuarioAutorizacao>(), Perfils = Array.Empty<UsuarioPerfilAcesso>() };
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _authRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var result = await _handler.UpsertUsuarioAsync(request, entity, CancellationToken.None);

        Assert.NotNull(result);
        _authRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None), Times.Once);
        _authRepositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_should_use_claimed_user_without_requested_authorizations()
    {
        var request = new AutenticarUsuarioComGovAutorizacaoManyCommand { AutorizacaoMany = Array.Empty<string>() };
        typeof(AutenticarUsuarioComGovAutorizacaoManyCommand).GetProperty("IdToken", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(request, CreateToken());
        var entity = new Usuario { Id = 0, IdExterno = Guid.NewGuid(), Login = "usuario", Nome = "Usuário", Email = "usuario@teste.local", Autorizacoes = Array.Empty<UsuarioAutorizacao>(), Perfils = Array.Empty<UsuarioPerfilAcesso>() };
        _repositoryMock.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, object>>[]>())).ReturnsAsync(entity);
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _authRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _userAuthRepositoryMock.Setup(item => item.AddCollectionAsync(It.IsAny<IEnumerable<UsuarioAutorizacao>>(), CancellationToken.None)).Returns(Task.CompletedTask);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
    }

    private static string CreateToken()
    {
        var token = new JwtSecurityToken("issuer", "audience", new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("preferred_username", "usuario"),
            new Claim("name", "Usuário"),
            new Claim("email", "usuario@teste.local"),
            new Claim("matricula", "123")
        });
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}