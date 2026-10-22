namespace Owh.Olivaw.Client.Features.CustomerFeature.Commands;

public sealed class UpdateCustomerCommand
{
    public long Revision { get; set; }
    public string Business { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? Document { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "onboarding";
    public List<CustomerContactInput> Contacts { get; set; } = [];
}
public sealed class UpdateCustomerResponse
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
}
