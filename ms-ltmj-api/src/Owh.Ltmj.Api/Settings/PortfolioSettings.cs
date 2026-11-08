namespace Owh.Ltmj.Api.Settings;

public sealed class PortfolioSettings
{
    public string StoragePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data", "portfolio");
}
