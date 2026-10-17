using Microsoft.IdentityModel.Tokens;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Hostings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mps.Auth.Api.Helpers;

public static class CryptographyHelper
{
    public static string GerarTokenAtualizacao()
    {
        byte[] value = Encoding
            .UTF8
            .GetBytes(Guid.NewGuid().ToString());

        byte[] hashValue = SHA512.HashData(value);

        return Base64UrlEncoder.Encode(hashValue);
    }

    public static string EnhancedHashPassword
    (
        string? password
    )
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        return BCrypt
            .Net
            .BCrypt
            .EnhancedHashPassword(password);
    }

    public static bool EnhancedVerify
    (
        string password,
        string hash
    )
    {
        return BCrypt
            .Net
            .BCrypt
            .EnhancedVerify(password, hash);
    }

    public static string GetJwtSecurityToken
    (
        Usuario entity,
        IConfiguration configuration
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var secret = configuration.GetValueByKey<string>("authentication:secret");

        if (string.IsNullOrEmpty(secret))
            return string.Empty;

        var key = Encoding
            .ASCII
            .GetBytes(secret);

        var tokenDescriptor = GetSecurityTokenDescriptor
        (
            entity,
            configuration,
            key
        );

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static SecurityTokenDescriptor GetSecurityTokenDescriptor
    (
        Usuario entity,
        IConfiguration configuration,
        byte[] key
    )
    {
        return new SecurityTokenDescriptor
        {
            Subject = GetClaimsIdentity(entity, configuration),
            Expires = GetTimeout(configuration.GetValue<int>("authentication:timeout")),
            SigningCredentials = GetSigningCredentials(key)
        };
    }

    private static SigningCredentials GetSigningCredentials
    (
        byte[] key
    )
    {
        return new SigningCredentials
        (
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );
    }

    private static DateTime GetTimeout
    (
        int timeout
    )
    {
        return DateTime
            .UtcNow
            .AddMinutes(timeout);
    }

    private static ClaimsIdentity GetClaimsIdentity
    (
        Usuario entity,
        IConfiguration configuration
    )
    {
        return new ClaimsIdentity
        (
            GetClaims
            (
                entity,
                configuration
            )
        );
    }

    private static Claim[] GetClaims
    (
        Usuario entity,
        IConfiguration configuration
    )
    {
        List<Claim> claims = GetOpenIdClaims(entity, configuration);

        foreach (var item in entity.Autorizacoes ?? [])
            switch (item.Autorizacao.Tipo)
            {
                case "Role":
                    claims.Add(new Claim(ClaimTypes.Role, item.Autorizacao.Valor));
                    break;

                default:
                    claims.Add(new Claim(item.Autorizacao.Tipo, item.Autorizacao.Valor));
                    break;
            }

        IEnumerable<string> autorizacaoValorMany = GetManyAutorizacaoValor(entity);

        foreach (string item in autorizacaoValorMany.Distinct())
            claims.Add(new Claim(ClaimTypes.Role, item));

        return [.. claims];
    }

    private static IEnumerable<string> GetManyAutorizacaoValor
    (
        Usuario entity
    )
    {
        IEnumerable<string> autorizacaoValorMany = [];

        if (entity.Perfils != null)
            foreach (UsuarioPerfilAcesso perfil in entity.Perfils)
            {
                List<string> itens = GetAutorizacaoValor(perfil);
                autorizacaoValorMany = [.. autorizacaoValorMany, .. itens];
            }

        return autorizacaoValorMany;
    }

    private static List<string> GetAutorizacaoValor
    (
        UsuarioPerfilAcesso perfil
    )
    {
        return [.. perfil
             .PerfilAcesso
             .Autorizacaos
             .Select(item => item.Autorizacao)
             .Select(item => item.Valor)];
    }

    private static List<Claim> GetOpenIdClaims
    (
        Usuario entity,
        IConfiguration configuration
    )
    {
        return
        [
            new Claim
            (
                configuration.GetValueByKey("authentication:oidc:standard-claims:sub"),
                entity.IdExterno.ToString() ?? string.Empty
            ),
            new Claim
            (
                configuration.GetValueByKey("authentication:oidc:standard-claims:username"),
                entity.Login ?? string.Empty
            ),
            new Claim
            (
                configuration.GetValueByKey("authentication:oidc:standard-claims:name"),
                entity.Nome ?? string.Empty
            ),
            new Claim
            (
                configuration.GetValueByKey("authentication:oidc:standard-claims:email"),
                entity.Email ?? string.Empty
            )
        ];
    }
}