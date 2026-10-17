using FluentValidation;
using FluentValidation.TestHelper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest.Commands;

public class AutenticarUsuarioImpersonateTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<UsuarioAutenticacao>> _authRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepositoryPerfilAcesso> _perfilRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IRepository<Autorizacao>> _autorizacaoRepositoryMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = new FakerBuilder().Configuration;

    private readonly AutenticarImpersonateCommandValidator _validator = new AutenticarImpersonateCommandValidator();

    private readonly AutenticarImpersonateCommandHandler _handler;

    public AutenticarUsuarioImpersonateTest() => _handler = new AutenticarImpersonateCommandHandler
    (
        _configuration,
        _mediatorMock.Object,
        _repositoryMock.Object,
        _authRepositoryMock.Object,
        _perfilRepositoryMock.Object,
        _autorizacaoRepositoryMock.Object,
        _validator
    );

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new AutenticarImpersonateCommandHandler
            (
                null!,
                _mediatorMock.Object,
                _repositoryMock.Object,
                _authRepositoryMock.Object,
                _perfilRepositoryMock.Object,
                _autorizacaoRepositoryMock.Object,
                _validator
            )
        );

        Assert.Throws<ArgumentNullException>
        (
            () => new AutenticarImpersonateCommandHandler
            (
                _configuration,
                null!,
                _repositoryMock.Object,
                _authRepositoryMock.Object,
                _perfilRepositoryMock.Object,
                _autorizacaoRepositoryMock.Object,
                _validator
            )
        );
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AutenticarImpersonateCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertUsuarioAutenticacaoAsync_should_add_authentication_for_existing_user()
    {
        var request = new AutenticarImpersonateCommand { Login = "login", Nome = "Nome", Email = "email@teste.local" };
        var entity = new Usuario { Id = 10, IdExterno = Guid.NewGuid(), Login = request.Login, Nome = request.Nome, Email = request.Email, Autorizacoes = Array.Empty<UsuarioAutorizacao>() };
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        var result = await _handler.UpsertUsuarioAutenticacaoAsync(request, entity, CancellationToken.None);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        _authRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None), Times.Once);
        _authRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_add_override_authorization_for_existing_user()
    {
        var request = new AutenticarImpersonateCommand { Login = "login", Nome = "Nome", Email = "email@teste.local" };
        var entity = new Usuario { Id = 10, IdExterno = Guid.NewGuid(), Login = request.Login, Nome = request.Nome, Email = request.Email, Autorizacoes = Array.Empty<UsuarioAutorizacao>(), Perfils = Array.Empty<UsuarioPerfilAcesso>() };
        var overrideAuthorization = new Autorizacao { Id = 20, IdExterno = Guid.NewGuid(), Tipo = "role", Valor = "override", Nome = "override" };
        _repositoryMock.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, object>>[]>())).ReturnsAsync(entity);
        _autorizacaoRepositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Autorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(overrideAuthorization);
        _authRepositoryMock.Setup(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);
        var response = await _handler.Handle(request, CancellationToken.None);
        Assert.Equal(entity.IdExterno, response.UsuarioId);
        Assert.Single(entity.Autorizacoes);
        _authRepositoryMock.Verify(item => item.AddAsync(It.IsAny<UsuarioAutenticacao>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validate_should_require_login_name_and_email()
    {
        var result = _handler;
        Assert.NotNull(result);
        var validator = new AutenticarImpersonateCommandValidator();
        Assert.NotEmpty(validator.TestValidate(new AutenticarImpersonateCommand()).Errors);
    }

    [Theory]
    [InlineData("AuthorityOverrideAdvogado")]
    [InlineData("AuthorityOverrideBeneficiario")]
    [InlineData("AuthorityOverrideProcurador")]
    public void Token_should_contain_generic_and_specific_role
    (
        string specificRole
    )
    {
        var configuration = new FakerBuilder().Configuration;

        var usuario = new Usuario
        {
            IdExterno = Guid.NewGuid(),
            Login = "personagem.teste",
            Nome = "Personagem de teste",
            Email = "personagem@teste.com",
            Autorizacoes =
            [
                CreateUsuarioAutorizacao("AuthorityOverride"),
                CreateUsuarioAutorizacao(specificRole)
            ],
            Perfils = []
        };

        AutenticarImpersonateCommandResponse response = usuario
            .ToAutenticarImpersonateCommandResponse
            (
                "refresh-token",
                configuration
            );

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(response.Token);

        string[] roles = token
            .Claims
            .Where(claim => claim.Type == "role")
            .Select(claim => claim.Value)
            .ToArray();

        Assert.Contains("AuthorityOverride", roles);
        Assert.Contains(specificRole, roles);
    }

    [Theory]
    [InlineData("AuthorityOverrideAdvogado")]
    [InlineData("AuthorityOverrideBeneficiario")]
    [InlineData("AuthorityOverrideProcurador")]
    public async Task Handler_should_find_role_and_return_it_in_token
    (
        string specificRole
    )
    {
        var configuration = new FakerBuilder().Configuration;

        var usuario = new Usuario
        {
            Id = 10,
            IdExterno = Guid.NewGuid(),
            Login = "personagem.teste",
            Nome = "Personagem de teste",
            Email = "personagem@teste.com",
            Autorizacoes = [],
            Perfils = []
        };

        var autorizacoes = new[]
        {
            new Autorizacao
            {
                Tipo = "Role",
                Valor = "AuthorityOverride"
            },
            new Autorizacao
            {
                Tipo = "Role",
                Valor = specificRole
            }
        };

        var repositoryUsuario = new Mock<IRepositoryUsuario>();

        repositoryUsuario
            .Setup(repository => repository.GetFirstOrDefaultAsync
            (
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Usuario, object>>[]>()
            ))
            .ReturnsAsync(usuario);

        repositoryUsuario
            .Setup(repository => repository.SaveChangesAsync
            (
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(1);

        var repositoryUsuarioAutenticacao =
            new Mock<IRepository<UsuarioAutenticacao>>();

        repositoryUsuarioAutenticacao
            .Setup(repository => repository.AddAsync
            (
                It.IsAny<UsuarioAutenticacao>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var repositoryAutorizacao = new Mock<IRepository<Autorizacao>>();

        repositoryAutorizacao
            .Setup(repository => repository.GetFirstAsync
            (
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Autorizacao, object>>[]>()
            ))
            .ReturnsAsync
            ((
                Expression<Func<Autorizacao, bool>> predicate,
                CancellationToken _,
                Expression<Func<Autorizacao, object>>[] _
            ) => autorizacoes.Single(predicate.Compile()));

        repositoryAutorizacao
            .Setup(repository => repository.GetAsync
            (
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Autorizacao, object>>[]>()
            ))
            .ReturnsAsync
            ((
                Expression<Func<Autorizacao, bool>> predicate,
                CancellationToken _,
                Expression<Func<Autorizacao, object>>[] _
            ) => autorizacoes.Where(predicate.Compile()));

        var handler = new AutenticarImpersonateCommandHandler
        (
            configuration,
            new Mock<IMediator>().Object,
            repositoryUsuario.Object,
            repositoryUsuarioAutenticacao.Object,
            new Mock<IRepositoryPerfilAcesso>().Object,
            repositoryAutorizacao.Object,
            new AutenticarImpersonateCommandValidator()
        );

        var request = new AutenticarImpersonateCommand
        {
            Login = usuario.Login,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AutorizacaoMany = [specificRole]
        };

        AutenticarImpersonateCommandResponse response = await handler.Handle
        (
            request,
            CancellationToken.None
        );

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(response.Token);

        string[] roles = token
            .Claims
            .Where(claim => claim.Type == "role")
            .Select(claim => claim.Value)
            .ToArray();

        Assert.Contains("AuthorityOverride", roles);
        Assert.Contains(specificRole, roles);
    }

    [Fact]
    public async Task Handler_should_reject_nonexistent_authorization()
    {
        const string specificRole = "AuthorityOverrideInexistente";

        var configuration = new FakerBuilder().Configuration;

        var usuario = new Usuario
        {
            Id = 10,
            IdExterno = Guid.NewGuid(),
            Login = "personagem.teste",
            Nome = "Personagem de teste",
            Email = "personagem@teste.com",
            Autorizacoes = [],
            Perfils = []
        };

        var autorizacoes = new[]
        {
            new Autorizacao
            {
                Tipo = "Role",
                Valor = "AuthorityOverride"
            }
        };

        var repositoryUsuario = new Mock<IRepositoryUsuario>();

        repositoryUsuario
            .Setup(repository => repository.GetFirstOrDefaultAsync
            (
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Usuario, object>>[]>()
            ))
            .ReturnsAsync(usuario);

        repositoryUsuario
            .Setup(repository => repository.SaveChangesAsync
            (
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(1);

        var repositoryUsuarioAutenticacao =
            new Mock<IRepository<UsuarioAutenticacao>>();

        repositoryUsuarioAutenticacao
            .Setup(repository => repository.AddAsync
            (
                It.IsAny<UsuarioAutenticacao>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        var repositoryAutorizacao = new Mock<IRepository<Autorizacao>>();

        repositoryAutorizacao
            .Setup(repository => repository.GetFirstAsync
            (
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Autorizacao, object>>[]>()
            ))
            .ReturnsAsync
            ((
                Expression<Func<Autorizacao, bool>> predicate,
                CancellationToken _,
                Expression<Func<Autorizacao, object>>[] _
            ) =>
            {
                Autorizacao? autorizacao = autorizacoes
                    .SingleOrDefault(predicate.Compile());

                return autorizacao ?? throw new ValidationException
                (
                    "Autorizacao não encontrado(a)."
                );
            });

        repositoryAutorizacao
            .Setup(repository => repository.GetAsync
            (
                It.IsAny<Expression<Func<Autorizacao, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Autorizacao, object>>[]>()
            ))
            .ReturnsAsync
            ((
                Expression<Func<Autorizacao, bool>> predicate,
                CancellationToken _,
                Expression<Func<Autorizacao, object>>[] _
            ) => autorizacoes.Where(predicate.Compile()));

        var handler = new AutenticarImpersonateCommandHandler
        (
            configuration,
            new Mock<IMediator>().Object,
            repositoryUsuario.Object,
            repositoryUsuarioAutenticacao.Object,
            new Mock<IRepositoryPerfilAcesso>().Object,
            repositoryAutorizacao.Object,
            new AutenticarImpersonateCommandValidator()
        );

        var request = new AutenticarImpersonateCommand
        {
            Login = usuario.Login,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AutorizacaoMany = [specificRole]
        };

        Task action() => handler.Handle
        (
            request,
            CancellationToken.None
        );

        await Assert.ThrowsAsync<ValidationException>(action);

        repositoryUsuarioAutenticacao.Verify
        (
            repository => repository.AddAsync
            (
                It.IsAny<UsuarioAutenticacao>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    private static UsuarioAutorizacao CreateUsuarioAutorizacao
    (
        string role
    )
    {
        return new UsuarioAutorizacao
        {
            Autorizacao = new Autorizacao
            {
                Tipo = "Role",
                Valor = role
            }
        };
    }
}