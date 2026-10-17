using FluentValidation.TestHelper;
using Moq;
using Mps.Ltmj.Api.Features.ExampleFeature.Queries;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Ltmj.Tests.ExampleFeatureTest.Queries;

public class SelecionarExampleByIdTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly SelecionarExampleByIdQueryValidator _validator;
    private readonly SelecionarExampleByIdQueryHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public SelecionarExampleByIdTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new SelecionarExampleByIdQueryValidator();
        _handler = new SelecionarExampleByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_have_error_when_repository_is_null()
    {
        IRepository<Example>? repository = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarExampleByIdQueryHandler(repository!)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        SelecionarExampleByIdQuery? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<SelecionarExampleByIdQuery>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_select_by_id_and_return_response()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByIdQuery
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

        var response = await _handler.Handle(request, cancellationToken);

        _repositoryMock.Verify(item => item.GetFirstAsync
        (
            It.IsAny<Expression<Func<Example, bool>>>(),
            cancellationToken
        ), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();

        Assert.IsType<SelecionarExampleByIdQueryResponse>(response);
        Assert.Equal(entity.IdExterno, response.Id);
        Assert.Equal(entity.DataCadastro, response.DataCadastro);
        Assert.Equal(entity.DataAtualizacao, response.DataAtualizacao);
        Assert.Equal(entity.DataRemocao, response.DataRemocao);
        Assert.Equal(entity.UsuarioLogadoId, response.UsuarioLogadoId);
        Assert.Equal(entity.Name, response.Name);
        Assert.Equal(entity.Description, response.Description);
        Assert.Equal(entity.Permission, response.Permission);
    }

    [Fact]
    public void Validate_should_not_have_error_when_request_is_valid()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByIdQuery
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_have_error_when_id_is_empty()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByIdQuery
            .Generate();
        request.Id = Guid.Empty;

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(item => item.Id)
            .WithErrorMessage(MessageHelper.EmptyFor<SelecionarExampleByIdQuery, Guid>(item => item.Id));
    }
}
