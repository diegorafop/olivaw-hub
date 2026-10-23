using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Npgsql;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature.Internals;

internal static class ManagedServiceRules
{
    internal static bool IsConflict(DbUpdateException exception)
    {
        if (exception is DbUpdateConcurrencyException) return true;
        return exception.InnerException is PostgresException { SqlState: "23505" } or SqlException { Number: 2601 or 2627 };
    }
    internal static async Task<List<Customer>> ReadCustomersAsync(List<Guid> ids, IRepository<Customer> repository, CancellationToken token)
    {
        if (ids.Count == 0) return new();
        var customers = (await repository.GetAsync(item => ids.Contains(item.ExternalId) && item.DeletedAt == null, token)).ToList();
        var validator = new InlineValidator<List<Guid>>();
        validator.RuleFor(item => item).Must(_ => customers.Count == ids.Count).OverridePropertyName("CustomerExternalIds")
            .WithMessage("Um dos clientes não existe ou foi removido. Confira os vínculos.");
        await validator.ValidateAndThrowAsync(ids, token);
        return customers;
    }
    internal static void SynchronizeLinks(ManagedService service, IEnumerable<ManagedServiceCustomer> current, IEnumerable<Customer> selected, Guid actor, string actorName, DateTime now)
    {
        var desired = selected.ToDictionary(item => item.Id);
        foreach (var link in current)
        {
            if (desired.Remove(link.CustomerId)) continue;
            link.Close(actor, actorName, now);
        }
        foreach (var customer in desired.Values) service.Customers.Add(ManagedServiceCustomer.Create(service, customer, actor, actorName, now));
    }
}
