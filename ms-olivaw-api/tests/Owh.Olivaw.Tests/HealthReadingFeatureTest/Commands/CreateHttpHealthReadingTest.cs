using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Api.Services;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest.Commands;
public sealed class CreateHttpHealthReadingTest
{
    [Fact]
    public async Task Handle_should_probe_only_explicitly_allowed_inventory_reference_and_deduplicate_retry()
    {
        var data = InventoryFakerBuilder.Data(); data.ReferenceUrl = "https://example.test/ready";
        var resource = InventoryResource.Create("http-check", data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); resource.Id = 1;
        using var transport = new ProbeHandler(); using var client = new HttpClient(transport);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Monitoring:AllowedUrls:0"] = data.ReferenceUrl }).Build();
        var repository = InventoryFakerBuilder.Repository<HealthReading>();
        var handler = new CreateHttpHealthReadingHandler(repository.Object, InventoryFakerBuilder.Repository(resource).Object, new PublicHealthProbe(client, config));
        var request = new CreateHttpHealthReadingMediatorCommand { ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { ResourceExternalId = resource.ExternalId, SampleKey = Guid.NewGuid() } };
        Assert.True(new CreateHttpHealthReadingMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, CancellationToken.None); Assert.Null(first.Conflict);
        Assert.Equal(first.Response.ExternalId, (await handler.Handle(request, CancellationToken.None)).Response.ExternalId); Assert.Equal(1, transport.Calls);
        var denied = new CreateHttpHealthReadingHandler(repository.Object, InventoryFakerBuilder.Repository(resource).Object, new PublicHealthProbe(client, new ConfigurationBuilder().Build()));
        Assert.NotNull((await denied.Handle(request, CancellationToken.None)).Conflict); Assert.Equal(1, transport.Calls);
        request.Payload.SampleKey = Guid.Empty; Assert.False(new CreateHttpHealthReadingValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    private sealed class ProbeHandler : HttpMessageHandler
    {
        internal int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) { Calls++; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)); }
    }
}
