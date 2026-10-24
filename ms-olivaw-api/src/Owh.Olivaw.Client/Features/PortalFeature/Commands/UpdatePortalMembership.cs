using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class UpdatePortalMembershipCommand { public long Revision { get; set; } }
public sealed class UpdatePortalMembershipResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
