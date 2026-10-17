using FluentValidation;
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

public class InserirAutorizacaoTest
{
    private readonly Mock<IRepository<Autorizacao>> _repositoryMock = new(MockBehavior.Strict);
    private readonly InserirAutorizacaoCommandValidator _validator = new();
    private readonly InserirAutorizacaoCommandHandler _handler;

    public InserirAutorizacaoTest()
    {
        _handler = new InserirAutorizacaoCommandHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new InserirAutorizacaoCommandHandler(null!, _validator));
        Assert.Throws<ArgumentNullException>(() => new InserirAutorizacaoCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => _handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<InserirAutorizacaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_validate_duplicate_and_persist_new_authorization()
    {
        var request = new InserirAutorizacaoCommand
        {
            Tipo = "role",
            Valor = "admin",
            Nome = "Administrador",
            Descricao = "Descrição",
            Perfil = true
        };
        Autorizacao? capturedEntity = null;
        _repositoryMock
            .Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(item => item.AddAsync(It.IsAny<Autorizacao>(), CancellationToken.None))
            .Callback<Autorizacao, CancellationToken>((entity, _) => capturedEntity = entity)
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedEntity);
        Assert.Equal(request.Tipo, capturedEntity!.Tipo);
        Assert.Equal(request.Valor, capturedEntity.Valor);
        Assert.Equal(request.Nome, capturedEntity.Nome);
        Assert.Equal(request.Descricao, capturedEntity.Descricao);
        Assert.NotNull(response);
        _repositoryMock.Verify(item => item.ExistsAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<Autorizacao>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_authorization()
    {
        _repositoryMock
            .Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None))
            .ReturnsAsync(true);
        var request = new InserirAutorizacaoCommand { Tipo = "role", Valor = "admin" };

        await Assert.ThrowsAsync<ValidationException>
        (
            () => _handler.Handle(request, CancellationToken.None)
        );

        _repositoryMock.Verify(item => item.ExistsAsync(
            It.IsAny<Expression<Func<Autorizacao, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_require_type_and_value()
    {
        var result = _validator.TestValidate(new InserirAutorizacaoCommand());

        result.ShouldHaveValidationErrorFor(item => item.Tipo);
        result.ShouldHaveValidationErrorFor(item => item.Valor);
    }
}