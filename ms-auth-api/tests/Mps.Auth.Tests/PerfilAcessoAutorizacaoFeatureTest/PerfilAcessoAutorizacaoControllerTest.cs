using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Mps.Auth.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.PerfilAcessoAutorizacaoFeatureTest;

public class PerfilAcessoAutorizacaoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly PerfilAcessoAutorizacaoController _controller;

    public PerfilAcessoAutorizacaoControllerTest()
    {
        _controller = new PerfilAcessoAutorizacaoController
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
            () => new PerfilAcessoAutorizacaoController(null!, ControllerTestContext.Configuration)
        );

        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public async Task Get_by_filter_should_send_request_and_return_response()
    {
        var request = new SelecionarPerfilAcessoAutorizacaoByFilterQuery();
        IEnumerable<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse> expected = new[] { new SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse() };

        await AssertSuccess(request, expected, () => _controller.Get(request));
    }

    [Fact]
    public async Task Get_by_id_should_send_request_and_return_response()
    {
        var request = new SelecionarPerfilAcessoAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        var expected = new SelecionarPerfilAcessoAutorizacaoByIdQueryResponse();

        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_should_send_request_and_return_response()
    {
        var request = new InserirPerfilAcessoAutorizacaoCommand();
        var expected = new InserirPerfilAcessoAutorizacaoCommandResponse();

        await AssertSuccess(request, expected, () => _controller.Post(request));
    }

    [Fact]
    public async Task Put_should_send_request_and_return_response()
    {
        var request = new AtualizarPerfilAcessoAutorizacaoCommand();
        var expected = new AtualizarPerfilAcessoAutorizacaoCommandResponse();

        await AssertSuccess(request, expected, () => _controller.Put(request));
    }

    [Fact]
    public async Task Delete_should_create_request_with_id_and_return_response()
    {
        var id = Guid.NewGuid();
        var expected = new RemoverPerfilAcessoAutorizacaoCommandResponse();
        _mediatorMock
            .Setup(item => item.Send(It.Is<RemoverPerfilAcessoAutorizacaoCommand>(request => request.Id == id), CancellationToken.None))
            .ReturnsAsync(expected);

        var response = await _controller.Delete(id);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(It.Is<RemoverPerfilAcessoAutorizacaoCommand>(request => request.Id == id), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_by_filter_should_propagate_mediator_error()
    {
        var request = new SelecionarPerfilAcessoAutorizacaoByFilterQuery();
        var expected = new InvalidOperationException("fatal error");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Get(request));

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
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
