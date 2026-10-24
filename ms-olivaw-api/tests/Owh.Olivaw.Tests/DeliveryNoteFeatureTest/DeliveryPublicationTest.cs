using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Dados.Repositories;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.DeliveryNoteFeatureTest;
public sealed class DeliveryPublicationTest
{
    private static DeliveryNoteData Data(string status = "draft") => new() { Title = "Entrega", Summary = "Resumo público", InternalNote = "INTERNAL-SECRET", Status = status, ApprovalReference = "INTERNAL-REFERENCE", DeliveredAt = InventoryFakerBuilder.Now };
    [Fact]
    public void Publication_should_require_confirmation_and_preserve_previous_snapshots()
    {
        var customer = ManagedServiceFakerBuilder.Customer();
        Assert.Throws<ArgumentException>(() => DeliveryNote.Create(customer, null, null, Data("published"), InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        var note = DeliveryNote.Create(customer, null, null, Data(), InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var data = Data("published"); data.ApprovalReference = "";
        Assert.Throws<ArgumentException>(() => note.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        data.ApprovalReference = "Confirmação"; data.DeliveredAt = InventoryFakerBuilder.Now.AddDays(1);
        Assert.Throws<ArgumentException>(() => note.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        data.DeliveredAt = InventoryFakerBuilder.Now; var published = note.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); note.Changes.Add(published);
        data.Summary = "Correção do resumo"; note.Changes.Add(note.Apply(data, InventoryFakerBuilder.Actor, "Outro operador", InventoryFakerBuilder.Now.AddHours(1)));
        Assert.Equal("Resumo público", published.Summary); Assert.Equal(3, note.Revision); Assert.Equal(3, note.Changes.Count); Assert.Equal("draft", note.Changes.First().Status);
        data.Status = "withdrawn"; data.InternalNote = "";
        Assert.Throws<ArgumentException>(() => note.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
    }
    [Fact]
    public async Task Portal_should_scope_company_before_querying_and_never_return_private_fields()
    {
        var company = ManagedServiceFakerBuilder.Customer(); var other = ManagedServiceFakerBuilder.Customer(2); var user = Guid.NewGuid();
        var members = InventoryFakerBuilder.Repository(PortalMembership.Create(user, company, "viewer", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        var note = DeliveryNote.Create(company, null, null, Data(), InventoryFakerBuilder.Actor, "INTERNAL-STAFF", InventoryFakerBuilder.Now); note.Apply(Data("published"), InventoryFakerBuilder.Actor, "INTERNAL-STAFF", InventoryFakerBuilder.Now);
        var notes = new Mock<IRepositoryDeliveryNote>();
        notes.Setup(repo => repo.ReadPublishedAsync(It.Is<DeliveryTimelineFilter>(filter => filter.CustomerId == company.Id), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { note });
        var handler = new ReadPortalDeliveriesHandler(new PortalAccess(members.Object), InventoryFakerBuilder.Repository(company, other).Object, notes.Object);
        var query = new ReadPortalDeliveriesMediatorQuery { ActorUserId = user, Payload = new() { CustomerExternalId = company.ExternalId } };
        var response = await handler.Handle(query, CancellationToken.None);
        Assert.Single(response.Items); Assert.Equal("Resumo público", response.Items.Single().Summary); Assert.DoesNotContain("INTERNAL", JsonSerializer.Serialize(response));
        query.Payload.CustomerExternalId = other.ExternalId;
        await Assert.ThrowsAsync<IdentityFailure>(() => handler.Handle(query, CancellationToken.None));
        notes.Verify(repo => repo.ReadPublishedAsync(It.IsAny<DeliveryTimelineFilter>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Timeline_should_translate_scoping_publication_filters_and_cursor_to_sql(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused"); else options.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true");
        options.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(options.Options, new ConfigurationBuilder().AddInMemoryCollection(new[] { new System.Collections.Generic.KeyValuePair<string,string?>("DatabaseProvider", provider) }).Build());
        var sql = new RepositoryDeliveryNote(context).ReadPublishedRows(new() { CustomerId = 1, Category = "fix", BeforeAt = InventoryFakerBuilder.Now, BeforeId = 42, Now = InventoryFakerBuilder.Now }).ToQueryString();
        Assert.Contains("customerid", sql); Assert.Contains("published", sql); Assert.Contains("ORDER BY", sql); Assert.Contains("deliveredat", sql); Assert.Contains("42", sql);
        Assert.True(context.Model.FindEntityType(typeof(DeliveryNote))!.FindProperty("Revision")!.IsConcurrencyToken);
    }
}
