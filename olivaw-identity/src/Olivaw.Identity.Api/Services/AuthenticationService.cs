using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Services;

public sealed record AuthenticationResult(string AccessToken, DateTime ExpiresAt);

public sealed class AuthenticationService(IIdentityStore store, IPasswordHasher<UserProfile> hasher, Security.IdentityOptions options)
{
    private readonly string _dummyHash = hasher.HashPassword(new UserProfile(), Guid.NewGuid().ToString());

    public async Task<AuthenticationResult> LoginAsync(string login, string senha, CancellationToken cancellationToken)
    {
        var normalized = login.Trim().ToUpperInvariant();
        var usuario = await store.FindAsync<UserProfile>(item => item.NormalizedLogin == normalized, cancellationToken);
        if (usuario is null)
        {
            hasher.VerifyHashedPassword(new UserProfile(), _dummyHash, senha);
            throw InvalidCredentials();
        }
        if (!usuario.IsActive) throw InvalidCredentials();
        if (usuario.LockedUntil > DateTime.UtcNow) throw InvalidCredentials();
        var verification = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, senha);
        if (verification == PasswordVerificationResult.Failed)
        {
            usuario.FailedLoginAttempts++;
            if (usuario.FailedLoginAttempts >= 5)
            {
                usuario.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                usuario.FailedLoginAttempts = 0;
            }
            await store.SaveAsync(cancellationToken);
            throw InvalidCredentials();
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            usuario.PasswordHash = hasher.HashPassword(usuario, senha);
        usuario.FailedLoginAttempts = 0;
        usuario.LockedUntil = null;
        await store.SaveAsync(cancellationToken);
        var expires = DateTime.UtcNow.AddMinutes(options.TokenMinutes);
        var claims = new List<Claim>
        {
            new Claim("sub", usuario.ExternalId.ToString()),
            new Claim("name", usuario.Name),
            new Claim("stamp", usuario.SecurityStamp.ToString()),
            new Claim("root", usuario.IsRoot.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
            new Claim("service", usuario.IsServiceAccount.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        var permissions = await store.PermissionsAsync(usuario.Id, cancellationToken);
        foreach (var permission in permissions) claims.Add(new Claim("permission", permission));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, DateTime.UtcNow, expires, credentials);
        return new AuthenticationResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var usuario = await store.FindAsync<UserProfile>(item => item.ExternalId == userId, cancellationToken)
            ?? throw InvalidCredentials();
        if (!usuario.IsActive) throw InvalidCredentials();
        if (hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            throw InvalidCredentials();
        usuario.PasswordHash = hasher.HashPassword(usuario, newPassword);
        usuario.SecurityStamp = Guid.NewGuid();
        usuario.ActorUserId = userId;
        usuario.UpdatedAt = DateTime.UtcNow;
        await store.SaveAsync(cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        var usuario = await store.FindAsync<UserProfile>(item => item.ExternalId == userId, cancellationToken)
            ?? throw InvalidCredentials();
        usuario.SecurityStamp = Guid.NewGuid();
        usuario.ActorUserId = userId;
        usuario.UpdatedAt = DateTime.UtcNow;
        await store.SaveAsync(cancellationToken);
    }

    private static IdentityFailure InvalidCredentials()
    {
        return new IdentityFailure(401, "Credenciais inválidas.");
    }
}
