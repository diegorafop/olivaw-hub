using System;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class Authorization : Entity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
