using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.PortalFeatureTest;
public sealed class PortalConversationIsolationTest
{
    [Fact]
    public async Task Public_thread_should_never_include_internal_notes_staff_identity_or_another_company()
    {
        var company = ManagedServiceFakerBuilder.Customer(); var other = ManagedServiceFakerBuilder.Customer(2); var user = Guid.NewGuid();
        var members = InventoryFakerBuilder.Repository(PortalMembership.Create(user, company, "requester", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        var request = SupportRequest.Create(company, new() { Title = "Meu pedido", Requester = "Cliente", Owner = "INTERNAL-OWNER", NextAction = "INTERNAL-ACTION", InternalNote = "INTERNAL-NOTE" }, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now, null); request.Id = 1;
        var foreign = SupportRequest.Create(other, new() { Title = "FOREIGN-TITLE", Requester = "Pessoa", Owner = "Operador", NextAction = "Avaliar" }, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now, null); foreign.Id = 2;
        var requests = InventoryFakerBuilder.Repository(request, foreign);
        var publicMessage = SupportMessage.Create(request, Guid.NewGuid(), "Resposta pública", true, true, InventoryFakerBuilder.Actor, "INTERNAL-STAFF-NAME", InventoryFakerBuilder.Now);
        var privateMessage = SupportMessage.Create(request, Guid.NewGuid(), "INTERNAL-CONVERSATION", false, true, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var foreignMessage = SupportMessage.Create(foreign, Guid.NewGuid(), "FOREIGN-CONVERSATION", true, false, Guid.NewGuid(), "Outra pessoa", InventoryFakerBuilder.Now);
        var messages = InventoryFakerBuilder.Repository(publicMessage, privateMessage, foreignMessage);
        var handler = new ReadPortalMessagesHandler(new PortalAccess(members.Object), InventoryFakerBuilder.Repository(company, other).Object, requests.Object, messages.Object);
        var query = new ReadPortalMessagesMediatorQuery { ActorUserId = user, Payload = new() { CustomerExternalId = company.ExternalId, RequestExternalId = request.ExternalId } };
        var response = await handler.Handle(query, CancellationToken.None); Assert.Single(response.Items); Assert.Equal("Equipe Olivaw", response.Items.Single().Author);
        var json = JsonSerializer.Serialize(response); Assert.Equal("Resposta pública", response.Items.Single().Body); Assert.DoesNotContain("INTERNAL", json); Assert.DoesNotContain("FOREIGN", json);
        query.Payload.RequestExternalId = foreign.ExternalId; await Assert.ThrowsAsync<IdentityFailure>(() => handler.Handle(query, CancellationToken.None));
        query.Payload.CustomerExternalId = other.ExternalId; await Assert.ThrowsAsync<IdentityFailure>(() => handler.Handle(query, CancellationToken.None));
    }
    [Fact]
    public void Invitation_should_expire_be_single_use_and_be_revocable_without_storing_plaintext_token()
    {
        var user = Guid.NewGuid(); var invite = PortalInvitation.Create(user, ManagedServiceFakerBuilder.Customer(), "person@example.test", "requester", new string('A', 64), InventoryFakerBuilder.Actor, InventoryFakerBuilder.Now, Guid.NewGuid());
        Assert.True(invite.CanAccept(user, InventoryFakerBuilder.Now)); Assert.False(invite.CanAccept(Guid.NewGuid(), InventoryFakerBuilder.Now)); Assert.False(invite.CanAccept(user, InventoryFakerBuilder.Now.AddHours(48)));
        invite.Accept(user, InventoryFakerBuilder.Now.AddMinutes(1)); Assert.False(invite.CanAccept(user, InventoryFakerBuilder.Now.AddMinutes(2)));
        Assert.Throws<ArgumentException>(() => invite.Accept(user, InventoryFakerBuilder.Now.AddMinutes(3)));
    }
}
