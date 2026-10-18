using FluentValidation.TestHelper;
using Olivaw.Identity.Api.Features.AutenticacaoFeature.Commands;
using Olivaw.Identity.Api.Features.UsuarioFeature.Commands;
using Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;
using Xunit;

namespace Olivaw.Identity.Tests;

public class ValidatorTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void InserirUsuario_should_reject_empty_password(string password)
    {
        var result = new InserirUsuarioValidator().TestValidate(new InserirUsuarioCommand
        {
            Login = "joao",
            Nome = "João",
            Email = "joao@example.test",
            Senha = password
        });
        result.ShouldHaveValidationErrorFor(item => item.Senha);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("demo7* ")]
    public void Passwords_should_accept_nonempty_short_values(string password)
    {
        new InserirUsuarioValidator().TestValidate(new InserirUsuarioCommand
        {
            Login = "cliente", Nome = "Cliente", Email = "cliente@example.test", Senha = password
        }).ShouldNotHaveAnyValidationErrors();
        new AlterarSenhaValidator().TestValidate(new AlterarSenhaCommand
        {
            SenhaAtual = "anterior", NovaSenha = password
        }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Passwords_should_keep_maximum_length()
    {
        var password = new string('x', 129);
        new InserirUsuarioValidator().TestValidate(new InserirUsuarioCommand
        {
            Login = "cliente", Nome = "Cliente", Email = "cliente@example.test", Senha = password
        }).ShouldHaveValidationErrorFor(item => item.Senha);
        new AlterarSenhaValidator().TestValidate(new AlterarSenhaCommand
        {
            SenhaAtual = "anterior", NovaSenha = password
        }).ShouldHaveValidationErrorFor(item => item.NovaSenha);
    }

    [Fact]
    public void InserirUsuario_should_reject_invalid_email()
    {
        new InserirUsuarioValidator().TestValidate(new InserirUsuarioCommand
        {
            Login = "joao",
            Nome = "João",
            Email = "invalid",
            Senha = IdentityHost.Password
        }).ShouldHaveValidationErrorFor(item => item.Email);
    }

    [Theory]
    [InlineData("PEDIDOS.LER")]
    [InlineData("pedidos ler")]
    [InlineData("pedidos..ler")]
    public void Autorizacao_should_reject_unstable_permission_keys(string key)
    {
        new InserirAutorizacaoValidator().TestValidate(new InserirAutorizacaoCommand
        {
            Chave = key,
            Nome = "Ler pedidos"
        }).ShouldHaveValidationErrorFor(item => item.Chave);
    }

    [Fact]
    public void AlterarSenha_should_require_current_and_new_password()
    {
        var result = new AlterarSenhaValidator().TestValidate(new AlterarSenhaCommand());
        result.ShouldHaveValidationErrorFor(item => item.SenhaAtual);
        result.ShouldHaveValidationErrorFor(item => item.NovaSenha);
    }
}
