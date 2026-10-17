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

public class AtualizarPerfilAcessoTest
{
    private readonly Mock<IRepository<PerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarPerfilAcessoCommandValidator _validator = new();
    private readonly AtualizarPerfilAcessoCommandHandler _handler;

    public AtualizarPerfilAcessoTest()
    {
        _handler = new AtualizarPerfilAcessoCommandHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<AtualizarPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_validate_name_update_entity_and_save()
    {
        var request = new AtualizarPerfilAcessoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Operador",
            Descricao = "Atualizado"
        };
        var entity = new PerfilAcesso
        {
            IdExterno = request.Id,
            Nome = "Antigo",
            Descricao = "Anterior"
        };
        _repositoryMock
            .Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None))
            .ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Nome, entity.Nome);
        Assert.Equal(request.Descricao, entity.Descricao);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.ExistsAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id_and_name()
    {
        var result = _validator.TestValidate(new AtualizarPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.Nome);
    }
}