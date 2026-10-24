using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalMembershipByFilterQuery { public Guid? CustomerExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalMembershipByFilterResponse { public bool AuthorityConfigured { get; set; } public List<PortalMembershipItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalMembershipItem { public Guid ExternalId { get; set; } public Guid CustomerExternalId { get; set; } public string Business { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public bool IsActive { get; set; } public long Revision { get; set; } }
