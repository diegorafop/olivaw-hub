namespace Owh.Olivaw.Client.Features.CustomerFeature.Queries;

public sealed class ReadCustomerSummaryQuery { }
public sealed class ReadCustomerSummaryResponse
{
    public long Total { get; set; }
    public long Onboarding { get; set; }
    public long Active { get; set; }
    public long Paused { get; set; }
    public long Closed { get; set; }
    public DateTimeOffset AsOf { get; set; }
}
