namespace Owh.Olivaw.Client.Features.CustomerFeature.Commands;

public sealed class CreateCustomerCommand
{
    public Guid? SourceLeadExternalId { get; set; }
    public string Business { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? Document { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<CustomerContactInput> Contacts { get; set; } = [];
}
public sealed class CustomerContactInput
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
}
public sealed class CreateCustomerResponse
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
}
