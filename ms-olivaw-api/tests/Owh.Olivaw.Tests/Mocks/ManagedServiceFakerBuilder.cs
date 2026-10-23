using System;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;

namespace Owh.Olivaw.Tests.Mocks;

internal static class ManagedServiceFakerBuilder
{
    internal static readonly Guid Actor = Guid.Parse("00000000-0000-0000-0000-000000000018");
    internal static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    internal static ManagedService Create(string scope = "internal") => ManagedService.Create("ms-example-api", "Aplicação exemplo", "api", "Descrição", "Equipe", scope, "active", Actor, "Operador", Now);
    internal static Customer Customer(int id = 1)
    {
        var customer = Domain.Customer.Create("Empresa " + id, "", null, "", "", Actor, "Operador", Now, null);
        customer.Id = id;
        return customer;
    }
    internal static CreateManagedServiceMediatorCommand Command() => new()
    {
        ActorUserId = Actor, ActorName = "Operador",
        Payload = new() { TechnicalId = "ms-example-api", Name = "Aplicação exemplo", Type = "api", Scope = "internal", Status = "planned" }
    };
}
