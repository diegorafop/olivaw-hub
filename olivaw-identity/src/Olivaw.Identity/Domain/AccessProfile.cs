using System;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class AccessProfile : Entity
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
