using FluentValidation.TestHelper;
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
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest.Commands;

public class AutenticarUsuarioComAdTest
{
    private readonly Mock<IPublishEndpoint> _publisherMock = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutenticacao>> _authRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutorizacao>> _userAuthRepositoryMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = CreateConfiguration();
    private readonly AutenticarUsuarioComAdCommandValidator _validator = new();
    private readonly AutenticarUsuarioComAdCommandHandler _handler;

    public AutenticarUsuarioComAdTest()
    {
        _handler = new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, _repositoryMock.Object, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(null!, _configuration, _mediatorMock.Object, _repositoryMock.Object, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, null!, _mediatorMock.Object, _repositoryMock.Object, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, null!, _repositoryMock.Object, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, null!, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, _repositoryMock.Object, null!, _userAuthRepositoryMock.Object, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, _repositoryMock.Object, _authRepositoryMock.Object, null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AutenticarUsuarioComAdCommandHandler(_publisherMock.Object, _configuration, _mediatorMock.Object, _repositoryMock.Object, _authRepositoryMock.Object, _userAuthRepositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AutenticarUsuarioComAdCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
        _userAuthRepositoryMock.VerifyNoOtherCalls();
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertUsuarioAsync_should_update_existing_user_and_add_authentication()
    {
        var request = new AutenticarUsuarioComAdCommand("token", Guid.NewGuid()) { Login = "login", Nome = "Nome", Email = "email@teste.local" };
        var entity = new Usuario { Id = 0, IdExterno = Guid.NewGuid(), Login = "login", Nome = "Nome", Email = "email@teste.local" };
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);

        var result = await _handler.UpsertUsuarioAsync(request, entity, CancellationToken.None);

        Assert.NotNull(result);
        _authRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_use_existing_claimed_user_and_persist_authentication()
    {
        var request = new AutenticarUsuarioComAdCommand(CreateToken(), Guid.NewGuid());
        var entity = new Usuario { Id = 10, IdExterno = Guid.NewGuid(), Login = "usuario", Nome = "Usuário", Email = "usuario@teste.local", Autorizacoes = [], Perfils = [] };
        _repositoryMock.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, object>>[]>())).ReturnsAsync(entity);
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _authRepositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        _repositoryMock.Verify(item => item.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, object>>[]>()), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validate_should_require_id_token()
    {
        var result = _validator.TestValidate(new AutenticarUsuarioComAdCommand());
        Assert.NotEmpty(result.Errors);
    }

    private static string CreateToken()
    {
        var token = new JwtSecurityToken
        (
            issuer: "issuer",
            audience: "audience",
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("preferred_username", "usuario"),
                new Claim("name", "Usuário"),
                new Claim("email", "usuario@teste.local"),
                new Claim("matricula", "123")
            ]
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddConfiguration(new FakerBuilder().Configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["constants:login:funcionario"] = "funcionario",
                ["constants:login:gestor"] = "gestor"
            })
            .Build();
    }
}