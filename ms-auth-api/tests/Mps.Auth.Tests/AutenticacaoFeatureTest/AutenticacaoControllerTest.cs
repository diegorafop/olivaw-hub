using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.AutenticacaoFeature;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutenticacaoFeatureTest;

public class AutenticacaoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AutenticacaoController _controller;

    public AutenticacaoControllerTest()
    {
        _controller = new AutenticacaoController
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
            () => new AutenticacaoController(null!, ControllerTestContext.Configuration)
        );

        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_have_error_when_configuration_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AutenticacaoController(_mediatorMock.Object, null!)
        );

        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public async Task Post_autenticar_should_send_request_and_return_response()
    {
        var request = new AutenticarUsuarioExternalCommand();
        var expected = new AutenticarUsuarioExternalCommandResponse();
        var response = await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));

        Assert.Same(expected, response);
    }

    [Fact]
    public async Task Post_autenticar_should_propagate_mediator_error()
    {
        var request = new AutenticarUsuarioExternalCommand();
        var expected = new InvalidOperationException("fatal error");
        _mediatorMock
            .Setup(item => item.Send(request, CancellationToken.None))
            .ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Post(request, CancellationToken.None)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Post_autenticar_com_ad_should_send_request_and_return_response()
    {
        var request = new AutenticarUsuarioComAdCommand();
        var expected = new AutenticarUsuarioComAdCommandResponse();
        await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_autenticar_com_gov_should_send_request_and_return_response()
    {
        var request = new AutenticarUsuarioComGovCommand();
        var expected = new AutenticarUsuarioComGovCommandResponse();
        await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_autenticar_com_gov_autorizacao_many_should_send_request_and_return_response()
    {
        var request = new AutenticarUsuarioComGovAutorizacaoManyCommand();
        var expected = new AutenticarUsuarioComGovAutorizacaoManyCommandResponse();
        await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_impersonate_should_send_request_and_return_response()
    {
        var request = new AutenticarImpersonateCommand();
        var expected = new AutenticarImpersonateCommandResponse();
        await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_deslogar_should_send_request_and_return_response()
    {
        var request = new DeslogarUsuarioCommand();
        var expected = new DeslogarUsuarioCommandResponse();
        await SendAndAssert(request, expected,
            () => _controller.Post(request, CancellationToken.None));
    }

    private async Task<TResponse> SendAndAssert<TRequest, TResponse>
    (
        TRequest request,
        TResponse expected,
        Func<Task<ActionResult>> action
    ) where TRequest : IRequest<TResponse>
    {
        _mediatorMock
            .Setup(item => item.Send(request, CancellationToken.None))
            .ReturnsAsync(expected);

        var response = await action();

        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        return (TResponse)result.Value!;
    }
}
