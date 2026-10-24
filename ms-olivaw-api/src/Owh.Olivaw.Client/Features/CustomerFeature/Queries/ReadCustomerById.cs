using Owh.Olivaw.Client.Features.CustomerFeature.Commands;

namespace Owh.Olivaw.Client.Features.CustomerFeature.Queries;

public sealed class ReadCustomerByIdQuery
{
    public Guid ExternalId { get; set; }
}
public sealed class ReadCustomerByIdResponse
{
    public Guid ExternalId { get; set; }
    public Guid? SourceLeadExternalId { get; set; }
    public long Revision { get; set; }
    public string Business { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? Document { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CustomerContactInput> Contacts { get; set; } = [];
    public List<CustomerChangeItem> Changes { get; set; } = [];
}
public sealed class CustomerChangeItem
{
    public DateTime At { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
