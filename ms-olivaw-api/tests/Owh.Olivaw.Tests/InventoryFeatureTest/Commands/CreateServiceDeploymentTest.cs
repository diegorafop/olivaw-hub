using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest.Commands;
public sealed class CreateServiceDeploymentTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_preserve_deployment_periods_and_detect_write_conflicts(bool conflict)
    {
        var deployment = InventoryFakerBuilder.Deployment();
        var gate = new Mock<IRepositoryInventoryRevision>();
        var repository = InventoryFakerBuilder.Repository<ServiceDeployment>();
        var use = DeploymentResource.Create(InventoryFakerBuilder.Resource(), deployment, "Uso", 1, 2, 3, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var uses = InventoryFakerBuilder.Repository(use);
        if (conflict) repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        var request = new CreateServiceDeploymentMediatorCommand { Payload = new() { ServiceExternalId = deployment.ManagedService.ExternalId, Name = "API", Environment = "staging" }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        var handler = new CreateServiceDeploymentHandler(repository.Object, InventoryFakerBuilder.Repository(deployment.ManagedService).Object, gate.Object);
        var result = await handler.Handle(request, CancellationToken.None);
        Assert.Equal(conflict, result.Conflict != null);
        if (!conflict) Assert.Equal(1, result.Response.Revision);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
        Assert.True(new CreateServiceDeploymentMediatorValidator().Validate(request).IsValid);
        request.Payload.Endpoint = "https://example.test?password=test"; Assert.False(new CreateServiceDeploymentValidator().Validate(request.Payload).IsValid);
    }
}
