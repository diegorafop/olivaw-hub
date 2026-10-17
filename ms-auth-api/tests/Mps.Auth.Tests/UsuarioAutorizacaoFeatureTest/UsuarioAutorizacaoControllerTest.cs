using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;
using Mps.Auth.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioAutorizacaoFeatureTest;

public class UsuarioAutorizacaoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly UsuarioAutorizacaoController _controller;

    public UsuarioAutorizacaoControllerTest()
    {
        _controller = new UsuarioAutorizacaoController(_mediatorMock.Object, ControllerTestContext.Configuration);
        ControllerTestContext.Configure(_controller);
    }

    [Fact]
    public void Constructor_should_have_error_when_mediator_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsuarioAutorizacaoController(null!, ControllerTestContext.Configuration));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public async Task Get_by_id_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        var expected = new SelecionarUsuarioAutorizacaoByIdQueryResponse();
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_filter_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioAutorizacaoByFilterQuery();
        IEnumerable<SelecionarUsuarioAutorizacaoByFilterQueryResponse> expected = new[] { new SelecionarUsuarioAutorizacaoByFilterQueryResponse() };
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_should_send_request_and_return_response()
    {
        var request = new InserirUsuarioAutorizacaoCommand();
        var expected = new InserirUsuarioAutorizacaoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Post(request));
    }

    [Fact]
    public async Task Put_should_send_request_and_return_response()
    {
        var request = new AtualizarUsuarioAutorizacaoCommand();
        var expected = new AtualizarUsuarioAutorizacaoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request));
    }

    [Fact]
    public async Task Delete_should_send_request_and_return_response()
    {
        var request = new RemoverUsuarioAutorizacaoCommand { Id = Guid.NewGuid() };
        var expected = new RemoverUsuarioAutorizacaoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Delete(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_id_should_propagate_mediator_error()
    {
        var request = new SelecionarUsuarioAutorizacaoByIdQuery { Id = Guid.NewGuid() };
        var expected = new InvalidOperationException("fatal error");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Get(request, CancellationToken.None));

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    private async Task AssertSuccess<TRequest, TResponse>(TRequest request, TResponse expected, Func<Task<ActionResult>> action)
        where TRequest : IRequest<TResponse>
    {
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ReturnsAsync(expected);
        var response = await action();
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}
