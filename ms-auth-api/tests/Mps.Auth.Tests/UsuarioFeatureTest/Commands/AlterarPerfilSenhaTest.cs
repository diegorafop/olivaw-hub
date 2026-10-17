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

public class AlterarPerfilSenhaTest
{
    private readonly Mock<IRepositoryUsuario> _repositoryMock = new(MockBehavior.Strict);
    private readonly AlterarPerfilSenhaCommandHandler _handler;
    private readonly AlterarPerfilSenhaCommandValidator _validator = new();

    public AlterarPerfilSenhaTest() => _handler = new AlterarPerfilSenhaCommandHandler(_repositoryMock.Object);

    [Fact]
    public void Constructor_should_reject_null_repository() => Assert.Throws<ArgumentNullException>(() => new AlterarPerfilSenhaCommandHandler(null!));

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AlterarPerfilSenhaCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_change_password_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AlterarPerfilSenhaCommand(usuarioLogadoId) { Id = Guid.NewGuid(), Senha = "NovaSenha123" };
        var entity = new Usuario { IdExterno = request.Id, Senha = "senha-antiga" };
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
    public void Validate_should_require_id_and_password()
    {
        var result = _validator.TestValidate(new AlterarPerfilSenhaCommand());
        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.Senha);
    }
}