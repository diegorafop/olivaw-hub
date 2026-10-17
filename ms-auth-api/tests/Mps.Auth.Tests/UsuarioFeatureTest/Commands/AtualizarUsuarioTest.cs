using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class AtualizarUsuarioTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarUsuarioCommandValidator _validator = new();
    private readonly AtualizarUsuarioCommandHandler _handler;

    public AtualizarUsuarioTest() => _handler = new AtualizarUsuarioCommandHandler(_repositoryMock.Object, _validator);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioCommandHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AtualizarUsuarioCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_user_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarUsuarioCommand(usuarioLogadoId) { Id = Guid.NewGuid(), Login = "novo.login", Nome = "Novo Nome", Email = "novo@teste.local", IntegracaoId = "900", StatusIntegracaoEnum = StatusIntegracaoEnum.Integrado };
        var entity = new Usuario { IdExterno = request.Id, Nome = "Antigo", Login = "antigo", Email = "antigo@teste.local" };
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(false);
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Login, entity.Login);
        Assert.Equal(request.Nome, entity.Nome);
        Assert.Equal(request.Email, entity.Email);
        Assert.Equal(request.IntegracaoId, entity.IntegracaoId);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_name()
    {
        var request = new AtualizarUsuarioCommand { Id = Guid.NewGuid(), Login = "login", Nome = "Duplicado", Email = "email@teste.local" };
        _repositoryMock.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.ExistsAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_id_login_and_email()
    {
        var result = _validator.TestValidate(new AtualizarUsuarioCommand());
        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.Login);
        result.ShouldHaveValidationErrorFor(item => item.Email);
    }
}