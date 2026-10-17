using FluentValidation.TestHelper;
using Moq;
using Mps.Limpo.Api.Features.ExampleFeature.Commands;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Domain;
using Mps.HolosCore.Repositories;
using Mps.Limpo.Tests.Mocks;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Limpo.Tests.ExampleFeatureTest.Commands;

public class AlternarExampleTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly AlternarExampleCommandValidator _validator;
    private readonly AlternarExampleCommandHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public AlternarExampleTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new AlternarExampleCommandValidator();
        _handler = new AlternarExampleCommandHandler
        (
            _repositoryMock.Object,
            _validator
        );
    }

    [Fact]
    public void Constructor_should_have_error_when_repository_is_null()
    {
        IRepository<Example>? repository = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AlternarExampleCommandHandler(repository!, _validator)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_have_error_when_validator_is_null()
    {
        AlternarExampleCommandValidator? validator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AlternarExampleCommandHandler(_repositoryMock.Object, validator!)
        );

        Assert.Equal("validator", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        AlternarExampleCommand? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<AlternarExampleCommand>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_should_toggle_and_return_response(bool ativo)
    {
        var usuarioLogadoId = Guid.NewGuid();
        var generatedRequest = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommand
            .Generate();

        var request = new AlternarExampleCommand(usuarioLogadoId)
        {
            Id = generatedRequest.Id,
            Ativo = ativo
        };

        var entity = _fakerBuilder.DomainFakerBuilder.Example.Generate();
        entity.IdExterno = request.Id;
        entity.DataAtualizacao = null;
        entity.DataRemocao = ativo ? DateTime.UtcNow.AddDays(-1) : null;

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var startedAt = DateTime.UtcNow;

        _repositoryMock
            .Setup(item => item.GetFirstAsync
            (
                It.Is<Expression<Func<Example, bool>>>
                (
                    predicate => predicate.Compile()(entity)
                ),
                cancellationToken
            ))
            .ReturnsAsync(entity);

        _repositoryMock
            .Setup(item => item.UpdateAsync(entity, cancellationToken))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(item => item.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var response = await _handler.Handle(request, cancellationToken);

        _repositoryMock.Verify(item => item.GetFirstAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            cancellationToken
        ), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, cancellationToken), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(cancellationToken), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();

        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.NotNull(entity.DataAtualizacao);
        Assert.InRange(entity.DataAtualizacao.Value, startedAt, DateTime.UtcNow);

        if (ativo)
        {
            Assert.Null(entity.DataRemocao);
        }
        else
        {
            Assert.NotNull(entity.DataRemocao);
            Assert.InRange(entity.DataRemocao.Value, startedAt, DateTime.UtcNow);
        }

        Assert.IsType<AlternarExampleCommandResponse>(response);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        Assert.Equal(entity.DataRemocao, response.DataRemocao);
    }

    [Fact]
    public void Validate_should_not_have_error_when_request_is_valid()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommand
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_have_error_when_id_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommand
            .Generate();
        request.Id = Guid.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper.EmptyFor<AlternarExampleCommand, Guid>(item => item.Id));
    }
}
