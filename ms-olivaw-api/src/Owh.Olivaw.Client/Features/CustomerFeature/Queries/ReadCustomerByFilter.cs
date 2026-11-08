namespace Owh.Olivaw.Client.Features.CustomerFeature.Queries;

public sealed class ReadCustomerByFilterQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
}
public sealed class ReadCustomerByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<CustomerListItem> Items { get; set; } = [];
}
public sealed class CustomerListItem
{
    public Guid ExternalId { get; set; }
    public string Business { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
