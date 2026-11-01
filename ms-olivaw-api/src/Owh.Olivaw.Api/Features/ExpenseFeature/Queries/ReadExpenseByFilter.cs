using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Client.Features.ExpenseFeature.Queries;
namespace Owh.Olivaw.Api.Features.ExpenseFeature.Queries;
public sealed class ReadExpenseByFilterMediatorQuery : IRequest<ReadExpenseByFilterResponse> { public ReadExpenseByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadExpenseByFilterHandler(IRepository<Expense> expenses) : IRequestHandler<ReadExpenseByFilterMediatorQuery, ReadExpenseByFilterResponse>
{
    public async Task<ReadExpenseByFilterResponse> Handle(ReadExpenseByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        // Consulta limitada ao intervalo solicitado; não devolver totais parciais silenciosamente.
        var rows = (await expenses.TakeOrderedAsync(x => x.DeletedAt == null && (p.IncludeCredits || x.Kind == "cost") && x.OccurredAt >= p.From && x.OccurredAt < p.To, 5001, x => x.OccurredAt, token, x => x.Customer!)).ToList();
        if (rows.Count > 5000) throw new IdentityFailure(422, "Selecione um período menor; há mais de 5.000 lançamentos.");
        var shared = rows.Where(x => x.CustomerId == null).Sum(x => x.Kind == "credit" ? -x.AmountBrl : x.AmountBrl);
        if (p.UnallocatedOnly) rows = rows.Where(x => x.CustomerId == null).ToList();
        if (p.CustomerExternalId.HasValue) rows = rows.Where(x => x.Customer?.ExternalId == p.CustomerExternalId).ToList();
        DateTime Bucket(DateTime date) => p.GroupBy == "year" ? new(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc) : p.GroupBy == "month" ? new(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc) : date.Date;
        var response = new ReadExpenseByFilterResponse { GrossBrl = rows.Where(x => x.Kind == "cost").Sum(x => x.AmountBrl), CreditsBrl = rows.Where(x => x.Kind == "credit").Sum(x => x.AmountBrl), UnallocatedBrl = shared, Total = rows.Count };
        response.NetBrl = response.GrossBrl - response.CreditsBrl;
        var groups = rows.GroupBy(x => Bucket(x.OccurredAt)).ToDictionary(g => g.Key, g => new ExpensePoint { Date = g.Key, CostBrl = g.Where(x => x.Kind == "cost").Sum(x => x.AmountBrl), CreditsBrl = g.Where(x => x.Kind == "credit").Sum(x => x.AmountBrl), NetBrl = g.Sum(x => x.Kind == "credit" ? -x.AmountBrl : x.AmountBrl) });
        for (var date = Bucket(p.From); date < p.To; date = p.GroupBy == "year" ? date.AddYears(1) : p.GroupBy == "month" ? date.AddMonths(1) : date.AddDays(1)) response.Series.Add(groups.GetValueOrDefault(date) ?? new ExpensePoint { Date = date });
        ExpenseBreakdown Breakdown(string key, string label, IEnumerable<Expense> entries)
        {
            var group = entries.ToArray();
            var gross = group.Where(x => x.Kind == "cost").Sum(x => x.AmountBrl);
            var credit = group.Where(x => x.Kind == "credit").Sum(x => x.AmountBrl);
            return new() { Key = key, Label = label, GrossBrl = gross, CreditsBrl = credit, NetBrl = gross - credit, Entries = group.Length };
        }
        response.ByCategory = rows.GroupBy(x => x.Category).Select(g => Breakdown(g.Key, g.Key, g)).OrderByDescending(x => x.GrossBrl).ThenBy(x => x.Key).ToList();
        response.ByCustomer = rows.GroupBy(x => x.Customer?.ExternalId.ToString() ?? "shared").Select(g => Breakdown(g.Key, g.First().Customer?.Business ?? "Compartilhada / sem rateio", g)).OrderByDescending(x => x.GrossBrl).ThenBy(x => x.Key).ToList();
        response.Items = rows.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id).Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).Select(x => new ExpenseItem { ExternalId = x.ExternalId, Description = x.Description, Category = x.Category, Kind = x.Kind, Amount = x.Amount, Currency = x.Currency, ExchangeRateToBrl = x.ExchangeRateToBrl, AmountBrl = x.AmountBrl, OccurredAt = x.OccurredAt, Source = x.Source, CustomerName = x.Customer?.Business ?? "Compartilhada / sem rateio" }).ToList();
        return response;
    }
}
public sealed class ReadExpenseByFilterValidator : AbstractValidator<ReadExpenseByFilterQuery>
{
    public ReadExpenseByFilterValidator()
    {
        RuleFor(x => x.From).Must(v => v.Kind == DateTimeKind.Utc && v.TimeOfDay == TimeSpan.Zero && v.Year >= 2000);
        RuleFor(x => x.To).Must((p, v) => v.Kind == DateTimeKind.Utc && v.TimeOfDay == TimeSpan.Zero && v > p.From && (v - p.From).TotalDays <= 366);
        RuleFor(x => x.GroupBy).Must(v => v is "day" or "month" or "year"); RuleFor(x => x.CustomerExternalId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerExternalId).Null().When(x => x.UnallocatedOnly);
        RuleFor(x => x.Page).InclusiveBetween(1, 5000); RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadExpenseByFilterMediatorValidator : AbstractValidator<ReadExpenseByFilterMediatorQuery> { public ReadExpenseByFilterMediatorValidator() { RuleFor(x => x.Payload).NotNull().SetValidator(new ReadExpenseByFilterValidator()).OverridePropertyName(string.Empty); } }
