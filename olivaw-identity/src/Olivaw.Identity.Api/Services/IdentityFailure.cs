namespace Olivaw.Identity.Api.Services;

public sealed class IdentityFailure(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
