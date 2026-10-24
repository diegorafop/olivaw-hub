using FluentValidation.TestHelper;
using Olivaw.Identity.Api.Features.AuthenticationFeature.Commands;
using Olivaw.Identity.Api.Features.UserProfileFeature.Commands;
using Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;
using Xunit;

namespace Olivaw.Identity.Tests;

public class ValidatorTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUserProfile_should_reject_empty_password(string password)
    {
        var result = new CreateUserProfileValidator().TestValidate(new CreateUserProfileCommand
        {
            Login = "joao",
            Name = "João",
            Email = "joao@example.test",
            Password = password
        });
        result.ShouldHaveValidationErrorFor(item => item.Password);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("demo7* ")]
    public void Passwords_should_accept_nonempty_short_values(string password)
    {
        new CreateUserProfileValidator().TestValidate(new CreateUserProfileCommand
        {
            Login = "cliente", Name = "Cliente", Email = "cliente@example.test", Password = password
        }).ShouldNotHaveAnyValidationErrors();
        new ChangePasswordValidator().TestValidate(new ChangePasswordCommand
        {
            CurrentPassword = "anterior", NewPassword = password
        }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Passwords_should_keep_maximum_length()
    {
        var password = new string('x', 129);
        new CreateUserProfileValidator().TestValidate(new CreateUserProfileCommand
        {
            Login = "cliente", Name = "Cliente", Email = "cliente@example.test", Password = password
        }).ShouldHaveValidationErrorFor(item => item.Password);
        new ChangePasswordValidator().TestValidate(new ChangePasswordCommand
        {
            CurrentPassword = "anterior", NewPassword = password
        }).ShouldHaveValidationErrorFor(item => item.NewPassword);
    }

    [Fact]
    public void CreateUserProfile_should_reject_invalid_email()
    {
        new CreateUserProfileValidator().TestValidate(new CreateUserProfileCommand
        {
            Login = "joao",
            Name = "João",
            Email = "invalid",
            Password = IdentityHost.Password
        }).ShouldHaveValidationErrorFor(item => item.Email);
    }

    [Theory]
    [InlineData("PEDIDOS.LER")]
    [InlineData("pedidos ler")]
    [InlineData("pedidos..ler")]
    public void Authorization_should_reject_unstable_permission_keys(string key)
    {
        new CreateAuthorizationValidator().TestValidate(new CreateAuthorizationCommand
        {
            Key = key,
            Name = "Ler pedidos"
        }).ShouldHaveValidationErrorFor(item => item.Key);
    }

    [Theory]
    [InlineData("devops.deploy")]
    [InlineData("devops.deploy:lisierp")]
    [InlineData("devops.deploy:olivaw/ms-ltmj-api")]
    public void Authorization_should_accept_scoped_permission_keys(string key)
    {
        new CreateAuthorizationValidator().TestValidate(new CreateAuthorizationCommand
        {
            Key = key,
            Name = "Deploy service"
        }).ShouldNotHaveValidationErrorFor(item => item.Key);
    }

    [Fact]
    public void ChangePassword_should_require_current_and_new_password()
    {
        var result = new ChangePasswordValidator().TestValidate(new ChangePasswordCommand());
        result.ShouldHaveValidationErrorFor(item => item.CurrentPassword);
        result.ShouldHaveValidationErrorFor(item => item.NewPassword);
    }
}
