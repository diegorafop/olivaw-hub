namespace Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;

public sealed class ReadSecurityReportByIdQuery
{
    public Guid ExternalId { get; set; }
}

public sealed class ReadSecurityReportByIdResponse
{
    public SecurityReportItem? Item { get; set; }
}
