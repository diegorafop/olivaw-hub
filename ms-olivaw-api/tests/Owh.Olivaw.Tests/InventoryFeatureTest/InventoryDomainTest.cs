using System;
using System.Linq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest;
public sealed class InventoryDomainTest
{
    [Theory]
    [InlineData("shared", true)]
    [InlineData("individual", true)]
    [InlineData("platform", true)]
    [InlineData("customer", true)]
    [InlineData("all", false)]
    [InlineData("", false)]
    public void Resource_scope_should_accept_sharing_and_preserve_legacy_records(string scope, bool expected)
    {
        Assert.Equal(expected, InventoryResourceData.IsScope(scope));
        Assert.Equal("shared", new InventoryResourceData().Scope);
    }
    [Fact]
    public void Reachability_should_deduplicate_diamonds_and_stop_dependency_cycles()
    {
        var edges = new[] { Edge(1, 2), Edge(1, 3), Edge(2, 4), Edge(3, 4), Edge(4, 1) };
        var roots = new[] { new InventoryOrigin { ResourceId = 1, CustomerId = 10, Environment = "production" }, new InventoryOrigin { ResourceId = 3, CustomerId = 20, Environment = "staging" } };
        var result = InventoryReachability.Build(edges, roots);
        Assert.False(result.LimitExceeded);
        Assert.Equal(new[] {10, 20}, result.Customers[4].OrderBy(item => item));
        Assert.Equal(2, result.Environments[4].Count);
        Assert.True(InventoryReachability.Build(edges, roots, 1).LimitExceeded);
    }
    [Fact]
    public void Composition_should_reject_cycles_but_ignore_other_dependency_kinds()
    {
        var edge = Edge(1, 2); edge.Kind = "part-of";
        Assert.True(InventoryRelations.CreatesCompositionCycle(2, 1, new[] {edge}));
        edge.Kind = "depends-on";
        Assert.False(InventoryRelations.CreatesCompositionCycle(2, 1, new[] {edge}));
        Assert.True(InventoryRelations.IsCompatible("database", "database-instance", "part-of"));
        Assert.False(InventoryRelations.IsCompatible("database", "volume", "part-of"));
        Assert.False(InventoryRelations.IsCompatible("volume", "database", "runs-on"));
        Assert.True(InventoryRelations.IsCompatible("database-instance", "compute", "runs-on"));
    }
    [Fact]
    public void Resource_should_preserve_identity_snapshots_and_unknown_capacity()
    {
        var data = InventoryFakerBuilder.Data();
        Assert.Null(InventoryResource.IdentityKey(data));
        data.Provider = "AWS"; data.ProviderResourceId = "i-Example";
        var resource = InventoryResource.Create("vm-example", data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var before = Assert.Single(resource.Changes);
        var key = resource.ProviderIdentityKey;
        data.Provider = "aws"; data.Name = "Renomeado"; data.Status = "verified"; data.Source = "Conferência manual";
        var change = resource.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now.AddHours(1));
        Assert.Equal(key, resource.ProviderIdentityKey);
        Assert.Equal("Recurso exemplo", before.Name);
        Assert.Equal("Renomeado", change.Name);
        Assert.Equal(2, resource.Revision);
        Assert.Null(resource.MemoryCapacityGiB);
        Assert.Equal(InventoryFakerBuilder.Now.AddHours(1), resource.VerifiedAt);
        data.ProviderResourceId = "i-example";
        Assert.Throws<ArgumentException>(() => resource.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
    }
    [Theory]
    [InlineData("https://example.test/path", true)]
    [InlineData("", true)]
    [InlineData("https://user:secret@example.test", false)]
    [InlineData("https://example.test?token=secret", false)]
    [InlineData("file:///etc/passwd", false)]
    [InlineData("https://example.test#token", false)]
    public void Reference_should_accept_only_public_reference_shapes(string url, bool expected) => Assert.Equal(expected, InventoryResourceData.IsReferenceUrl(url));
    [Fact]
    public void Periods_should_preserve_authors_and_allow_new_period_after_closure()
    {
        var resource = InventoryFakerBuilder.Resource();
        var customer = ManagedServiceFakerBuilder.Customer();
        var link = ResourceCustomer.Create(resource, customer, "Domínio", InventoryFakerBuilder.Actor, "Início", InventoryFakerBuilder.Now);
        var closer = Guid.NewGuid();
        link.Close(closer, "Fim", InventoryFakerBuilder.Now.AddDays(1));
        link.Close(Guid.NewGuid(), "Outro", InventoryFakerBuilder.Now.AddDays(2));
        Assert.Equal("Início", link.StartedByName); Assert.Equal("Fim", link.EndedByName); Assert.Equal(closer, link.EndedByUserId); Assert.Equal(2, link.Revision);
        var next = ResourceCustomer.Create(resource, customer, "Nova vigência", InventoryFakerBuilder.Actor, "Início", InventoryFakerBuilder.Now.AddDays(2));
        Assert.NotEqual(link.ExternalId, next.ExternalId); Assert.Null(next.EndedAt);
    }
    [Fact]
    public void Deployment_should_preserve_period_and_reject_reactivation_or_negative_reservation()
    {
        var deployment = InventoryFakerBuilder.Deployment();
        Assert.Throws<ArgumentException>(() => DeploymentResource.Create(InventoryFakerBuilder.Resource(), deployment, "Uso", -1, null, null, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        deployment.Apply(deployment.Name, deployment.Location, deployment.Endpoint, "closed", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now.AddDays(1));
        Assert.NotNull(deployment.EndedAt);
        Assert.Throws<ArgumentException>(() => deployment.Apply("Nome", "", "", "active", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now.AddDays(2)));
        Assert.Throws<ArgumentException>(() => DeploymentResource.Create(InventoryFakerBuilder.Resource(), deployment, "Uso", null, null, null, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
    }
    private static InventoryEdge Edge(int from, int to) => new() { SourceId = from, TargetId = to, Kind = "depends-on" };
}
