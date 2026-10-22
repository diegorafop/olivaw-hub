using System;
using System.Security.Cryptography;
using System.Text;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;

namespace Owh.Olivaw.Tests.Mocks;

internal static class LeadFollowUpFakerBuilder
{
    internal static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    internal static readonly Guid Actor = Guid.Parse("00000000-0000-0000-0000-000000000017");
    internal static Lead Lead() => Domain.Lead.Create(Guid.NewGuid(), Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(new string('a', 64)))), 1, "{}", Now);
    internal static UpdateLeadFollowUpMediatorCommand Command(Lead lead) => new()
    {
        ExternalId = lead.ExternalId, ActorUserId = Actor, ActorName = "Operador",
        Payload = new UpdateLeadFollowUpCommand { Revision = lead.FollowUpRevision, Status = "conversation", Note = "Aguardando fotos." }
    };
}
