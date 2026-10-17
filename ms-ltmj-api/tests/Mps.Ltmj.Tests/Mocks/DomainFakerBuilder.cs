using Bogus;
using Mps.OlivawCore.Domain;
using System;

namespace Mps.Ltmj.Tests.Mocks;

internal class DomainFakerBuilder
{
    private Random Random { get; set; }

    public Faker<Example> _Example { get; set; }

    public DomainFakerBuilder()
    {
        Random = new Random();
    }

    public virtual Faker<Example> Example
    {
        get
        {
            _Example ??= GetExample();

            return _Example;
        }
        set { _Example = value; }
    }

    public static Faker<Example> GetExample()
    {
        return new Faker<Example>("pt_BR")
            .RuleFor(item => item.Id, faker => faker
                    .Random
                    .Int(1, int.MaxValue))
                .RuleFor(item => item.IdExterno, faker => faker
                    .Random
                    .Guid())
                .RuleFor(item => item.UsuarioLogadoId, faker => faker
                    .Random
                    .Guid())
            .RuleFor(item => item.Name, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(item => item.Permission, faker => faker.Random.Bool())
            .RuleFor(item => item.DataCadastro, faker => faker
                .Date
                .Between(DateTime
                    .UtcNow
                    .AddMonths(-4), DateTime
                        .UtcNow
                        .AddMonths(-2)))
            .RuleFor(item => item.DataAtualizacao, faker => faker
                .Date
                .Between(DateTime
                    .UtcNow
                    .AddMonths(-2), DateTime
                        .UtcNow
                        .AddMonths(-1)))
            .RuleFor(item => item.DataRemocao, faker => faker
                .Date
                .Between(DateTime
                    .UtcNow
                    .AddMonths(-1), DateTime.UtcNow));
    }
}
