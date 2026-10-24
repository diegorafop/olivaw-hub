using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.PlanFeature;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest;
public sealed class PlanControllerTest
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Commands_should_forward_actor_token_and_preserve_conflicts(bool conflict)
    {
        using var cancellation = new CancellationTokenSource(); var mediator = new Mock<IMediator>();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador") }, "test")) };
        var cost = new CreateCostReferenceCommand(); var study = new CreatePlanStudyCommand(); var plan = new CreateCustomerPlanCommand();
        mediator.Setup(item => item.Send(It.Is<CreateCostReferenceMediatorCommand>(request => request.Payload == cost && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateCostReferenceResult { Conflict = conflict ? "Mudou" : null });
        mediator.Setup(item => item.Send(It.Is<CreatePlanStudyMediatorCommand>(request => request.Payload == study && request.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new CreatePlanStudyResult { Conflict = conflict ? "Mudou" : null });
        mediator.Setup(item => item.Send(It.Is<CreateCustomerPlanMediatorCommand>(request => request.Payload == plan && request.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new CreateCustomerPlanResult { Conflict = conflict ? "Mudou" : null });
        var controller = new PlanController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        foreach (var response in new[] { await controller.CreateCost(cost, cancellation.Token), await controller.CreateStudy(study, cancellation.Token), await controller.CreateCondition(plan, cancellation.Token) })
        {
            if (conflict) Assert.IsType<ConflictObjectResult>(response); else Assert.Equal(201, Assert.IsType<ObjectResult>(response).StatusCode);
        }
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Queries_should_forward_filters_and_simulation_without_commands()
    {
        var mediator = new Mock<IMediator>();
        var costs = new ReadCostReferenceByFilterQuery(); var studies = new ReadPlanStudyByFilterQuery(); var conditions = new ReadCustomerPlanByFilterQuery(); var estimate = new ReadPlanEstimateQuery();
        mediator.Setup(item => item.Send(It.Is<ReadCostReferenceByFilterMediatorQuery>(request => request.Payload == costs), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadCostReferenceByFilterResponse());
        mediator.Setup(item => item.Send(It.Is<ReadPlanStudyByFilterMediatorQuery>(request => request.Payload == studies), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadPlanStudyByFilterResponse());
        mediator.Setup(item => item.Send(It.Is<ReadCustomerPlanByFilterMediatorQuery>(request => request.Payload == conditions), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadCustomerPlanByFilterResponse());
        mediator.Setup(item => item.Send(It.Is<ReadPlanEstimateMediatorQuery>(request => request.Payload == estimate), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadPlanEstimateResponse());
        var controller = new PlanController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.ReadCosts(costs, CancellationToken.None)); Assert.IsType<OkObjectResult>(await controller.ReadStudies(studies, CancellationToken.None)); Assert.IsType<OkObjectResult>(await controller.ReadConditions(conditions, CancellationToken.None)); Assert.IsType<OkObjectResult>(await controller.ReadEstimate(estimate, CancellationToken.None)); mediator.VerifyAll();
    }
}
