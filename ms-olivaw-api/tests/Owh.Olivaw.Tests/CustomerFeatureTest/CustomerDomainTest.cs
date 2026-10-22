using System;
using System.Linq;
using Owh.Olivaw.Domain;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest;

public class CustomerDomainTest
{
    [Theory]
    [InlineData("onboarding", "active", true)]
    [InlineData("active", "onboarding", false)]
    [InlineData("active", "paused", true)]
    [InlineData("paused", "active", true)]
    [InlineData("active", "closed", true)]
    [InlineData("closed", "active", false)]
    [InlineData("closed", "onboarding", true)]
    [InlineData("invalid", "active", false)]
    public void CanTransition_should_enforce_explicit_lifecycle(string current, string next, bool expected) => Assert.Equal(expected, Customer.CanTransition(current,next));
    [Fact]
    public void Apply_should_keep_creation_and_append_audited_status_history()
    {
        var now = new DateTime(2026,10,21,10,0,0,DateTimeKind.Utc);
        var actor = Guid.NewGuid();
        var customer = Customer.Create("Empresa", "", null, "", "", actor, "Operador", now, null);
        var id = customer.ExternalId;
        Assert.Null(customer.UpdatedAt);
        customer.Apply("Empresa", "Empresa Ltda", "12345678000199", "Operador", "Notas", "active", actor, "Operador", now.AddMinutes(1));
        Assert.Equal(id, customer.ExternalId);
        Assert.Equal(now, customer.CreatedAt);
        Assert.Equal(2, customer.Revision);
        Assert.Equal("onboarding", customer.Changes.Last().PreviousStatus);
        Assert.Equal("active", customer.Changes.Last().Status);
        Assert.Equal(actor, customer.Changes.Last().ActorUserId);
    }
    [Fact]
    public void Contact_should_require_name_and_at_least_one_channel()
    {
        Assert.Throws<ArgumentException>(() => CustomerContact.Create("Maria", "", "", "", Guid.NewGuid(),DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() => CustomerContact.Create("", "", "a@b.test", "", Guid.NewGuid(),DateTime.UtcNow));
    }
}
