using System;
using System.Collections.Generic;

namespace Olivaw.Identity.Contracts;

public sealed class IdentityAccess
{
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsServiceAccount { get; set; }
    public bool IsRoot { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}
