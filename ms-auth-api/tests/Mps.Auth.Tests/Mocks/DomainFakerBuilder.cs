using Bogus;
using Mps.Auth.Domain;
using System;

namespace Mps.Auth.Tests.Mocks;

internal class DomainFakerBuilder
{
    private Random Random { get; set; }

    public Faker<Usuario> _Usuario { get; set; }

    public DomainFakerBuilder()
    {
        Random = new Random();
    }

    public virtual Faker<Usuario> Usuario
    {
        get
        {
            _Usuario ??= GetUsuario();

            return _Usuario;
        }
        set { _Usuario = value; }
    }

    public static Faker<Usuario> GetUsuario()
    {
        return new Faker<Usuario>("pt_BR")
            .RuleFor(item => item.Id, faker => faker
                    .Random
                    .Int(1, int.MaxValue))
                .RuleFor(item => item.IdExterno, faker => faker
                    .Random
                    .Guid())
                .RuleFor(item => item.UsuarioLogadoId, faker => faker
                    .Random
                    .Guid())
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