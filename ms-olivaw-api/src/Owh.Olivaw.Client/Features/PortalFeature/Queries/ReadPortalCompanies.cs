using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalCompaniesQuery { public int BeforeId { get; set; } }
public sealed class ReadPortalCompaniesResponse { public List<PortalCompanyItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalCompanyItem { public Guid ExternalId { get; set; } public string Business { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; }
