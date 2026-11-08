using System;

namespace Owh.OlivawCore.Requests;

public class AuthenticateUserCommand
{
    public string Login { get; set; }
    public string Password { get; set; }
}

public class AuthenticateUserCommandResponse
{
    public bool IsActive { get; set; }
    public Guid UserProfileId { get; set; }
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}