using Bogus;
using Mps.Ltmj.Api.Features.ExampleFeature.Commands;
using Mps.Ltmj.Api.Features.ExampleFeature.Queries;
using System;

namespace Mps.Ltmj.Tests.Mocks;

internal class ExampleFeatureFakerBuilder
{
    public Faker<InserirExampleCommand> InserirExampleCommand { get; set; }
    public Faker<InserirExampleCommandResponse> InserirExampleCommandResponse { get; set; }

    public Faker<AtualizarExampleCommand> AtualizarExampleCommand { get; set; }
    public Faker<AtualizarExampleCommandResponse> AtualizarExampleCommandResponse { get; set; }

    public Faker<RemoverExampleCommand> RemoverExampleCommand { get; set; }
    public Faker<RemoverExampleCommandResponse> RemoverExampleCommandResponse { get; set; }

    public Faker<AlternarExampleCommand> AlternarExampleCommand { get; set; }
    public Faker<AlternarExampleCommandResponse> AlternarExampleCommandResponse { get; set; }

    public Faker<SelecionarExampleByIdQuery> SelecionarExampleByIdQuery { get; set; }
    public Faker<SelecionarExampleByIdQueryResponse> SelecionarExampleByIdQueryResponse { get; set; }

    public Faker<SelecionarExampleByFilterQuery> SelecionarExampleByFilterQuery { get; set; }
    public Faker<SelecionarExampleByFilterQueryResponse> SelecionarExampleByFilterQueryResponse { get; set; }

    public ExampleFeatureFakerBuilder()
    {
        InserirExampleCommand =
            GetInserirExampleCommand();

        InserirExampleCommandResponse =
            GetInserirExampleCommandResponse();

        AtualizarExampleCommand =
            GetAtualizarExampleCommand();

        AtualizarExampleCommandResponse =
            GetAtualizarExampleCommandResponse();

        RemoverExampleCommand =
            GetRemoverExampleCommand();

        RemoverExampleCommandResponse =
            GetRemoverExampleCommandResponse();

        AlternarExampleCommand =
            GetAlternarExampleCommand();

        AlternarExampleCommandResponse =
            GetAlternarExampleCommandResponse();

        SelecionarExampleByIdQuery =
            GetSelecionarExampleByIdQuery();

        SelecionarExampleByIdQueryResponse =
            GetSelecionarExampleByIdQueryResponse();

        SelecionarExampleByFilterQuery =
            GetSelecionarExampleByFilterQuery();

        SelecionarExampleByFilterQueryResponse =
            GetSelecionarExampleByFilterQueryResponse();
    }

    public static Faker<InserirExampleCommand>
        GetInserirExampleCommand()
    {
        return new Faker<InserirExampleCommand>("pt_BR")
            .CustomInstantiator(faker => new InserirExampleCommand(faker.Random.Guid()))
            .RuleFor(item => item.Name, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool());
    }

    public static Faker<InserirExampleCommandResponse>
        GetInserirExampleCommandResponse()
    {
        return new Faker<InserirExampleCommandResponse>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid())
            .RuleFor(item => item.DataCadastro, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.UsuarioLogadoId, faker => faker.Random.Guid());
    }

    public static Faker<AtualizarExampleCommand>
        GetAtualizarExampleCommand()
    {
        return new Faker<AtualizarExampleCommand>("pt_BR")
            .CustomInstantiator(faker => new AtualizarExampleCommand(faker.Random.Guid()))
            .RuleFor(item => item.Id, faker => faker.Random.Guid())
            .RuleFor(item => item.Name, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool());
    }

    public static Faker<AtualizarExampleCommandResponse>
        GetAtualizarExampleCommandResponse()
    {
        return new Faker<AtualizarExampleCommandResponse>("pt_BR")
            .RuleFor(item => item.DataAtualizacao, faker => faker.Date.Past().ToUniversalTime());
    }

    public static Faker<RemoverExampleCommand>
        GetRemoverExampleCommand()
    {
        return new Faker<RemoverExampleCommand>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid());
    }

    public static Faker<RemoverExampleCommandResponse>
        GetRemoverExampleCommandResponse()
    {
        return new Faker<RemoverExampleCommandResponse>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid());
    }

    public static Faker<AlternarExampleCommand>
        GetAlternarExampleCommand()
    {
        return new Faker<AlternarExampleCommand>("pt_BR")
            .CustomInstantiator(faker => new AlternarExampleCommand(faker.Random.Guid()))
            .RuleFor(item => item.Id, faker => faker.Random.Guid())
            .RuleFor(item => item.Ativo, faker => faker.Random.Bool());
    }

    public static Faker<AlternarExampleCommandResponse>
        GetAlternarExampleCommandResponse()
    {
        return new Faker<AlternarExampleCommandResponse>("pt_BR")
            .RuleFor(item => item.DataAtualizacao, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.DataRemocao, faker => faker.Date.Past().ToUniversalTime());
    }

    public static Faker<SelecionarExampleByIdQuery>
        GetSelecionarExampleByIdQuery()
    {
        return new Faker<SelecionarExampleByIdQuery>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid());
    }

    public static Faker<SelecionarExampleByIdQueryResponse>
        GetSelecionarExampleByIdQueryResponse()
    {
        return new Faker<SelecionarExampleByIdQueryResponse>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid())
            .RuleFor(item => item.DataCadastro, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.DataAtualizacao, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.DataRemocao, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.UsuarioLogadoId, faker => faker.Random.Guid())
            .RuleFor(item => item.Name, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool());
    }

    public static Faker<SelecionarExampleByFilterQuery>
        GetSelecionarExampleByFilterQuery()
    {
        return new Faker<SelecionarExampleByFilterQuery>("pt_BR")
            .RuleFor(item => item.Name, faker => faker.Lorem.Word())
            .RuleFor(item => item.Description, faker => faker.Lorem.Word())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool());
    }

    public static Faker<SelecionarExampleByFilterQueryResponse>
        GetSelecionarExampleByFilterQueryResponse()
    {
        return new Faker<SelecionarExampleByFilterQueryResponse>("pt_BR")
            .RuleFor(item => item.Id, faker => faker.Random.Guid())
            .RuleFor(item => item.DataCadastro, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.DataAtualizacao, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.DataRemocao, faker => faker.Date.Past().ToUniversalTime())
            .RuleFor(item => item.UsuarioLogadoId, faker => faker.Random.Guid())
            .RuleFor(item => item.Name, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool());
    }
}
