using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature;
using Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Commands;
using Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UserProfileAccessProfileControllerTest
{
    [Fact]
    public async Task Create_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileAccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new CreateUserProfileAccessProfileCommand();
        var response = new CreateUserProfileAccessProfileResponse();

        CreateUserProfileAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<CreateUserProfileAccessProfileCommand>(), token))
            .Callback<IRequest<CreateUserProfileAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<CreateUserProfileAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Create(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<CreateUserProfileAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileAccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new UpdateUserProfileAccessProfileCommand();
        var response = new UpdateUserProfileAccessProfileResponse();
        var id = Guid.NewGuid();
        UpdateUserProfileAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<UpdateUserProfileAccessProfileCommand>(), token))
            .Callback<IRequest<UpdateUserProfileAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<UpdateUserProfileAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Update(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<UpdateUserProfileAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileAccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new DeleteUserProfileAccessProfileCommand();
        var response = new DeleteUserProfileAccessProfileResponse();
        var id = Guid.NewGuid();
        DeleteUserProfileAccessProfileCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<DeleteUserProfileAccessProfileCommand>(), token))
            .Callback<IRequest<DeleteUserProfileAccessProfileResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<DeleteUserProfileAccessProfileCommand>(item)).ReturnsAsync(response);
        var result = await controller.Delete(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<DeleteUserProfileAccessProfileCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileAccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadUserProfileAccessProfileByIdQuery();
        var response = new ReadUserProfileAccessProfileByIdResponse();
        var id = Guid.NewGuid();
        ReadUserProfileAccessProfileByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadUserProfileAccessProfileByIdQuery>(), token))
            .Callback<IRequest<ReadUserProfileAccessProfileByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadUserProfileAccessProfileByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<ReadUserProfileAccessProfileByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReadByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UserProfileAccessProfileController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new ReadUserProfileAccessProfileByFilterQuery();
        var response = new ReadUserProfileAccessProfileByFilterResponse();

        ReadUserProfileAccessProfileByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<ReadUserProfileAccessProfileByFilterQuery>(), token))
            .Callback<IRequest<ReadUserProfileAccessProfileByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<ReadUserProfileAccessProfileByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Read(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<ReadUserProfileAccessProfileByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
