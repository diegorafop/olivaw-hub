using FluentValidation.TestHelper;
using Owh.Devops.Api.Features.DeploymentFeature.Commands;
using Owh.Devops.Tests.Mocks;
using Owh.OlivawCore.Api.Helpers;
using Xunit;

namespace Owh.Devops.Tests.DeploymentFeatureTest.Commands;

public class CreateDeploymentValidatorTest
{
    [Fact]
    public void Validate_should_accept_valid_request()
    {
        new CreateDeploymentCommandValidator().TestValidate(DeploymentFakerBuilder.Create()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Validate_should_reject_empty_fields_with_standard_messages(string? value)
    {
        var request = new CreateDeploymentCommand { Folder = value!, Branch = value! };
        var result = new CreateDeploymentCommandValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Folder)
            .WithErrorMessage(MessageHelper.EmptyFor<CreateDeploymentCommand, string>(item => item.Folder));
        result.ShouldHaveValidationErrorFor(item => item.Branch)
            .WithErrorMessage(MessageHelper.EmptyFor<CreateDeploymentCommand, string>(item => item.Branch));
    }

    [Theory]
    [InlineData("repo;id")] [InlineData("repo&&id")] [InlineData("repo|id")]
    [InlineData("../repo")] [InlineData("repo/../outro")] [InlineData("/etc")]
    [InlineData("repo\\outro")] [InlineData("$(id)")] [InlineData("repo\nid")]
    [InlineData("repo%2f..%2f")] [InlineData("repo//outro")] [InlineData("repo/.")]
    public void Validate_should_reject_unsafe_folder(string value)
    {
        var request = DeploymentFakerBuilder.Create(); request.Folder = value;
        new CreateDeploymentCommandValidator().TestValidate(request).ShouldHaveValidationErrorFor(item => item.Folder);
    }

    [Theory]
    [InlineData("main;id")] [InlineData("main&&id")] [InlineData("main|id")]
    [InlineData("../main")] [InlineData("feature/../main")] [InlineData("--help")]
    [InlineData("$(id)")] [InlineData("main\nid")] [InlineData("main@{1}")]
    [InlineData("main.lock")] [InlineData(".hidden")] [InlineData("HEAD")]
    [InlineData("feature//a")] [InlineData("feature/")] [InlineData("main.")]
    public void Validate_should_reject_unsafe_branch(string value)
    {
        var request = DeploymentFakerBuilder.Create(); request.Branch = value;
        new CreateDeploymentCommandValidator().TestValidate(request).ShouldHaveValidationErrorFor(item => item.Branch);
    }

    [Theory]
    [InlineData("feature/issue001-devops")] [InlineData("release/1.2.3")] [InlineData("master")]
    public void Validate_should_accept_gitflow_branches(string branch)
    {
        var request = DeploymentFakerBuilder.Create(); request.Branch = branch;
        new CreateDeploymentCommandValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData(" ")]
    [InlineData("production")] [InlineData("DEV")] [InlineData("dev;id")]
    [InlineData("dev&&id")] [InlineData("dev|id")] [InlineData("../prd")]
    public void Validate_should_reject_missing_or_unsafe_environment(string? environment)
    {
        var request = DeploymentFakerBuilder.Create(); request.Environment = environment!;
        new CreateDeploymentCommandValidator().TestValidate(request).ShouldHaveValidationErrorFor(item => item.Environment);
    }

    [Theory]
    [InlineData("dev")] [InlineData("qas")] [InlineData("stg")] [InlineData("rls")] [InlineData("prd")]
    public void Validate_should_accept_supported_environment(string environment)
    {
        var request = DeploymentFakerBuilder.Create(); request.Environment = environment;
        new CreateDeploymentCommandValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
