using System;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class AccessProfileAuthorization : Entity
{
    public int AccessProfileId { get; set; }
    public int AuthorizationId { get; set; }
    public AccessProfile? AccessProfile { get; set; }
    public Authorization? Authorization { get; set; }
}
