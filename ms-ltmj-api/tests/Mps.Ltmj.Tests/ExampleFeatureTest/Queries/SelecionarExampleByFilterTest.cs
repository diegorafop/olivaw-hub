using FluentValidation.TestHelper;
using Moq;
using Mps.Ltmj.Api.Features.ExampleFeature.Queries;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Ltmj.Tests.ExampleFeatureTest.Queries;

public class SelecionarExampleByFilterTest
{
    private readonly Mock<IRepository<Example>> _repositoryMock;
    private readonly SelecionarExampleByFilterQueryHandler.SelecionarExampleByFilterQueryValidator _validator;
    private readonly SelecionarExampleByFilterQueryHandler _handler;
    private readonly FakerBuilder _fakerBuilder;

    public SelecionarExampleByFilterTest()
    {
        _fakerBuilder = new FakerBuilder();
        _repositoryMock = new Mock<IRepository<Example>>();
        _validator = new SelecionarExampleByFilterQueryHandler.SelecionarExampleByFilterQueryValidator();
        _handler = new SelecionarExampleByFilterQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_should_have_error_when_repository_is_null()
    {
        IRepository<Example>? repository = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new SelecionarExampleByFilterQueryHandler(repository!)
        );

        Assert.Equal("repository", exception.ParamName);
    }

    [Fact]
    public async Task Handle_should_have_error_when_request_is_null()
    {
        SelecionarExampleByFilterQuery? request = null;

        Task action() => _handler.Handle(request!, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<SelecionarExampleByFilterQuery>()).Message,
            exception.Message
        );

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_select_by_filters_and_map_all_items()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByFilterQuery
            .Generate();

        var entities = _fakerBuilder
            .DomainFakerBuilder
            .Example
            .Generate(2);

        foreach (var entity in entities)
        {
            entity.Name = $"prefixo {request.Name.ToUpperInvariant()} sufixo";
            entity.Description = $"prefixo {request.Description.ToUpperInvariant()} sufixo";
        }

        _repositoryMock
            .Setup
            (
                item => item.GetAsync
                (
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(entities);

        var response = await _handler.Handle
        (
            request,
            CancellationToken.None
        );

        _repositoryMock
            .Verify
            (
                item => item.GetAsync
                (
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    CancellationToken.None
                ),
                Times.Once
            );

        _repositoryMock.VerifyNoOtherCalls();

        var itens = response.ToList();
        Assert.Equal(entities.Count, itens.Count);

        for (var index = 0; index < entities.Count; index++)
        {
            var entity = entities[index];
            var item = itens[index];

            Assert.Equal(entity.IdExterno, item.Id);
            Assert.Equal(entity.DataCadastro, item.DataCadastro);
            Assert.Equal(entity.DataAtualizacao, item.DataAtualizacao);
            Assert.Equal(entity.DataRemocao, item.DataRemocao);
            Assert.Equal(entity.UsuarioLogadoId, item.UsuarioLogadoId);
            Assert.Equal(entity.Name, item.Name);
            Assert.Equal(entity.Description, item.Description);
            Assert.Equal(entity.Permission, item.Permission);
        }
    }

    [Fact]
    public async Task Handle_should_return_empty_collection_when_repository_has_no_items()
    {
        var request = new SelecionarExampleByFilterQuery();

        _repositoryMock
            .Setup
            (
                item => item.GetAsync
                (
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    CancellationToken.None
                )
            )
            .ReturnsAsync([]);

        var response = await _handler.Handle
        (
            request,
            CancellationToken.None
        );

        _repositoryMock.Verify
        (
            item => item.GetAsync
            (
                It.IsAny<Expression<Func<Example, bool>>>(),
                CancellationToken.None
            ),
            Times.Once
        );

        _repositoryMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public void Validate_should_not_have_error_for_supported_filters()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByFilterQuery
            .Generate();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_should_not_have_error_when_filters_are_empty()
    {
        var request = new SelecionarExampleByFilterQuery();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}