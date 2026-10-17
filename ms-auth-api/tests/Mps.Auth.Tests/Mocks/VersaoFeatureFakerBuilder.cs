using Bogus;
using Mps.Auth.Api.Features.VersaoFeature.Queries;

namespace Mps.Auth.Tests.Mocks;

internal class VersaoFeatureFakerBuilder
{
    public Faker<SelecionarVersaoExternalQuery>
        SelecionarVersaoExternalQuery
    { get; set; }


    public Faker<SelecionarVersaoExternalQueryResponse>
        SelecionarVersaoExternalQueryResponse
    { get; set; }


    public VersaoFeatureFakerBuilder()
    {
        SelecionarVersaoExternalQueryResponse =
            GetSelecionarVersaoExternalQueryResponse();

        SelecionarVersaoExternalQuery =
            GetSelecionarVersaoExternalQuery();
    }

    public static Faker<SelecionarVersaoExternalQuery>
        GetSelecionarVersaoExternalQuery()
    {
        return new Faker<SelecionarVersaoExternalQuery>("pt_BR");
    }

    public static Faker<SelecionarVersaoExternalQueryResponse>
        GetSelecionarVersaoExternalQueryResponse()
    {
        return new Faker<SelecionarVersaoExternalQueryResponse>("pt_BR")
            .RuleFor(item => item.Versao, faker =>
            {
                var version = faker.System.Version();
                return $"{version.MajorRevision}.{version.Major}.{version.Minor}";
            })
            .RuleFor(item => item.Titulo, faker => faker.Lorem.Paragraph());
    }
}