using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.LeadFeature.Queries;

public sealed class ReadLeadSummaryQuery { }
public sealed class ReadLeadSummaryResponse
{
    public long Total { get; set; }
    public long Recent { get; set; }
    public DateTimeOffset Since { get; set; }
    public DateTimeOffset AsOf { get; set; }
}
