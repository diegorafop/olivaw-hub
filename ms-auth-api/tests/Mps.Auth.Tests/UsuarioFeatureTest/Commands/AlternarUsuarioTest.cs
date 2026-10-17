using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class AlternarUsuarioTest
{
    private readonly Mock<IRepository<Usuario>> _repositoryMock = new(MockBehavior.Strict);
    private readonly AlternarUsuarioCommandValidator _validator = new();
    private readonly AlternarUsuarioCommandHandler _handler;

    public AlternarUsuarioTest() => _handler = new AlternarUsuarioCommandHandler(_repositoryMock.Object, _validator);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AlternarUsuarioCommandHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new AlternarUsuarioCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AlternarUsuarioCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_toggle_user_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AlternarUsuarioCommand(usuarioLogadoId) { Id = Guid.NewGuid(), Ativo = false };
        var entity = new Usuario { IdExterno = request.Id, DataRemocao = null };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(entity.DataRemocao);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new AlternarUsuarioCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
    }
}