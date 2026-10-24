using System;
namespace Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
public sealed class CreateHttpHealthReadingCommand { public Guid ResourceExternalId { get; set; } public Guid SampleKey { get; set; } }
public sealed class CreateHttpHealthReadingResponse { public Guid ExternalId { get; set; } }
