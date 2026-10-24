using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalMessageCommand { public Guid CustomerExternalId { get; set; } public Guid RequestExternalId { get; set; } public Guid MessageKey { get; set; } public string Body { get; set; } = string.Empty; public bool Reopen { get; set; } public long Revision { get; set; } }
public sealed class CreatePortalMessageResponse { public Guid ExternalId { get; set; } }
