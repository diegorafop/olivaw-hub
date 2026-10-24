using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Owh.OlivawCore.Domain;
using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Dados;

public static class IdentityModelExtensions
{
    public static ModelBuilder AddOlivawIdentity(this ModelBuilder model)
    {
        var usuario = Configure<UserProfile>(model);
        usuario.Property(item => item.Login).HasMaxLength(100).IsRequired();
        usuario.Property(item => item.NormalizedLogin).HasMaxLength(100).IsRequired();
        usuario.HasIndex(item => item.NormalizedLogin).IsUnique();
        usuario.Property(item => item.Name).HasMaxLength(150).IsRequired();
        usuario.Property(item => item.Email).HasMaxLength(254).IsRequired();
        usuario.Property(item => item.PasswordHash).HasMaxLength(1024).IsRequired();
        usuario.Property(item => item.SecurityStamp).IsConcurrencyToken();

        var perfil = Configure<AccessProfile>(model);
        perfil.Property(item => item.Name).HasMaxLength(100).IsRequired();
        perfil.Property(item => item.NormalizedName).HasMaxLength(100).IsRequired();
        perfil.Property(item => item.Description).HasMaxLength(500);
        perfil.HasIndex(item => item.NormalizedName).IsUnique();

        var autorizacao = Configure<Authorization>(model);
        autorizacao.Property(item => item.Key).HasMaxLength(150).IsRequired();
        autorizacao.HasIndex(item => item.Key).IsUnique();
        autorizacao.Property(item => item.Name).HasMaxLength(150).IsRequired();
        autorizacao.Property(item => item.Description).HasMaxLength(500);

        var usuarioPerfil = Configure<UserProfileAccessProfile>(model);
        usuarioPerfil.HasIndex(item => new { item.UserProfileId, item.AccessProfileId }).IsUnique();
        usuarioPerfil.HasOne(item => item.UserProfile).WithMany().HasForeignKey(item => item.UserProfileId).OnDelete(DeleteBehavior.Restrict);
        usuarioPerfil.HasOne(item => item.AccessProfile).WithMany().HasForeignKey(item => item.AccessProfileId).OnDelete(DeleteBehavior.Restrict);

        var perfilAuthorization = Configure<AccessProfileAuthorization>(model);
        perfilAuthorization.HasIndex(item => new { item.AccessProfileId, item.AuthorizationId }).IsUnique();
        perfilAuthorization.HasOne(item => item.AccessProfile).WithMany().HasForeignKey(item => item.AccessProfileId).OnDelete(DeleteBehavior.Restrict);
        perfilAuthorization.HasOne(item => item.Authorization).WithMany().HasForeignKey(item => item.AuthorizationId).OnDelete(DeleteBehavior.Restrict);
        return model;
    }

    private static EntityTypeBuilder<T> Configure<T>(ModelBuilder model) where T : Entity
    {
        var entity = model.Entity<T>();
        entity.ToTable(typeof(T).Name.ToLowerInvariant());
        entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.HasQueryFilter(item => item.DeletedAt == null);
        return entity;
    }
}
