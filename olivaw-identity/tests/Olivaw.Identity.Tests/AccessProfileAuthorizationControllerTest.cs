using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature;
using Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Commands;
using Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AccessProfileAuthorizationControllerTest
{
    [Fact]
    public async Task Create_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileAuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new CreateAccessProfileAuthorizationCommand();
        var response = new CreateAccessProfileAuthorizationResponse();

        CreateAccessProfileAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<CreateAccessProfileAuthorizationCommand>(), token))
            .Callback<IRequest<CreateAccessProfileAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<CreateAccessProfileAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Create(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<CreateAccessProfileAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileAuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new UpdateAccessProfileAuthorizationCommand();
        var response = new UpdateAccessProfileAuthorizationResponse();
        var id = Guid.NewGuid();
        UpdateAccessProfileAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<UpdateAccessProfileAuthorizationCommand>(), token))
            .Callback<IRequest<UpdateAccessProfileAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<UpdateAccessProfileAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Update(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<UpdateAccessProfileAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileAuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new DeleteAccessProfileAuthorizationCommand();
        var response = new DeleteAccessProfileAuthorizationResponse();
        var id = Guid.NewGuid();
        DeleteAccessProfileAuthorizationCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<DeleteAccessProfileAuthorizationCommand>(), token))
            .Callback<IRequest<DeleteAccessProfileAuthorizationResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<DeleteAccessProfileAuthorizationCommand>(item)).ReturnsAsync(response);
        var result = await controller.Delete(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<DeleteAccessProfileAuthorizationCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileAuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAccessProfileAuthorizationByIdQuery();
        var response = new ReadAccessProfileAuthorizationByIdResponse();
        var id = Guid.NewGuid();
        ReadAccessProfileAuthorizationByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAccessProfileAuthorizationByIdQuery>(), token))
            .Callback<IRequest<ReadAccessProfileAuthorizationByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAccessProfileAuthorizationByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<ReadAccessProfileAuthorizationByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileAuthorizationController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAccessProfileAuthorizationByFilterQuery();
        var response = new ReadAccessProfileAuthorizationByFilterResponse();

        ReadAccessProfileAuthorizationByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAccessProfileAuthorizationByFilterQuery>(), token))
            .Callback<IRequest<ReadAccessProfileAuthorizationByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAccessProfileAuthorizationByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<ReadAccessProfileAuthorizationByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
