using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;
using Mps.Auth.Api.Features.PerfisAcessoFeature;
using Mps.Auth.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoFeatureTest;

public class PerfilAcessoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly PerfilAcessoController _controller;

    public PerfilAcessoControllerTest()
    {
        _controller = new PerfilAcessoController
        (
            _mediatorMock.Object,
            ControllerTestContext.Configuration
        );
        ControllerTestContext.Configure(_controller);
    }

    [Fact]
    public void Constructor_should_have_error_when_mediator_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new PerfilAcessoController(null!, ControllerTestContext.Configuration)
        );

        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public async Task Get_by_id_should_send_request_and_return_response()
    {
        var request = new SelecionarPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
        var expected = new SelecionarPerfilAcessoByIdQueryResponse();

        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_filter_should_send_request_and_return_response()
    {
        var request = new SelecionarPerfilAcessoByFilterQuery();
        IEnumerable<SelecionarPerfilAcessoByFilterQueryResponse> expected = new[] { new SelecionarPerfilAcessoByFilterQueryResponse() };

        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_should_send_request_and_return_response()
    {
        var request = new InserirPerfilAcessoCommand();
        var expected = new InserirPerfilAcessoCommandResponse();

        await AssertSuccess(request, expected, () => _controller.Post(request));
    }

    [Fact]
    public async Task Put_should_send_request_and_return_response()
    {
        var request = new AtualizarPerfilAcessoCommand();
        var expected = new AtualizarPerfilAcessoCommandResponse();

        await AssertSuccess(request, expected, () => _controller.Put(request));
    }

    [Fact]
    public async Task Delete_should_create_request_with_id_and_return_response()
    {
        var id = Guid.NewGuid();
        var expected = new RemoverPerfilAcessoCommandResponse();
        _mediatorMock.Setup(item => item.Send(It.Is<RemoverPerfilAcessoCommand>(request => request.Id == id), CancellationToken.None)).ReturnsAsync(expected);

        var response = await _controller.Delete(id);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(It.Is<RemoverPerfilAcessoCommand>(request => request.Id == id), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Alternar_should_send_request_and_return_response()
    {
        var request = new AlternarPerfilAcessoCommand();
        var expected = new AlternarPerfilAcessoCommandResponse();

        await AssertSuccess(request, expected, () => _controller.Alternar(request));
    }

    private async Task AssertSuccess<TRequest, TResponse>
    (
        TRequest request,
        TResponse expected,
        Func<Task<ActionResult>> action
    ) where TRequest : IRequest<TResponse>
    {
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ReturnsAsync(expected);

        var response = await action();

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}
