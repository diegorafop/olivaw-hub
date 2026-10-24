using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.InventoryFeature;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest;
public sealed class InventoryLinkControllerTest
{
    [Fact]
    public async Task Read_should_forward_filter_and_token()
    {
        using var cancellation = new CancellationTokenSource(); var payload = new ReadInventoryLinkByFilterQuery();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<ReadInventoryLinkByFilterMediatorQuery>(request => request.Payload == payload), cancellation.Token)).ReturnsAsync(new ReadInventoryLinkByFilterResponse());
        Assert.IsType<OkObjectResult>(await new InventoryLinkController(mediator.Object).ReadByFilter(payload, cancellation.Token)); mediator.VerifyAll();
    }
}
