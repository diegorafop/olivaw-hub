using System;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;

namespace Owh.Olivaw.Tests.Mocks;

internal static class LeadFeatureFakerBuilder
{
    internal static UpdateLeadMediatorCommand Create() => new()
    {
        ExternalId = Guid.Parse("ad2a101a-ddf4-432b-bfb7-308f5032be12"),
        WriteKey = new string('a', 64),
        Payload = new UpdateLeadCommand
        {
            SchemaVersion = 1, Revision = 1,
            StartedAt = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero),
            Entry = "studio", Stage = "submitted", Email = "lead@example.test",
            Project = new LeadProject { Source = "studio", Title = "Vender meus produtos", Choices = ["Categorias"] }
        }
    };
}
