using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;
using Mps.Auth.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioPerfilAcessoFeatureTest;

public class UsuarioPerfilAcessoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly UsuarioPerfilAcessoController _controller;

    public UsuarioPerfilAcessoControllerTest()
    {
        _controller = new UsuarioPerfilAcessoController(_mediatorMock.Object, ControllerTestContext.Configuration);
        ControllerTestContext.Configure(_controller);
    }

    [Fact]
    public void Constructor_should_have_error_when_mediator_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsuarioPerfilAcessoController(null!, ControllerTestContext.Configuration));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public async Task Get_by_id_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
        var expected = new SelecionarUsuarioPerfilAcessoByIdQueryResponse();
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_filter_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioPerfilAcessoByFilterQuery();
        IEnumerable<SelecionarUsuarioPerfilAcessoByFilterQueryResponse> expected = new[] { new SelecionarUsuarioPerfilAcessoByFilterQueryResponse() };
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_should_send_request_and_return_response()
    {
        var request = new InserirUsuarioPerfilAcessoCommand();
        var expected = new InserirUsuarioPerfilAcessoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_should_send_request_and_return_response()
    {
        var request = new AtualizarUsuarioPerfilAcessoCommand();
        var expected = new AtualizarUsuarioPerfilAcessoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_should_send_request_and_return_response()
    {
        var request = new RemoverUsuarioPerfilAcessoCommand { Id = Guid.NewGuid() };
        var expected = new RemoverUsuarioPerfilAcessoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Delete(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_id_should_propagate_mediator_error()
    {
        var request = new SelecionarUsuarioPerfilAcessoByIdQuery { Id = Guid.NewGuid() };
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
