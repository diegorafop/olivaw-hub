using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportMessagesQuery { public Guid RequestExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadSupportMessagesResponse { public List<SupportMessageItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class SupportMessageItem { public Guid ExternalId { get; set; } public string Body { get; set; } = string.Empty; public bool IsPublic { get; set; } public bool IsStaff { get; set; } public string AuthorName { get; set; } = string.Empty; public DateTime CreatedAt { get; set; } }
