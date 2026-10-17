using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.AutorizacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutorizacaoFeatureTest.Commands;

public class AtualizarAutorizacaoTest
{
    private readonly Mock<IRepository<Autorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarAutorizacaoCommandValidator _validator = new();
    private readonly AtualizarAutorizacaoCommandHandler _handler;

    public AtualizarAutorizacaoTest()
    {
        _handler = new AtualizarAutorizacaoCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<AtualizarAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_entity_and_save_changes()
    {
        var request = new AtualizarAutorizacaoCommand
        {
            Id = Guid.NewGuid(),
            Tipo = "role",
            Valor = "operator",
            Nome = "Operador",
            Descricao = "Atualizado",
            Perfil = true
        };
        var entity = new Autorizacao
        {
            IdExterno = request.Id,
            Tipo = "role",
            Valor = "old",
            Nome = "Antigo"
        };
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None))
            .ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.UpdateAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Tipo, entity.Tipo);
        Assert.Equal(request.Valor, entity.Valor);
        Assert.Equal(request.Nome, entity.Nome);
        Assert.Equal(request.Descricao, entity.Descricao);
        Assert.Equal(request.Perfil, entity.Perfil);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_required_fields_when_empty()
    {
        var result = _validator.TestValidate(new AtualizarAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.Tipo);
        result.ShouldHaveValidationErrorFor(item => item.Valor);
    }
}