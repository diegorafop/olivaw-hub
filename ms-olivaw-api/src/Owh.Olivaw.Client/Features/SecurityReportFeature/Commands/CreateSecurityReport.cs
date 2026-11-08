namespace Owh.Olivaw.Client.Features.SecurityReportFeature.Commands;

public sealed class CreateSecurityReportCommand
{
    public string RawData { get; set; } = string.Empty;
}

public sealed class CreateSecurityReportResponse
{
    public Guid ExternalId { get; set; }
    public Guid RunId { get; set; }
    public bool AlreadyReceived { get; set; }
}
