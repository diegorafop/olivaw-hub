namespace Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
public sealed class UpdateSecurityRemediationReviewCommand
{
    public bool ConfirmedStopped { get; set; }
    public string Note { get; set; } = string.Empty;
}
