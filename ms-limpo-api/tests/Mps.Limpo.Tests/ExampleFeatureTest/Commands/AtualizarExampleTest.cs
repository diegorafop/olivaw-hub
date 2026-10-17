using FluentValidation;
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

public class AtualizarExampleTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly AtualizarExampleCommandValidator _validator;
    private readonly AtualizarExampleCommandHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public AtualizarExampleTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new AtualizarExampleCommandValidator();
        _handler = new AtualizarExampleCommandHandler
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
            () => new AtualizarExampleCommandHandler(repository!, _validator)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_have_error_when_validator_is_null()
    {
        AtualizarExampleCommandValidator? validator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AtualizarExampleCommandHandler(_repositoryMock.Object, validator!)
        );

        Assert.Equal("validator", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        AtualizarExampleCommand? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<AtualizarExampleCommand>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_update_and_return_response()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var generatedRequest = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();

        var request = new AtualizarExampleCommand(usuarioLogadoId)
        {
            Id = generatedRequest.Id,
            Name = generatedRequest.Name,
            Description = generatedRequest.Description,
            Permission = generatedRequest.Permission
        };

        var entity = _fakerBuilder.DomainFakerBuilder.Example.Generate();
        entity.IdExterno = request.Id;
        entity.Name = "Nome anterior";
        entity.Description = "Descrição anterior";
        entity.Permission = !request.Permission;
        entity.DataAtualizacao = null;

        var duplicateCandidate = new Example
        {
            IdExterno = Guid.NewGuid(),
            Name = request.Name
        };

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var startedAt = DateTime.UtcNow;

        _repositoryMock
            .Setup(item => item.ExistsAsync
            (
                It.Is<Expression<Func<Example, bool>>>
                (
                    predicate => predicate.Compile()(duplicateCandidate)
                ),
                cancellationToken
            ))
            .ReturnsAsync(false);

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

        _repositoryMock.Verify(item => item.ExistsAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            cancellationToken
        ), Times.Once);
        _repositoryMock.Verify(item => item.GetFirstAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            cancellationToken
        ), Times.Once);
        _repositoryMock.Verify(item => item.UpdateAsync(entity, cancellationToken), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(cancellationToken), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();

        Assert.Equal(request.Name, entity.Name);
        Assert.Equal(request.Description, entity.Description);
        Assert.Equal(request.Permission, entity.Permission);
        Assert.Equal(usuarioLogadoId, entity.UsuarioLogadoId);
        Assert.NotNull(entity.DataAtualizacao);
        Assert.InRange(entity.DataAtualizacao.Value, startedAt, DateTime.UtcNow);

        Assert.IsType<AtualizarExampleCommandResponse>(response);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
    }

    [Fact]
    public async Task Handle_should_have_error_when_name_already_exists_for_another_entity()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
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
            .AtualizarExampleCommand
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_have_error_when_id_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();
        request.Id = Guid.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper.EmptyFor<AtualizarExampleCommand, Guid>(item => item.Id));
    }

    [Fact]
    public void Validate_should_have_error_when_name_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();
        request.Name = string.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Name)
            .WithErrorMessage(MessageHelper.EmptyFor<AtualizarExampleCommand, string>(item => item.Name));
    }

    [Fact]
    public void Validate_should_have_error_when_description_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();
        request.Description = string.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Description)
            .WithErrorMessage(MessageHelper.EmptyFor<AtualizarExampleCommand, string>(item => item.Description));
    }
}
