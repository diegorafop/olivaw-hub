namespace Olivaw.Identity.Api.Security;

public sealed class IdentityOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int TokenMinutes { get; set; } = 30;
}
