using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalMessagesQuery { public Guid CustomerExternalId { get; set; } public Guid RequestExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalMessagesResponse { public List<PortalMessageItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalMessageItem { public Guid ExternalId { get; set; } public string Body { get; set; } = string.Empty; public string Author { get; set; } = string.Empty; public DateTime CreatedAt { get; set; } }
