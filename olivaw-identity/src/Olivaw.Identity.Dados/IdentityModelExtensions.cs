using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mps.OlivawCore.Domain;
using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Dados;

public static class IdentityModelExtensions
{
    public static ModelBuilder AddOlivawIdentity(this ModelBuilder model)
    {
        var usuario = Configure<Usuario>(model);
        usuario.Property(item => item.Login).HasMaxLength(100).IsRequired();
        usuario.Property(item => item.LoginNormalizado).HasMaxLength(100).IsRequired();
        usuario.HasIndex(item => item.LoginNormalizado).IsUnique();
        usuario.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        usuario.Property(item => item.Email).HasMaxLength(254).IsRequired();
        usuario.Property(item => item.SenhaHash).HasMaxLength(1024).IsRequired();
        usuario.Property(item => item.VersaoSeguranca).IsConcurrencyToken();

        var perfil = Configure<PerfilAcesso>(model);
        perfil.Property(item => item.Nome).HasMaxLength(100).IsRequired();
        perfil.Property(item => item.NomeNormalizado).HasMaxLength(100).IsRequired();
        perfil.Property(item => item.Descricao).HasMaxLength(500);
        perfil.HasIndex(item => item.NomeNormalizado).IsUnique();

        var autorizacao = Configure<Autorizacao>(model);
        autorizacao.Property(item => item.Chave).HasMaxLength(150).IsRequired();
        autorizacao.HasIndex(item => item.Chave).IsUnique();
        autorizacao.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        autorizacao.Property(item => item.Descricao).HasMaxLength(500);

        var usuarioPerfil = Configure<UsuarioPerfilAcesso>(model);
        usuarioPerfil.HasIndex(item => new { item.UsuarioId, item.PerfilAcessoId }).IsUnique();
        usuarioPerfil.HasOne(item => item.Usuario).WithMany().HasForeignKey(item => item.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        usuarioPerfil.HasOne(item => item.PerfilAcesso).WithMany().HasForeignKey(item => item.PerfilAcessoId).OnDelete(DeleteBehavior.Restrict);

        var perfilAutorizacao = Configure<PerfilAcessoAutorizacao>(model);
        perfilAutorizacao.HasIndex(item => new { item.PerfilAcessoId, item.AutorizacaoId }).IsUnique();
        perfilAutorizacao.HasOne(item => item.PerfilAcesso).WithMany().HasForeignKey(item => item.PerfilAcessoId).OnDelete(DeleteBehavior.Restrict);
        perfilAutorizacao.HasOne(item => item.Autorizacao).WithMany().HasForeignKey(item => item.AutorizacaoId).OnDelete(DeleteBehavior.Restrict);
        return model;
    }

    private static EntityTypeBuilder<T> Configure<T>(ModelBuilder model) where T : Entity
    {
        var entity = model.Entity<T>();
        entity.ToTable(typeof(T).Name.ToLowerInvariant());
        entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.IdExterno).IsUnique();
        entity.HasQueryFilter(item => item.DataRemocao == null);
        return entity;
    }
}
