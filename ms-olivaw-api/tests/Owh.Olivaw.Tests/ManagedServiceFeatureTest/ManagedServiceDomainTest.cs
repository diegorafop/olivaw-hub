using System;
using System.Linq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest;

public class ManagedServiceDomainTest
{
    [Theory]
    [InlineData("planned")]
    [InlineData("onboarding")]
    [InlineData("active")]
    [InlineData("closed")]
    public void Apply_should_record_free_status_changes_without_replacing_identity(string status)
    {
        var service = ManagedServiceFakerBuilder.Create();
        var first = Assert.Single(service.Changes);
        Assert.Equal(1, first.Revision);
        var change = service.Apply("Novo nome", "worker", "Descrição atualizada", "Equipe", "internal", status, ManagedServiceFakerBuilder.Actor, "Outro operador", ManagedServiceFakerBuilder.Now.AddMinutes(1));
        Assert.Equal(2, service.Revision);
        Assert.Equal("ms-example-api", service.TechnicalId);
        Assert.Equal(status, service.Status);
        Assert.Equal("active", change.PreviousStatus);
        Assert.Equal("Novo nome", change.Name);
        Assert.Equal("Outro operador", change.ActorName);
    }
    [Fact]
    public void Link_should_preserve_opening_and_closing_authors_and_allow_new_period()
    {
        var service = ManagedServiceFakerBuilder.Create("customer");
        var customer = ManagedServiceFakerBuilder.Customer();
        var link = ManagedServiceCustomer.Create(service, customer, ManagedServiceFakerBuilder.Actor, "Abertura", ManagedServiceFakerBuilder.Now);
        var secondActor = Guid.NewGuid();
        link.Close(secondActor, "Encerramento", ManagedServiceFakerBuilder.Now.AddDays(1));
        link.Close(Guid.NewGuid(), "Não deve mudar", ManagedServiceFakerBuilder.Now.AddDays(2));
        Assert.Equal(ManagedServiceFakerBuilder.Actor, link.StartedByUserId);
        Assert.Equal("Abertura", link.StartedByName);
        Assert.Equal(secondActor, link.EndedByUserId);
        Assert.Equal("Encerramento", link.EndedByName);
        Assert.Equal(ManagedServiceFakerBuilder.Now.AddDays(1), link.EndedAt);
        var reopened = ManagedServiceCustomer.Create(service, customer, secondActor, "Reabertura", ManagedServiceFakerBuilder.Now.AddDays(3));
        Assert.NotEqual(link.ExternalId, reopened.ExternalId);
        Assert.Null(reopened.EndedAt);
    }
    [Fact]
    public void Create_should_reject_invalid_identifier_and_link_for_internal_service()
    {
        Assert.Throws<ArgumentException>(() => ManagedService.Create("UPPER_case", "Nome", "api", "", "", "internal", "active", ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now));
        Assert.Throws<ArgumentException>(() => ManagedServiceCustomer.Create(ManagedServiceFakerBuilder.Create(), ManagedServiceFakerBuilder.Customer(), ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now));
    }
}
