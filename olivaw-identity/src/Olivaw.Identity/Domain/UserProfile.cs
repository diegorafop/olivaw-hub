using System;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class UserProfile : Entity
{
    public string Login { get; set; } = string.Empty;
    public string NormalizedLogin { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsServiceAccount { get; set; }
    public bool IsRoot { get; set; }
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
}
