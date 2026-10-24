using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.UserProfileFeature;
using Olivaw.Identity.Api.Features.UserProfileFeature.Commands;
using Olivaw.Identity.Api.Features.UserProfileFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UserProfileControllerTest
{
    [Fact]
    public async Task Create_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new CreateUserProfileCommand();
        var response = new CreateUserProfileResponse();

        CreateUserProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<CreateUserProfileCommand>(), token))
            .Callback<IRequest<CreateUserProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<CreateUserProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Create(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<CreateUserProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new UpdateUserProfileCommand();
        var response = new UpdateUserProfileResponse();
        var id = Guid.NewGuid();
        UpdateUserProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<UpdateUserProfileCommand>(), token))
            .Callback<IRequest<UpdateUserProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<UpdateUserProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Update(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<UpdateUserProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new DeleteUserProfileCommand();
        var response = new DeleteUserProfileResponse();
        var id = Guid.NewGuid();
        DeleteUserProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<DeleteUserProfileCommand>(), token))
            .Callback<IRequest<DeleteUserProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<DeleteUserProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Delete(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<DeleteUserProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadUserProfileByIdQuery();
        var response = new ReadUserProfileByIdResponse();
        var id = Guid.NewGuid();
        ReadUserProfileByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadUserProfileByIdQuery>(), token))
            .Callback<IRequest<ReadUserProfileByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadUserProfileByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<ReadUserProfileByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadUserProfileByFilterQuery();
        var response = new ReadUserProfileByFilterResponse();

        ReadUserProfileByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadUserProfileByFilterQuery>(), token))
            .Callback<IRequest<ReadUserProfileByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadUserProfileByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<ReadUserProfileByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
