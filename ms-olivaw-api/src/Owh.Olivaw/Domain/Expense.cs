using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class Expense : Entity
{
    public Guid EntryKey { get; private set; }
    public string Digest { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Kind { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal ExchangeRateToBrl { get; private set; }
    public decimal AmountBrl { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    private Expense() { }
    public static bool IsCategory(string value) => value is "infrastructure" or "tools" or "labor" or "other";
    public static Expense Create(Guid entryKey, string digest, string description, string category, string kind, string currency, decimal amount, decimal exchangeRate, DateTime occurredAt, string source, Customer? customer, Guid actor, DateTime now)
    {
        if (entryKey == Guid.Empty || digest.Length != 64 || actor == Guid.Empty) throw new ArgumentException("Identificação inválida.");
        if (string.IsNullOrWhiteSpace(description) || description.Length > 160 || string.IsNullOrWhiteSpace(source) || source.Length > 300) throw new ArgumentException("Descrição e origem são obrigatórias.");
        if (!IsCategory(category) || kind is not ("cost" or "credit") || currency is not ("BRL" or "USD")) throw new ArgumentException("Classificação inválida.");
        if (amount <= 0 || amount > 100000000 || decimal.Round(amount, 2) != amount || exchangeRate <= 0 || exchangeRate > 1000 || decimal.Round(exchangeRate, 6) != exchangeRate || (currency == "BRL" && exchangeRate != 1)) throw new ArgumentException("Valor ou conversão inválidos.");
        if (occurredAt.Kind != DateTimeKind.Utc || occurredAt.TimeOfDay != TimeSpan.Zero || occurredAt > now) throw new ArgumentException("Data deve ser um dia UTC já iniciado.");
        return new() { ExternalId = Guid.NewGuid(), EntryKey = entryKey, Digest = digest, Description = description, Category = category, Kind = kind, Currency = currency, Amount = amount, ExchangeRateToBrl = exchangeRate, AmountBrl = decimal.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero), OccurredAt = occurredAt, Source = source, Customer = customer, CustomerId = customer?.Id, ActorUserId = actor, CreatedAt = now };
    }
}
