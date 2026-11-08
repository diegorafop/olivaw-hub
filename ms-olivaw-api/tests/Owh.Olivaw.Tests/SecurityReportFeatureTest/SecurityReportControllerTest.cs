using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.SecurityReportFeature;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityReportFeatureTest;
public sealed class SecurityReportControllerTest
{
    [Fact]
    public async Task Create_should_delegate_authenticated_actor_and_distinguish_replay()
    {
        var mediator = new Mock<IMediator>();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", SecurityReportFakerBuilder.Actor.ToString()) }, "test")) };
        var controller = new SecurityReportController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context }));
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(It.Is<CreateSecurityReportMediatorCommand>(request => request.ActorUserId == SecurityReportFakerBuilder.Actor), source.Token)).ReturnsAsync(new CreateSecurityReportResponse());
        var result = Assert.IsType<ObjectResult>(await controller.Create(SecurityReportFakerBuilder.Create(), source.Token));
        Assert.Equal(201, result.StatusCode);
        mediator.Setup(item => item.Send(It.IsAny<CreateSecurityReportMediatorCommand>(), source.Token)).ReturnsAsync(new CreateSecurityReportResponse { AlreadyReceived = true });
        Assert.IsType<OkObjectResult>(await controller.Create(SecurityReportFakerBuilder.Create(), source.Token));
    }
    [Fact]
    public async Task Read_should_delegate_list_and_return_missing_detail()
    {
        var mediator = new Mock<IMediator>();
        var controller = new SecurityReportController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        mediator.Setup(item => item.Send(It.IsAny<ReadSecurityReportByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadSecurityReportByFilterResponse());
        Assert.IsType<OkObjectResult>(await controller.Read(new(), CancellationToken.None));
        mediator.Setup(item => item.Send(It.IsAny<ReadSecurityReportByIdMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadSecurityReportByIdResponse());
        Assert.IsType<NotFoundResult>(await controller.ReadById(Guid.NewGuid(), CancellationToken.None));
    }
    [Theory]
    [InlineData("Create", "security.reports.write")]
    [InlineData("Read", "olivaw.root")]
    [InlineData("ReadById", "olivaw.root")]
    public void Routes_should_declare_specific_authorization(string method, string policy)
    {
        var attributes = typeof(SecurityReportController).GetMethod(method)!.GetCustomAttributes(true);
        Assert.Contains(attributes.OfType<AuthorizeAttribute>(), item => item.Policy == policy);
        Assert.DoesNotContain(attributes, item => item is AllowAnonymousAttribute);
    }
}
