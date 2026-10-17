using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoFeatureTest.Commands;

public class AlternarPerfilAcessoTest
{
    private readonly Mock<IRepository<PerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly AlternarPerfilAcessoCommandHandler _handler;
    private readonly AlternarPerfilAcessoCommandValidator _validator = new();

    public AlternarPerfilAcessoTest() => _handler = new AlternarPerfilAcessoCommandHandler(_repositoryMock.Object);

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        Assert.Throws<ArgumentNullException>(() => new AlternarPerfilAcessoCommandHandler(null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AlternarPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_toggle_profile_and_save_changes()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AlternarPerfilAcessoCommand(usuarioLogadoId) { Id = Guid.NewGuid(), Ativo = false };
        var entity = new PerfilAcesso { IdExterno = request.Id, DataRemocao = null };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(entity.DataRemocao);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new AlternarPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
    }
}