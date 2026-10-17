using Bogus;
using Mps.Ltmj.Api.Features.VersaoFeature.Queries;

namespace Mps.Ltmj.Tests.Mocks;

internal class VersaoFeatureFakerBuilder
{
    public Faker<SelecionarVersaoQuery> SelecionarVersaoQuery { get; set; }
    public Faker<SelecionarVersaoQueryResponse> SelecionarVersaoQueryResponse { get; set; }

    public VersaoFeatureFakerBuilder()
    {
        SelecionarVersaoQueryResponse = GetSelecionarVersaoQueryResponse();
        SelecionarVersaoQuery = GetSelecionarVersaoQuery();
    }

    public static Faker<SelecionarVersaoQuery> GetSelecionarVersaoQuery()
    {
        return new Faker<SelecionarVersaoQuery>("pt_BR");
    }

    public static Faker<SelecionarVersaoQueryResponse> GetSelecionarVersaoQueryResponse()
    {
        return new Faker<SelecionarVersaoQueryResponse>("pt_BR")
            .RuleFor(item => item.Versao, faker =>
            {
                var version = faker.System.Version();
                return $"{version.MajorRevision}.{version.Major}.{version.Minor}";
            })
            .RuleFor(item => item.Titulo, faker => faker.Lorem.Paragraph());
    }
}