using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Services;

public sealed class RootBootstrap(IIdentityStore store, IPasswordHasher<Usuario> hasher)
{
    public async Task EnsureAsync(IConfiguration configuration, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("Identity:Bootstrap:Enabled")) return;
        var existing = await store.FindIncludingRemovedAsync<Usuario>(item => item.LoginNormalizado == "ROOT", cancellationToken);
        if (existing is not null)
        {
            if (!existing.Root) throw new InvalidOperationException("O login root já está ocupado por um usuário comum.");
            return;
        }
        var password = configuration["Identity:Bootstrap:Password"] ?? string.Empty;
        var email = configuration["Identity:Bootstrap:Email"] ?? string.Empty;
        var validator = new InlineValidator<BootstrapInput>();
        validator.RuleFor(item => item.Password).NotEmpty().MaximumLength(128);
        validator.RuleFor(item => item.Email).NotEmpty().EmailAddress().MaximumLength(254);
        await validator.ValidateAndThrowAsync(new BootstrapInput(password, email), cancellationToken);
        var id = Guid.NewGuid();
        var usuario = new Usuario
        {
            IdExterno = id,
            UsuarioLogadoId = id,
            DataCadastro = DateTime.UtcNow,
            Login = "root",
            LoginNormalizado = "ROOT",
            Nome = "Root",
            Email = email,
            Root = true
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, password);
        store.Add(usuario);
        await store.SaveAsync(cancellationToken);
    }

    private sealed record BootstrapInput(string Password, string Email);
}
