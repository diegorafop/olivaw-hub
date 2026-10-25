namespace Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
public sealed class UpdateSecurityRemediationCommand
{
    public string Action { get; set; } = string.Empty;
    public Guid LeaseToken { get; set; }
    public string ResultJson { get; set; } = "{}";
}
public sealed class UpdateSecurityRemediationResponse
{
    public string Status { get; set; } = string.Empty;
    public bool Conflict { get; set; }
}
