using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Mps.Ltmj.Api.Features.PortfolioFeature;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;
using Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest;

public class PortfolioControllerTest
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IAuthorizationService> _authorization = new();
    private readonly PortfolioController _controller;
    private readonly CancellationToken _token = new CancellationTokenSource().Token;

    public PortfolioControllerTest()
    {
        _controller = new PortfolioController(_mediator.Object, _authorization.Object) {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", Guid.NewGuid().ToString()) }, "test"));
    }

    [Fact]
    public async Task Public_should_delegate_query_and_disable_cache()
    {
        var request = new ReadEventsByFilterQuery(); var response = new ReadEventsByFilterResponse();
        _mediator.Setup(service => service.Send(request, _token)).ReturnsAsync(response);
        Assert.Same(response, Assert.IsType<OkObjectResult>(await _controller.Public(request, _token)).Value);
        Assert.Equal("no-store", _controller.Response.Headers.CacheControl);
        _mediator.Verify(service => service.Send(request, _token), Times.Once);
    }
    [Fact]
    public async Task Management_should_delegate_query()
    {
        var request = new ReadEventsByFilterQuery(); var response = new ReadEventsByFilterResponse();
        _mediator.Setup(service => service.Send(request, _token)).ReturnsAsync(response);
        Assert.Same(response, Assert.IsType<OkObjectResult>(await _controller.Management(request, _token)).Value);
        _mediator.Verify(service => service.Send(request, _token), Times.Once);
    }
    [Fact]
    public async Task Create_should_delegate_command()
    {
        var request = new CreateEventCommand(); var response = new CreateEventResponse();
        _mediator.Setup(service => service.Send(request, _token)).ReturnsAsync(response);
        Assert.Same(response, Assert.IsType<OkObjectResult>(await _controller.Create(request, _token)).Value);
        _mediator.Verify(service => service.Send(request, _token), Times.Once);
    }
    [Fact]
    public async Task Update_should_use_route_id()
    {
        var id = Guid.NewGuid(); var request = new UpdateEventCommand(); var response = new UpdateEventResponse();
        _mediator.Setup(service => service.Send(request, _token)).ReturnsAsync(response);
        Assert.Same(response, Assert.IsType<OkObjectResult>(await _controller.Update(id, request, _token)).Value);
        Assert.Equal(id, request.Id); _mediator.Verify(service => service.Send(request, _token), Times.Once);
    }
    [Fact]
    public async Task Visibility_should_use_route_id()
    {
        var id = Guid.NewGuid(); var request = new UpdateEventVisibilityCommand { IsVisible = false };
        var response = new UpdateEventVisibilityResponse();
        _mediator.Setup(service => service.Send(request, _token)).ReturnsAsync(response);
        Assert.Same(response, Assert.IsType<OkObjectResult>(await _controller.Visibility(id, request, _token)).Value);
        Assert.Equal(id, request.Id); _mediator.Verify(service => service.Send(request, _token), Times.Once);
    }
    [Fact]
    public async Task Delete_should_delegate_id_and_token()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(service => service.Send(It.Is<DeleteEventCommand>(item => item.Id == id), _token)).ReturnsAsync(new DeleteEventResponse());
        Assert.IsType<OkObjectResult>(await _controller.Delete(id, _token));
        _mediator.Verify(service => service.Send(It.Is<DeleteEventCommand>(item => item.Id == id), _token), Times.Once);
    }
    [Fact]
    public async Task Upload_should_delegate_all_files()
    {
        var id = Guid.NewGuid(); var files = new List<IFormFile>();
        _mediator.Setup(service => service.Send(It.Is<UploadEventPhotosCommand>(item => item.Id == id && item.Files == files), _token)).ReturnsAsync(new UploadEventPhotosResponse());
        Assert.IsType<OkObjectResult>(await _controller.Upload(id, files, _token));
        _mediator.Verify(service => service.Send(It.Is<UploadEventPhotosCommand>(item => item.Id == id && item.Files == files), _token), Times.Once);
    }
    [Fact]
    public async Task Photo_should_check_permission_and_return_stream_without_cache()
    {
        var id = Guid.NewGuid(); var photoId = Guid.NewGuid(); var content = new MemoryStream();
        _authorization.Setup(service => service.AuthorizeAsync(_controller.User, null, "portfolio.manage")).ReturnsAsync(AuthorizationResult.Failed());
        _mediator.Setup(service => service.Send(It.Is<ReadEventPhotoQuery>(item => item.EventId == id && item.PhotoId == photoId), _token))
            .ReturnsAsync(new ReadEventPhotoResponse { Content = content, ContentType = "image/jpeg" });
        var result = Assert.IsType<FileStreamResult>(await _controller.Photo(id, photoId, _token));
        Assert.Same(content, result.FileStream); Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal("no-store", _controller.Response.Headers.CacheControl);
        Assert.Equal("nosniff", _controller.Response.Headers["X-Content-Type-Options"]);
    }
}
