using System;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class UserProfileAccessProfile : Entity
{
    public int UserProfileId { get; set; }
    public int AccessProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }
    public AccessProfile? AccessProfile { get; set; }
}
