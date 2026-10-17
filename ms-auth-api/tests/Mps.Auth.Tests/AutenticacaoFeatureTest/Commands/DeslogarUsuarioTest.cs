using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest.Commands;

public class DeslogarUsuarioTest
{
    private readonly Mock<IRepository<UsuarioAutenticacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly DeslogarUsuarioCommandHandler _handler;
    private readonly DeslogarUsuarioCommandValidator _validator = new();

    public DeslogarUsuarioTest() => _handler = new DeslogarUsuarioCommandHandler(_repositoryMock.Object);

    [Fact]
    public void Constructor_should_reject_null_repository() => Assert.Throws<ArgumentNullException>(() => new DeslogarUsuarioCommandHandler(null!));

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<DeslogarUsuarioCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_mark_authentication_as_removed_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new DeslogarUsuarioCommand(usuarioLogadoId) { Token = "token" };
        var entity = new UsuarioAutenticacao { Token = request.Token };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutenticacao, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(entity.DataRemocao);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<UsuarioAutenticacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_token()
    {
        var result = _validator.TestValidate(new DeslogarUsuarioCommand());
        result.ShouldHaveValidationErrorFor(item => item.Token);
    }
}