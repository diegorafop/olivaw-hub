namespace Mps.OlivawCore.Domain;

public class Example : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Permission { get; set; } = true;
}