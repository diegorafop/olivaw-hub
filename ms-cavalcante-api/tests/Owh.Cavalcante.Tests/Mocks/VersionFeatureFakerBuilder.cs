using Bogus;
using Owh.Cavalcante.Api.Features.VersionFeature.Queries;

namespace Owh.Cavalcante.Tests.Mocks;

internal class VersionFeatureFakerBuilder
{
    public Faker<ReadVersionQuery> ReadVersionQuery { get; set; }
    public Faker<ReadVersionQueryResponse> ReadVersionQueryResponse { get; set; }

    public VersionFeatureFakerBuilder()
    {
        ReadVersionQueryResponse = GetReadVersionQueryResponse();
        ReadVersionQuery = GetReadVersionQuery();
    }

    public static Faker<ReadVersionQuery> GetReadVersionQuery()
    {
        return new Faker<ReadVersionQuery>("pt_BR");
    }

    public static Faker<ReadVersionQueryResponse> GetReadVersionQueryResponse()
    {
        return new Faker<ReadVersionQueryResponse>("pt_BR")
            .RuleFor(item => item.Version, faker =>
            {
                var version = faker.System.Version();
                return $"{version.MajorRevision}.{version.Major}.{version.Minor}";
            })
            .RuleFor(item => item.Title, faker => faker.Lorem.Paragraph());
    }
}