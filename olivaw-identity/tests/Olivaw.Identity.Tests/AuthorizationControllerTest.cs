using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.AuthorizationFeature;
using Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;
using Olivaw.Identity.Api.Features.AuthorizationFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AuthorizationControllerTest
{
    [Fact]
    public async Task Create_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new CreateAuthorizationCommand();
        var response = new CreateAuthorizationResponse();

        CreateAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<CreateAuthorizationCommand>(), token))
            .Callback<IRequest<CreateAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<CreateAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Create(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<CreateAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new UpdateAuthorizationCommand();
        var response = new UpdateAuthorizationResponse();
        var id = Guid.NewGuid();
        UpdateAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<UpdateAuthorizationCommand>(), token))
            .Callback<IRequest<UpdateAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<UpdateAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Update(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<UpdateAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new DeleteAuthorizationCommand();
        var response = new DeleteAuthorizationResponse();
        var id = Guid.NewGuid();
        DeleteAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<DeleteAuthorizationCommand>(), token))
            .Callback<IRequest<DeleteAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<DeleteAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Delete(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<DeleteAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAuthorizationByIdQuery();
        var response = new ReadAuthorizationByIdResponse();
        var id = Guid.NewGuid();
        ReadAuthorizationByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAuthorizationByIdQuery>(), token))
            .Callback<IRequest<ReadAuthorizationByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAuthorizationByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<ReadAuthorizationByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAuthorizationByFilterQuery();
        var response = new ReadAuthorizationByFilterResponse();

        ReadAuthorizationByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAuthorizationByFilterQuery>(), token))
            .Callback<IRequest<ReadAuthorizationByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAuthorizationByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<ReadAuthorizationByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
