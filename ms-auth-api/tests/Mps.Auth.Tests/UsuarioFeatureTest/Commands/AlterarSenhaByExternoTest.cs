using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class AlterarSenhaByExternoTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly AlterarSenhaByExternoCommandValidator _validator = new();
    private readonly AlterarSenhaByExternoCommandHandler _handler;

    public AlterarSenhaByExternoTest() => _handler = new AlterarSenhaByExternoCommandHandler(_repositoryMock.Object, _validator);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AlterarSenhaByExternoCommandHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AlterarSenhaByExternoCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AlterarSenhaByExternoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_change_external_user_password_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AlterarSenhaByExternoCommand(usuarioLogadoId) { UsuarioId = Guid.NewGuid(), SenhaNova = "NovaSenha123", SenhaConfirmacao = "NovaSenha123" };
        var entity = new Usuario { IdExterno = request.UsuarioId, Senha = "senha-antiga" };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotEqual("senha-antiga", entity.Senha);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_and_mismatched_passwords()
    {
        var result = _validator.TestValidate(new AlterarSenhaByExternoCommand { SenhaNova = "um", SenhaConfirmacao = "dois" });
        result.ShouldHaveValidationErrorFor(item => item.UsuarioId);
        result.ShouldHaveValidationErrorFor(item => item.SenhaConfirmacao);
    }
}