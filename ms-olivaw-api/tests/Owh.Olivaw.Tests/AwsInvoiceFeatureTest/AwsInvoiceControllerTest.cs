using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Commands;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Commands;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.AwsInvoiceFeatureTest;
public sealed class AwsInvoiceControllerTest
{
    [Theory][InlineData(false)][InlineData(true)]public async Task Create_should_propagate_actor_token_and_conflict(bool conflict)
    {
        using var c=new CancellationTokenSource();var m=new Mock<IMediator>();var p=AwsInvoiceFakerBuilder.Payload();
        m.Setup(x=>x.Send(It.Is<CreateAwsInvoiceMediatorCommand>(v=>v.Payload==p && v.ActorUserId==InventoryFakerBuilder.Actor),c.Token)).ReturnsAsync(new CreateAwsInvoiceResult{Conflict=conflict?"Conflito":null});
        var context=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("sub",InventoryFakerBuilder.Actor.ToString())},"test"))};var controller=new AwsInvoiceController(m.Object,new CurrentActor(new HttpContextAccessor{HttpContext=context}));
        var result=await controller.Create(p,c.Token);if(conflict)Assert.IsType<ConflictObjectResult>(result);else Assert.Equal(201,Assert.IsType<ObjectResult>(result).StatusCode);m.VerifyAll();
    }
    [Fact]public async Task Queries_should_forward_public_id_and_filters()
    {
        var m=new Mock<IMediator>();var q=new ReadAwsInvoiceByFilterQuery();var id=Guid.NewGuid();
        m.Setup(x=>x.Send(It.Is<ReadAwsInvoiceByFilterMediatorQuery>(v=>v.Payload==q),It.IsAny<CancellationToken>())).ReturnsAsync(new ReadAwsInvoiceByFilterResponse());
        m.Setup(x=>x.Send(It.Is<ReadAwsInvoiceByIdMediatorQuery>(v=>v.Payload.ExternalId==id),It.IsAny<CancellationToken>())).ReturnsAsync(new ReadAwsInvoiceByIdResponse());
        var controller=new AwsInvoiceController(m.Object,new CurrentActor(new HttpContextAccessor()));Assert.IsType<OkObjectResult>(await controller.Read(q,CancellationToken.None));Assert.IsType<OkObjectResult>(await controller.ReadById(id,CancellationToken.None));m.VerifyAll();
    }
}
