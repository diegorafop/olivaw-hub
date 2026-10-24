using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.AccessProfileFeature;
using Olivaw.Identity.Api.Features.AccessProfileFeature.Commands;
using Olivaw.Identity.Api.Features.AccessProfileFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AccessProfileControllerTest
{
    [Fact]
    public async Task Create_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new CreateAccessProfileCommand();
        var response = new CreateAccessProfileResponse();

        CreateAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<CreateAccessProfileCommand>(), token))
            .Callback<IRequest<CreateAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<CreateAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Create(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<CreateAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new UpdateAccessProfileCommand();
        var response = new UpdateAccessProfileResponse();
        var id = Guid.NewGuid();
        UpdateAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<UpdateAccessProfileCommand>(), token))
            .Callback<IRequest<UpdateAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<UpdateAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Update(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<UpdateAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new DeleteAccessProfileCommand();
        var response = new DeleteAccessProfileResponse();
        var id = Guid.NewGuid();
        DeleteAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<DeleteAccessProfileCommand>(), token))
            .Callback<IRequest<DeleteAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<DeleteAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Delete(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<DeleteAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAccessProfileByIdQuery();
        var response = new ReadAccessProfileByIdResponse();
        var id = Guid.NewGuid();
        ReadAccessProfileByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAccessProfileByIdQuery>(), token))
            .Callback<IRequest<ReadAccessProfileByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAccessProfileByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<ReadAccessProfileByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadAccessProfileByFilterQuery();
        var response = new ReadAccessProfileByFilterResponse();

        ReadAccessProfileByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadAccessProfileByFilterQuery>(), token))
            .Callback<IRequest<ReadAccessProfileByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadAccessProfileByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<ReadAccessProfileByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
