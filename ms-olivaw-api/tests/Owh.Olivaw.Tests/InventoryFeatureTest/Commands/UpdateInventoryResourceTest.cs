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
public sealed class UpdateInventoryResourceTest
{
    [Fact]
    public void Validator_should_validate_typed_details_and_public_field_names()
    {
        var payload = new UpdateInventoryResourceCommand { Name = "Backup", Type = "backup", Revision = 1 };
        var validator = new UpdateInventoryResourceValidator();
        Assert.True(validator.Validate(payload).IsValid);
        payload.BackupIntervalHours = 0;
        payload.BackupRetentionDays = 36501;
        Assert.Contains(validator.Validate(payload).Errors, item => item.PropertyName == "BackupIntervalHours");
        Assert.Contains(validator.Validate(payload).Errors, item => item.PropertyName == "BackupRetentionDays");
        payload.BackupIntervalHours = 24;
        payload.BackupRetentionDays = 30;
        Assert.True(validator.Validate(payload).IsValid);
        payload.Type = "domain";
        Assert.False(validator.Validate(payload).IsValid);
        payload.BackupIntervalHours = null;
        payload.BackupRetentionDays = null;
        payload.RenewalAt = InventoryResourceDetailsTest.Date();
        Assert.True(validator.Validate(payload).IsValid);
        payload.RenewalAt = payload.RenewalAt.Value.AddHours(1);
        Assert.Contains(validator.Validate(payload).Errors, item => item.PropertyName == "RenewalAt");
        payload.RenewalAt = null;
        payload.Type = "database";
        payload.TechnologyVersion = "18";
        Assert.Contains(validator.Validate(payload).Errors, item => item.PropertyName == "Technology");
        payload.Technology = "PostgreSQL";
        payload.LogicalName = "application";
        Assert.True(validator.Validate(payload).IsValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_update_tracked_resource_and_reject_stale_revision(bool stale)
    {
        var resource = InventoryFakerBuilder.Resource(); var repository = InventoryFakerBuilder.Repository(resource);
        var changes = InventoryFakerBuilder.Repository<InventoryResourceChange>(); var gate = new Mock<IRepositoryInventoryRevision>();
        var handler = new UpdateInventoryResourceHandler(repository.Object, changes.Object, gate.Object, InventoryFakerBuilder.Repository<ResourceCustomer>().Object, InventoryFakerBuilder.Repository<ResourceDependency>().Object, InventoryFakerBuilder.Repository<DeploymentResource>().Object);
        var request = new UpdateInventoryResourceMediatorCommand { ExternalId = resource.ExternalId, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { Revision = stale ? 9 : 1, Name = "Novo nome", Type = "compute", Technology = "Linux", TechnologyVersion = "6" } };
        var result = await handler.Handle(request, CancellationToken.None);
        Assert.Equal(stale, result.Conflict != null);
        Assert.Equal(stale ? 1 : 2, resource.Revision);
        if (!stale) { changes.Verify(item => item.AddAsync(It.Is<InventoryResourceChange>(change => change.Name == "Novo nome" && change.Technology == "Linux" && change.TechnologyVersion == "6"), It.IsAny<CancellationToken>()), Times.Once); }
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
        Assert.True(new UpdateInventoryResourceMediatorValidator().Validate(request).IsValid);
        request.Payload.Revision = 0; Assert.False(new UpdateInventoryResourceValidator().Validate(request.Payload).IsValid);
    }
    [Fact]
    public async Task Handle_should_require_closing_dependencies_before_retirement()
    {
        var resource = InventoryFakerBuilder.Resource(); var dependencies = InventoryFakerBuilder.Repository(ResourceDependency.Create(resource, InventoryFakerBuilder.Resource(2), "Uso", "depends-on", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        var repository = InventoryFakerBuilder.Repository(resource);
        var handler = new UpdateInventoryResourceHandler(repository.Object, InventoryFakerBuilder.Repository<InventoryResourceChange>().Object, Mock.Of<IRepositoryInventoryRevision>(), InventoryFakerBuilder.Repository<ResourceCustomer>().Object, dependencies.Object, InventoryFakerBuilder.Repository<DeploymentResource>().Object);
        var result = await handler.Handle(new() { ExternalId = resource.ExternalId, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { Revision = 1, Name = resource.Name, Type = resource.Type, Status = "retired" } }, CancellationToken.None);
        Assert.NotNull(result.Conflict); Assert.Equal("reported", resource.Status);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
