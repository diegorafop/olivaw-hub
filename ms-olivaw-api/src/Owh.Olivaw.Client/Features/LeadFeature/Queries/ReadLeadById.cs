using System;
using System.Collections.Generic;

using Owh.Olivaw.Client.Features.LeadFeature.Commands;

namespace Owh.Olivaw.Client.Features.LeadFeature.Queries;

public sealed class ReadLeadByIdQuery
{
    public Guid ExternalId { get; set; }
}
public sealed class ReadLeadByIdResponse
{
    public Guid ExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UpdateLeadCommand? Snapshot { get; set; }
    public bool ContentUnavailable { get; set; }
}
