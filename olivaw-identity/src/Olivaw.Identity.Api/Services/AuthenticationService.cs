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

public sealed class AuthenticationService(IIdentityStore store, IPasswordHasher<Usuario> hasher, Security.IdentityOptions options)
{
    private readonly string _dummyHash = hasher.HashPassword(new Usuario(), Guid.NewGuid().ToString());

    public async Task<AuthenticationResult> LoginAsync(string login, string senha, CancellationToken cancellationToken)
    {
        var normalized = login.Trim().ToUpperInvariant();
        var usuario = await store.FindAsync<Usuario>(item => item.LoginNormalizado == normalized, cancellationToken);
        if (usuario is null)
        {
            hasher.VerifyHashedPassword(new Usuario(), _dummyHash, senha);
            throw InvalidCredentials();
        }
        if (!usuario.Ativo) throw InvalidCredentials();
        if (usuario.BloqueadoAte > DateTime.UtcNow) throw InvalidCredentials();
        var verification = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (verification == PasswordVerificationResult.Failed)
        {
            usuario.FalhasLogin++;
            if (usuario.FalhasLogin >= 5)
            {
                usuario.BloqueadoAte = DateTime.UtcNow.AddMinutes(15);
                usuario.FalhasLogin = 0;
            }
            await store.SaveAsync(cancellationToken);
            throw InvalidCredentials();
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            usuario.SenhaHash = hasher.HashPassword(usuario, senha);
        usuario.FalhasLogin = 0;
        usuario.BloqueadoAte = null;
        await store.SaveAsync(cancellationToken);
        var expires = DateTime.UtcNow.AddMinutes(options.TokenMinutes);
        var claims = new[]
        {
            new Claim("sub", usuario.ExternalId.ToString()),
            new Claim("name", usuario.Nome),
            new Claim("stamp", usuario.VersaoSeguranca.ToString()),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, DateTime.UtcNow, expires, credentials);
        return new AuthenticationResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var usuario = await store.FindAsync<Usuario>(item => item.ExternalId == userId, cancellationToken)
            ?? throw InvalidCredentials();
        if (!usuario.Ativo) throw InvalidCredentials();
        if (hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, currentPassword) == PasswordVerificationResult.Failed)
            throw InvalidCredentials();
        usuario.SenhaHash = hasher.HashPassword(usuario, newPassword);
        usuario.VersaoSeguranca = Guid.NewGuid();
        usuario.UsuarioLogadoId = userId;
        usuario.UpdatedAt = DateTime.UtcNow;
        await store.SaveAsync(cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        var usuario = await store.FindAsync<Usuario>(item => item.ExternalId == userId, cancellationToken)
            ?? throw InvalidCredentials();
        usuario.VersaoSeguranca = Guid.NewGuid();
        usuario.UsuarioLogadoId = userId;
        usuario.UpdatedAt = DateTime.UtcNow;
        await store.SaveAsync(cancellationToken);
    }

    private static IdentityFailure InvalidCredentials()
    {
        return new IdentityFailure(401, "Credenciais inválidas.");
    }
}
