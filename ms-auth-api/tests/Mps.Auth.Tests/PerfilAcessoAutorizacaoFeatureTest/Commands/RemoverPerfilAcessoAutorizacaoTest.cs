using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoAutorizacaoFeatureTest.Commands;

public class RemoverPerfilAcessoAutorizacaoTest
{
    private readonly Mock<IRepository<PerfilAcessoAutorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly RemoverPerfilAcessoAutorizacaoCommandValidator _validator = new();
    private readonly RemoverPerfilAcessoAutorizacaoCommandHandler _handler;

    public RemoverPerfilAcessoAutorizacaoTest()
    {
        _handler = new RemoverPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoverPerfilAcessoAutorizacaoCommandHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new RemoverPerfilAcessoAutorizacaoCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<RemoverPerfilAcessoAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_remove_entity_and_return_external_id()
    {
        var request = new RemoverPerfilAcessoAutorizacaoCommand { Id = Guid.NewGuid() };
        var entity = new PerfilAcessoAutorizacao { IdExterno = request.Id };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync(entity);
        _repositoryMock.Setup(item => item.RemoveAsync(entity, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Id, response.Id);
        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.RemoveAsync(entity, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_missing_entity()
    {
        var request = new RemoverPerfilAcessoAutorizacaoCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None)).ReturnsAsync((PerfilAcessoAutorizacao)null!);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _repositoryMock.Verify(item => item.GetFirstAsync(It.IsAny<Expression<Func<PerfilAcessoAutorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_id()
    {
        var result = _validator.TestValidate(new RemoverPerfilAcessoAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Id);
    }
}