using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Limpo.Api.Features.ExampleFeature;
using Mps.Limpo.Api.Features.ExampleFeature.Commands;
using Mps.Limpo.Api.Features.ExampleFeature.Queries;
using Mps.Limpo.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Limpo.Tests.ExampleFeatureTest;

public class ExampleControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ExampleController _controller;

    private readonly FakerBuilder _fakerBuilder;

    public ExampleControllerTest()
    {
        _fakerBuilder = new FakerBuilder();
        _mediatorMock = new Mock<IMediator>();

        _controller = new ExampleController
        (
            _mediatorMock.Object,
            _fakerBuilder.Configuration
        );
    }

    [Fact]
    public void Constructor_should_have_error()
    {
        IMediator? mediator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new ExampleController
            (
                mediator,
                _fakerBuilder.Configuration
            )
        );

        Assert.Equal
        (
            new ArgumentNullException(nameof(mediator)).Message,
            exception.Message
        );
    }

    [Fact]
    public async Task Get_by_id_should_not_have_error()
    {
        var result = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByIdQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None))
            .ReturnsAsync(result);

        var response = await _controller.Get(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None);

        _mediatorMock.Verify(item => item.Send(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);

        var resultResponse = ((OkObjectResult)response).Value as SelecionarExampleByIdQueryResponse;
        Assert.NotNull(resultResponse);

        Assert.IsType<SelecionarExampleByIdQueryResponse>(resultResponse);
        Assert.Equal(result.Name, resultResponse.Name);
        Assert.Equal(result.Description, resultResponse.Description);
        Assert.Equal(result.Permission, resultResponse.Permission);
    }

    [Fact]
    public async Task Get_by_id_should_have_error()
    {
        var result = _fakerBuilder
            .ExampleFeature
            .SelecionarExampleByIdQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None))
            .ReturnsAsync(result)
            .Callback(() =>
            {
                throw new Exception("fatal error");
            });

        Task action() => _controller.Get(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(It.IsAny<SelecionarExampleByIdQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Post_should_not_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();

        var result = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommandResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(request, CancellationToken.None))
            .ReturnsAsync(result);

        var response = await _controller.Post(request);

        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);

        var resultResponse = ((OkObjectResult)response).Value as InserirExampleCommandResponse;
        Assert.NotNull(resultResponse);

        Assert.IsType<InserirExampleCommandResponse>(resultResponse);
        Assert.Equal(result.Id, resultResponse.Id);
        Assert.Equal(result.DataCadastro, resultResponse.DataCadastro);
        Assert.Equal(result.UsuarioLogadoId, resultResponse.UsuarioLogadoId);
    }

    [Fact]
    public async Task Post_should_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .InserirExampleCommand
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(request, CancellationToken.None))
            .ThrowsAsync(new Exception("fatal error"));

        Task action() => _controller.Post(request);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Put_should_not_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();

        var result = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommandResponse
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ReturnsAsync(result);

        var response = await _controller.Put(request, cancellationToken);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);

        var resultResponse = ((OkObjectResult)response).Value as AtualizarExampleCommandResponse;
        Assert.NotNull(resultResponse);

        Assert.IsType<AtualizarExampleCommandResponse>(resultResponse);
        Assert.Equal(result.DataAtualizacao, resultResponse.DataAtualizacao);
    }

    [Fact]
    public async Task Put_should_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AtualizarExampleCommand
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ThrowsAsync(new Exception("fatal error"));

        Task action() => _controller.Put(request, cancellationToken);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_not_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommand
            .Generate();

        var result = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommandResponse
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ReturnsAsync(result);

        var response = await _controller.Delete(request, cancellationToken);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);

        var resultResponse = ((OkObjectResult)response).Value as RemoverExampleCommandResponse;
        Assert.NotNull(resultResponse);

        Assert.IsType<RemoverExampleCommandResponse>(resultResponse);
        Assert.Equal(result.Id, resultResponse.Id);
    }

    [Fact]
    public async Task Delete_should_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .RemoverExampleCommand
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ThrowsAsync(new Exception("fatal error"));

        Task action() => _controller.Delete(request, cancellationToken);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Put_alternar_should_not_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommand
            .Generate();

        var result = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommandResponse
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ReturnsAsync(result);

        var response = await _controller.Put(request, cancellationToken);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);

        var resultResponse = ((OkObjectResult)response).Value as AlternarExampleCommandResponse;
        Assert.NotNull(resultResponse);

        Assert.IsType<AlternarExampleCommandResponse>(resultResponse);
        Assert.Equal(result.DataAtualizacao, resultResponse.DataAtualizacao);
        Assert.Equal(result.DataRemocao, resultResponse.DataRemocao);
    }

    [Fact]
    public async Task Put_alternar_should_have_error()
    {
        var request = _fakerBuilder
            .ExampleFeature
            .AlternarExampleCommand
            .Generate();

        var cancellationToken = new CancellationToken(canceled: true);

        _mediatorMock
            .Setup(item => item.Send(request, cancellationToken))
            .ThrowsAsync(new Exception("fatal error"));

        Task action() => _controller.Put(request, cancellationToken);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(request, cancellationToken), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}
