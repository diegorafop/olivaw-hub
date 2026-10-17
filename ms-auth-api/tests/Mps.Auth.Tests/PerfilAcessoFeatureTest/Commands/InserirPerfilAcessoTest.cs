using FluentValidation;
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

public class InserirPerfilAcessoTest
{
    private readonly Mock<IRepository<PerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly InserirPerfilAcessoCommandValidator _validator = new();
    private readonly InserirPerfilAcessoCommandHandler _handler;

    public InserirPerfilAcessoTest()
    {
        _handler = new InserirPerfilAcessoCommandHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<InserirPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_persist_new_profile()
    {
        var request = new InserirPerfilAcessoCommand
        {
            Nome = "Administrador",
            Descricao = "Descrição"
        };
        PerfilAcesso? capturedEntity = null;
        _repositoryMock
            .Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(item => item.AddAsync(It.IsAny<PerfilAcesso>(), CancellationToken.None))
            .Callback<PerfilAcesso, CancellationToken>((entity, _) => capturedEntity = entity)
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedEntity);
        Assert.Equal(request.Nome, capturedEntity!.Nome);
        Assert.Equal(request.Descricao, capturedEntity.Descricao);
        Assert.NotNull(response);
        _repositoryMock.Verify(item => item.ExistsAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<PerfilAcesso>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_profile_name()
    {
        _repositoryMock
            .Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>
        (
            () => _handler.Handle(new InserirPerfilAcessoCommand { Nome = "Administrador" }, CancellationToken.None)
        );
        _repositoryMock.Verify(item => item.ExistsAsync(
            It.IsAny<Expression<Func<PerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_name()
    {
        var result = _validator.TestValidate(new InserirPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Nome)
            .WithErrorMessage(MessageHelper
                .EmptyFor<InserirPerfilAcessoCommand, string>
                (item => item.Nome));
    }
}