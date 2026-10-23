using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Npgsql;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Internals;

internal static class InventoryRules
{
    internal static async Task<T> RequireAsync<T>(IRepository<T> repository, Guid id, CancellationToken token, params Expression<Func<T, object>>[] joins) where T : Entity
    {
        var entity = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == id && item.DeletedAt == null, token, joins);
        var validator = new InlineValidator<Guid>();
        validator.RuleFor(item => entity).NotNull().OverridePropertyName("ExternalId").WithMessage(MessageHelper.NotFoundFor<T, Guid>(item => item.ExternalId, id));
        await validator.ValidateAndThrowAsync(id, token);
        return entity!;
    }
    internal static bool IsConflict(DbUpdateException exception)
    {
        if (exception is DbUpdateConcurrencyException) return true;
        return exception.InnerException is PostgresException { SqlState: "23505" } or SqlException { Number: 2601 or 2627 };
    }
    internal static async Task<bool> SaveAsync<T>(IRepository<T> repository, CancellationToken token) where T : Entity
    {
        try { await repository.SaveChangesAsync(token); return true; }
        catch (DbUpdateException exception) when (IsConflict(exception)) { return false; }
    }
    internal static void RequireGraph(bool limitExceeded)
    {
        if (limitExceeded) throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Inventory", "O inventário excedeu o limite operacional desta consulta. Nenhum resultado parcial foi apresentado.") });
    }
    internal static InventoryResourceData ToDomain(Owh.Olivaw.Client.Features.InventoryFeature.Commands.CreateInventoryResourceCommand payload) => new()
    {
        Name = payload.Name,
        Type = payload.Type,
        Purpose = payload.Purpose,
        Provider = payload.Provider,
        AccountReference = payload.AccountReference,
        Region = payload.Region,
        ProviderResourceId = payload.ProviderResourceId,
        Owner = payload.Owner,
        Payer = payload.Payer,
        Operator = payload.Operator,
        Scope = payload.Scope,
        Status = payload.Status,
        Source = payload.Source,
        Monitoring = payload.Monitoring,
        ReferenceUrl = payload.ReferenceUrl,
        CpuCapacity = payload.CpuCapacity,
        MemoryCapacityGiB = payload.MemoryCapacityGiB,
        StorageCapacityGiB = payload.StorageCapacityGiB,
    };
    internal static InventoryResourceData ToDomain(Owh.Olivaw.Client.Features.InventoryFeature.Commands.UpdateInventoryResourceCommand payload) => new()
    {
        Name = payload.Name,
        Type = payload.Type,
        Purpose = payload.Purpose,
        Provider = payload.Provider,
        AccountReference = payload.AccountReference,
        Region = payload.Region,
        ProviderResourceId = payload.ProviderResourceId,
        Owner = payload.Owner,
        Payer = payload.Payer,
        Operator = payload.Operator,
        Scope = payload.Scope,
        Status = payload.Status,
        Source = payload.Source,
        Monitoring = payload.Monitoring,
        ReferenceUrl = payload.ReferenceUrl,
        CpuCapacity = payload.CpuCapacity,
        MemoryCapacityGiB = payload.MemoryCapacityGiB,
        StorageCapacityGiB = payload.StorageCapacityGiB,
    };
    internal static void Require(bool condition, string field, string message)
    {
        if (!condition) throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure(field, message) });
    }
}
