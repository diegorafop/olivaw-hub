using FluentValidation;
using MediatR;
using System.Security.Cryptography;
using System.Text.Json;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.ExpenseFeature.Commands;
namespace Owh.Olivaw.Api.Features.ExpenseFeature.Commands;
public sealed class CreateExpenseMediatorCommand : IRequest<CreateExpenseResult>
{
    public CreateExpenseCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
}
public sealed class CreateExpenseResult { public string? Conflict { get; set; } public CreateExpenseResponse Response { get; set; } = new(); }
public sealed class CreateExpenseHandler(IRepository<Expense> expenses, IRepository<Customer> customers) : IRequestHandler<CreateExpenseMediatorCommand, CreateExpenseResult>
{
    public async Task<CreateExpenseResult> Handle(CreateExpenseMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var digest = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(p)));
        var existing = await expenses.GetFirstOrDefaultAsync(item => item.EntryKey == p.EntryKey, token);
        if (existing != null) return existing.Digest != digest ? new() { Conflict = "A chave já foi usada para outra despesa." } : new() { Response = new() { ExternalId = existing.ExternalId } };
        var customer = p.CustomerExternalId.HasValue ? await InventoryRules.RequireAsync(customers, p.CustomerExternalId.Value, token) : null;
        var entity = Expense.Create(p.EntryKey, digest, p.Description, p.Category, p.Kind, p.Currency, p.Amount, p.ExchangeRateToBrl, p.OccurredAt, p.Source, customer, request.ActorUserId, DateTime.UtcNow);
        await expenses.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(expenses, token)) return new() { Conflict = "Envio concorrente. Confira a lista antes de repetir." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.EntryKey).NotEmpty(); RuleFor(x => x.Description).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(300); RuleFor(x => x.Category).Must(Expense.IsCategory);
        RuleFor(x => x.Kind).Must(v => v is "cost" or "credit"); RuleFor(x => x.Currency).Must(v => v is "BRL" or "USD");
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000).PrecisionScale(18, 2, true);
        RuleFor(x => x.ExchangeRateToBrl).GreaterThan(0).LessThanOrEqualTo(1000).PrecisionScale(18, 6, true);
        RuleFor(x => x.ExchangeRateToBrl).Equal(1).When(x => x.Currency == "BRL");
        RuleFor(x => x.OccurredAt).Must(v => v.Kind == DateTimeKind.Utc && v.TimeOfDay == TimeSpan.Zero && v <= DateTime.UtcNow);
        RuleFor(x => x.CustomerExternalId).NotEqual(Guid.Empty);
    }
}
public sealed class CreateExpenseMediatorValidator : AbstractValidator<CreateExpenseMediatorCommand>
{
    public CreateExpenseMediatorValidator() { RuleFor(x => x.ActorUserId).NotEmpty(); RuleFor(x => x.Payload).NotNull().SetValidator(new CreateExpenseValidator()).OverridePropertyName(string.Empty); }
}
