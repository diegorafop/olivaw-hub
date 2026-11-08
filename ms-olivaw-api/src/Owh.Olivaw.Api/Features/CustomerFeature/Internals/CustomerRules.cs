using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Internals;

internal static class CustomerRules
{
    internal static async Task RequireFoundAsync<T>(T? entity, Guid externalId, CancellationToken cancellationToken) where T : Owh.OlivawCore.Domain.Entity
    {
        var validator = new InlineValidator<Guid>();
        validator.RuleFor(item => entity).NotNull().OverridePropertyName("ExternalId").WithMessage(MessageHelper.NotFoundFor<T, Guid>(item => item.ExternalId, externalId));
        await validator.ValidateAndThrowAsync(externalId, cancellationToken);
    }
    internal static bool IsConflict(DbUpdateException exception) => exception is DbUpdateConcurrencyException
        || exception.InnerException is PostgresException { SqlState: "23505" } or SqlException { Number: 2601 or 2627 };

    internal static CustomerContact Contact(CustomerContactInput input, Guid actor, DateTime now) => CustomerContact.Create(input.Name, input.Role, input.Email, input.Whatsapp, actor, now);
}
public sealed class CustomerContactValidator : AbstractValidator<CustomerContactInput>
{
    public CustomerContactValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Role).NotNull().MaximumLength(120);
        RuleFor(item => item.Email).NotNull().MaximumLength(254);
        RuleFor(item => item.Whatsapp).NotNull();
        RuleFor(item => item.Email).EmailAddress().When(item => !string.IsNullOrEmpty(item.Email));
        RuleFor(item => item.Whatsapp).Matches("^[1-9][0-9]9[0-9]{8}$").When(item => !string.IsNullOrEmpty(item.Whatsapp));
        RuleFor(item => item).Must(item => !string.IsNullOrWhiteSpace(item.Email) || !string.IsNullOrWhiteSpace(item.Whatsapp)).WithMessage("Informe e-mail ou WhatsApp com DDD para o contato.");
    }
}
