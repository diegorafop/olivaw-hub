using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Queries;
public sealed class ReadFinancialSummaryMediatorQuery:IRequest<ReadFinancialSummaryResponse>{public ReadFinancialSummaryQuery Payload{get;set;}=new();}
public sealed class ReadFinancialSummaryHandler(IRepository<FinancialAccount> accounts,IRepository<FinancialEntry> entries,IRepository<FinancialSettlement> settlements):IRequestHandler<ReadFinancialSummaryMediatorQuery,ReadFinancialSummaryResponse>
{
 public async Task<ReadFinancialSummaryResponse> Handle(ReadFinancialSummaryMediatorQuery request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;
  var accountRows=(await accounts.TakeOrderedAsync(x=>x.DeletedAt==null && x.OpenedAt<p.To,101,x=>x.Id,token)).ToArray();
  var due=(await entries.TakeOrderedAsync(x=>x.DeletedAt==null && x.DueAt>=p.From && x.DueAt<p.To && (!p.CustomerExternalId.HasValue || x.Customer!.ExternalId==p.CustomerExternalId),5001,x=>x.Id,token)).ToArray();
  var paid=(await settlements.TakeOrderedAsync(x=>x.PaidAt<p.To && (!p.CustomerExternalId.HasValue || x.FinancialEntry.Customer!.ExternalId==p.CustomerExternalId),10001,x=>x.Id,token,x=>x.FinancialEntry)).ToArray();
  if(accountRows.Length>100 || due.Length>5000 || paid.Length>10000)throw new IdentityFailure(422,"Consulta excede o limite operacional; nenhum saldo parcial foi apresentado.");
  decimal Amount(FinancialSettlement row)=>row.Amount*(row.IsRefund?-1:1);
  decimal Cash(FinancialSettlement row)=>Amount(row)*(row.FinancialEntry.Direction=="receivable"?1:-1);
  var period=paid.Where(x=>x.PaidAt>=p.From).ToArray();
  var alreadyPaid=paid.GroupBy(x=>x.FinancialEntryId).ToDictionary(x=>x.Key,x=>x.Sum(Amount));
  var active=due.Where(x=>x.Status!="cancelled" || x.UpdatedAt>=p.To).ToArray();
  var opening=paid.Where(x=>x.PaidAt<p.From).Sum(Cash)+(p.CustomerExternalId.HasValue?0:accountRows.Where(x=>x.OpenedAt<=p.From).Sum(x=>x.OpeningBalance));
  var startsInPeriod=p.CustomerExternalId.HasValue?0:accountRows.Where(x=>x.OpenedAt>p.From).Sum(x=>x.OpeningBalance);
  var result=new ReadFinancialSummaryResponse{HasAccounts=accountRows.Length>0,IsCustomerContribution=p.CustomerExternalId.HasValue,OpeningBalanceBrl=opening,ClosingBalanceBrl=opening+startsInPeriod+period.Sum(Cash),ReceivedBrl=period.Where(x=>x.FinancialEntry.Direction=="receivable").Sum(Amount),PaidBrl=period.Where(x=>x.FinancialEntry.Direction=="payable").Sum(Amount),ReceivableBrl=active.Where(x=>x.Direction=="receivable").Sum(x=>Math.Max(0,x.Amount-alreadyPaid.GetValueOrDefault(x.Id))),PayableBrl=active.Where(x=>x.Direction=="payable").Sum(x=>Math.Max(0,x.Amount-alreadyPaid.GetValueOrDefault(x.Id)))};
  for(var day=p.From;day<p.To;day=day.AddDays(1)){var own=period.Where(x=>x.PaidAt==day);result.Series.Add(new(){Date=day,ReceivedBrl=own.Where(x=>x.FinancialEntry.Direction=="receivable").Sum(Amount),PaidBrl=own.Where(x=>x.FinancialEntry.Direction=="payable").Sum(Amount)});}
  return result;
 }
}
public sealed class ReadFinancialSummaryValidator:AbstractValidator<ReadFinancialSummaryQuery>{public ReadFinancialSummaryValidator(){RuleFor(x=>x.From).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000);RuleFor(x=>x.To).Must((p,v)=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v>p.From && (v-p.From).TotalDays<=366);RuleFor(x=>x.CustomerExternalId).NotEqual(Guid.Empty);}}
public sealed class ReadFinancialSummaryMediatorValidator:AbstractValidator<ReadFinancialSummaryMediatorQuery>{public ReadFinancialSummaryMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadFinancialSummaryValidator()).OverridePropertyName(string.Empty);}}
