using Bogus;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using System;

namespace Mps.Auth.Tests.Mocks;

internal class AutorizacaoFeatureFakerBuilder
{
    public Faker<Autorizacao> Autorizacao { get; } = new Faker<Autorizacao>("pt_BR")
        .UseSeed(731)
        .RuleFor(item => item.Id, faker => faker.Random.Int(1, 1000))
        .RuleFor(item => item.IdExterno, faker => faker.Random.Guid())
        .RuleFor(item => item.Tipo, _ => "role")
        .RuleFor(item => item.Valor, _ => "PerfilAcesso")
        .RuleFor(item => item.Nome, faker => faker.Lorem.Word())
        .RuleFor(item => item.Descricao, faker => faker.Lorem.Sentence())
        .RuleFor(item => item.Perfil, _ => true)
        .RuleFor(item => item.DataCadastro, _ => new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc))
        .RuleFor(item => item.DataAtualizacao, _ => new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc))
        .RuleFor(item => item.DataRemocao, _ => new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc));

    public Faker<SelecionarAutorizacaoByIdQuery> SelecionarAutorizacaoByIdQuery { get; } =
        new Faker<SelecionarAutorizacaoByIdQuery>("pt_BR")
            .UseSeed(732)
            .RuleFor(item => item.Id, faker => faker.Random.Guid());
}
