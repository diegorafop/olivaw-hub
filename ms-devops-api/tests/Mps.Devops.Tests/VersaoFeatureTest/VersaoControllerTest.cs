using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Devops.Api.Features.VersaoFeature;
using Mps.Devops.Api.Features.VersaoFeature.Queries;
using Mps.Devops.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Devops.Tests.VersaoFeatureTest;

public class VersaoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VersaoController _controller;

    private readonly FakerBuilder _fakerBuilder;

    public VersaoControllerTest()
    {
        _fakerBuilder = new FakerBuilder();
        _mediatorMock = new Mock<IMediator>();

        _controller = new VersaoController
        (
            _mediatorMock.Object
        );
    }

    [Fact]
    public void Constructor_should_have_error()
    {
        IMediator? mediator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new VersaoController(mediator)
        );

        Assert.Equal
        (
            new ArgumentNullException(nameof(mediator)).Message,
            exception.Message
        );
    }

    [Fact]
    public async Task Get_should_not_have_error()
    {
        var result = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None))
            .ReturnsAsync(result);

        var response = await _controller.Get(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None);

        _mediatorMock.Verify(item => item.Send(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);
        var resultResponse = ((OkObjectResult)response).Value as SelecionarVersaoQueryResponse;
        Assert.NotNull(resultResponse);
        Assert.IsType<SelecionarVersaoQueryResponse>(resultResponse);
        Assert.Equal(result.Versao, resultResponse.Versao);
        Assert.Equal(result.Titulo, resultResponse.Titulo);
    }

    [Fact]
    public async Task Get_should_have_error()
    {
        var result = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None))
            .ReturnsAsync(result)
            .Callback(() =>
            {
                throw new Exception("fatal error");
            });

        Task action() => _controller.Get(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(It.IsAny<SelecionarVersaoQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}