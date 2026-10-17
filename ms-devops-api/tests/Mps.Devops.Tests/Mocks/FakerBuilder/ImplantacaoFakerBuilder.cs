using Bogus;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;

namespace Mps.Devops.Tests.Mocks;

internal static class ImplantacaoFakerBuilder
{
    public static CriarImplantacaoCommand Create()
    {
        return new Faker<CriarImplantacaoCommand>("pt_BR")
            .RuleFor(item => item.Folder, "olivaw-hub/ms-exemplo-api")
            .RuleFor(item => item.Branch, "develop")
            .RuleFor(item => item.Environment, "dev").Generate();
    }
}
