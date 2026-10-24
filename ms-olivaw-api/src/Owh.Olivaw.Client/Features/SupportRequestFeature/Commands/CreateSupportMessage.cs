using System;
namespace Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
public sealed class CreateSupportMessageCommand { public Guid RequestExternalId { get; set; } public Guid MessageKey { get; set; } public string Body { get; set; } = string.Empty; public bool IsPublic { get; set; } }
public sealed class CreateSupportMessageResponse { public Guid ExternalId { get; set; } }
