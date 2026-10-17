using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioPerfilAcessoFeatureTest.Commands;

public class RemoverUsuarioPerfilAcessoTest
{
    private readonly Mock<IRepository<UsuarioPerfilAcesso>> _repositoryMock = new(MockBehavior.Strict);
    private readonly RemoverUsuarioPerfilAcessoCommandValidator _validator = new();
    private readonly RemoverUsuarioPerfilAcessoCommandHandler _handler;

    public RemoverUsuarioPerfilAcessoTest()
    {
        _handler = new RemoverUsuarioPerfilAcessoCommandHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<RemoverUsuarioPerfilAcessoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_remove_entity_and_return_external_id()
    {
        var request = new RemoverUsuarioPerfilAcessoCommand { Id = Guid.NewGuid() };
        var entity = new UsuarioPerfilAcesso { IdExterno = request.Id };
        _repositoryMock
            .Setup(item => item.GetFirstAsync(
                It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None))
            .ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.RemoveAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Id, response.Id);
        _repositoryMock.Verify(item => item.GetFirstAsync(
            It.IsAny<Expression<Func<UsuarioPerfilAcesso, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.RemoveAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new RemoverUsuarioPerfilAcessoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper
                .EmptyFor<RemoverUsuarioPerfilAcessoCommand, Guid>
                (item => item.Id));
    }
}