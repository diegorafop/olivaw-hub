using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Mps.Limpo.Api.Features.ExampleFeature.Commands;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;
using Mps.Limpo.Tests.Mocks;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Limpo.Tests.ExampleFeatureTest.Commands;

public class InserirExampleTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly InserirExampleCommandValidator _validator;
    private readonly InserirExampleCommandHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public InserirExampleTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new InserirExampleCommandValidator();
        _handler = new InserirExampleCommandHandler
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
            () => new InserirExampleCommandHandler(repository!, _validator)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_have_error_when_validator_is_null()
    {
        InserirExampleCommandValidator? validator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new InserirExampleCommandHandler(_repositoryMock.Object, validator!)
        );

        Assert.Equal("validator", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        InserirExampleCommand? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<InserirExampleCommand>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_insert_and_return_response()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var generatedRequest = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();

        var request = new InserirExampleCommand(usuarioLogadoId)
        {
            Name = generatedRequest.Name,
            Description = generatedRequest.Description,
            Permission = generatedRequest.Permission
        };

        var matchingEntity = new Example { Name = request.Name };
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var startedAt = DateTime.UtcNow;
        Example? insertedEntity = null;

        _repositoryMock
            .Setup(item => item.ExistsAsync
            (
                It.Is<Expression<Func<Example, bool>>>
                (
                    predicate => predicate.Compile()(matchingEntity)
                ),
                cancellationToken
            ))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(item => item.AddAsync(It.IsAny<Example>(), cancellationToken))
            .Callback<Example, CancellationToken>
            (
                (entity, _) => insertedEntity = entity
            )
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(item => item.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var response = await _handler.Handle(request, cancellationToken);

        _repositoryMock.Verify(item => item.ExistsAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            cancellationToken
        ), Times.Once);
        _repositoryMock.Verify(item => item.AddAsync(It.IsAny<Example>(), cancellationToken), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(cancellationToken), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();

        Assert.NotNull(insertedEntity);
        Assert.Equal(request.Name, insertedEntity.Name);
        Assert.Equal(request.Description, insertedEntity.Description);
        Assert.Equal(request.Permission, insertedEntity.Permission);
        Assert.Equal(usuarioLogadoId, insertedEntity.UsuarioLogadoId);
        Assert.NotEqual(Guid.Empty, insertedEntity.IdExterno);
        Assert.InRange(insertedEntity.DataCadastro, startedAt, DateTime.UtcNow);
        Assert.Null(insertedEntity.DataAtualizacao);

        Assert.IsType<InserirExampleCommandResponse>(response);
        Assert.Equal(insertedEntity.IdExterno, response.Id);
        Assert.Equal(insertedEntity.DataCadastro, response.DataCadastro);
        Assert.Equal(insertedEntity.UsuarioLogadoId, response.UsuarioLogadoId);
    }

    [Fact]
    public async Task Handle_should_have_error_when_name_already_exists()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();

        _repositoryMock
            .Setup(item => item.ExistsAsync
            (
                It.IsAny<Expression<Func<Example, bool>>>(),
                CancellationToken.None
            ))
            .ReturnsAsync(true);

        Task action() => _handler.Handle(request, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidationException>(action);

        Assert.Contains
        (
            exception.Errors,
            failure => failure.PropertyName == nameof(request.Name)
                && failure.ErrorMessage == MessageHelper.DuplicateKeyFor
                (
                    request,
                    item => item.Name
                )
        );

        _repositoryMock.Verify(item => item.ExistsAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            CancellationToken.None
        ), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_not_have_error_when_request_is_valid()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_have_error_when_name_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();
        request.Name = string.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Name)
            .WithErrorMessage(MessageHelper.EmptyFor<InserirExampleCommand, string>(item => item.Name));
    }

    [Fact]
    public void Validate_should_have_error_when_description_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();
        request.Description = string.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Description)
            .WithErrorMessage(MessageHelper.EmptyFor<InserirExampleCommand, string>(item => item.Description));
    }
}
