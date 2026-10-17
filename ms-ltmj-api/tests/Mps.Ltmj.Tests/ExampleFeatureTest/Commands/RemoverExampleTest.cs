using FluentValidation.TestHelper;
using Moq;
using Mps.Ltmj.Api.Features.ExampleFeature.Commands;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Ltmj.Tests.ExampleFeatureTest.Commands;

public class RemoverExampleTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly RemoverExampleCommandValidator _validator;
    private readonly RemoverExampleCommandHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public RemoverExampleTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new RemoverExampleCommandValidator();
        _handler = new RemoverExampleCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_have_error_when_repository_is_null()
    {
        IRepository<Example>? repository = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new RemoverExampleCommandHandler(repository!)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        RemoverExampleCommand? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<RemoverExampleCommand>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_remove_and_return_response()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommand
            .Generate();

        var entity = _fakerBuilder.DomainFakerBuilder.Example.Generate();
        entity.IdExterno = request.Id;

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

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
            .Setup(item => item.RemoveAsync(entity, cancellationToken))
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
        _repositoryMock.Verify(item => item.RemoveAsync(entity, cancellationToken), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(cancellationToken), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();

        Assert.IsType<RemoverExampleCommandResponse>(response);
        Assert.Equal(entity.IdExterno, response.Id);
    }

    [Fact]
    public void Validate_should_not_have_error_when_request_is_valid()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommand
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_have_error_when_id_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommand
            .Generate();
        request.Id = Guid.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper.EmptyFor<RemoverExampleCommand, Guid>(item => item.Id));
    }
}
