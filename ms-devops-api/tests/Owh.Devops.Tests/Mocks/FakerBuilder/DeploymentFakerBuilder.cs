using Bogus;
using Owh.Devops.Api.Features.DeploymentFeature.Commands;

namespace Owh.Devops.Tests.Mocks;

internal static class DeploymentFakerBuilder
{
    public static CreateDeploymentCommand Create()
    {
        return new Faker<CreateDeploymentCommand>("pt_BR")
            .RuleFor(item => item.Folder, "olivaw-hub/ms-exemplo-api")
            .RuleFor(item => item.Branch, "develop")
            .RuleFor(item => item.Environment, "dev").Generate();
    }
}
