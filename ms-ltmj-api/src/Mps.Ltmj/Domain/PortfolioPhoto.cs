using Mps.OlivawCore.Domain;

namespace Mps.Ltmj.Domain;

public class PortfolioPhoto : Entity
{
    public int PortfolioEventId { get; set; }
    public PortfolioEvent? PortfolioEvent { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int Position { get; set; }
}
