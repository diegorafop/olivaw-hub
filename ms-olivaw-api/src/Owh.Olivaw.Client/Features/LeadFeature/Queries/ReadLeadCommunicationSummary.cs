namespace Owh.Olivaw.Client.Features.LeadFeature.Queries;

public sealed class ReadLeadCommunicationSummaryResponse
{
    public int Inspected { get; set; }
    public bool Truncated { get; set; }
    public DateTime AsOf { get; set; }
    public Dictionary<string, int> Counts { get; set; } = new();
}
