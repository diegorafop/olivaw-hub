using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.FinancialFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Queries;
public sealed class ReadFinancialEntryByFilterMediatorQuery:IRequest<ReadFinancialEntryByFilterResponse>{public ReadFinancialEntryByFilterQuery Payload{get;set;}=new();}
public sealed class ReadFinancialEntryByFilterHandler(IRepository<FinancialEntry> entries):IRequestHandler<ReadFinancialEntryByFilterMediatorQuery,ReadFinancialEntryByFilterResponse>
{
 public async Task<ReadFinancialEntryByFilterResponse> Handle(ReadFinancialEntryByFilterMediatorQuery request,CancellationToken token){ValidationHelper.ThrowIfNull(request);var p=request.Payload;var rows=(await entries.TakeOrderedAsync(x=>x.DeletedAt==null && x.DueAt>=p.From && x.DueAt<p.To && (p.CustomerExternalId==null || x.Customer!.ExternalId==p.CustomerExternalId) && (p.Direction==null || x.Direction==p.Direction) && (p.Status==null || x.Status==p.Status) && (p.BeforeId==0 || x.Id<p.BeforeId),51,x=>x.Id,token,x=>x.Customer!,x=>x.Expense!)).ToList();return new(){Items=rows.Take(50).Select(x=>x.Map()).ToList(),NextBeforeId=rows.Count>50?rows[49].Id:null};}
}
public sealed class ReadFinancialEntryByFilterValidator:AbstractValidator<ReadFinancialEntryByFilterQuery>{public ReadFinancialEntryByFilterValidator(){RuleFor(x=>x.From).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000);RuleFor(x=>x.To).Must((p,v)=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v>p.From && (v-p.From).TotalDays<=366);RuleFor(x=>x.BeforeId).GreaterThanOrEqualTo(0);RuleFor(x=>x.CustomerExternalId).NotEqual(Guid.Empty);RuleFor(x=>x.Direction).Must(v=>v==null || v is "payable" or "receivable");RuleFor(x=>x.Status).Must(v=>v==null || v is "open" or "partial" or "settled" or "cancelled");}}
public sealed class ReadFinancialEntryByFilterMediatorValidator:AbstractValidator<ReadFinancialEntryByFilterMediatorQuery>{public ReadFinancialEntryByFilterMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadFinancialEntryByFilterValidator()).OverridePropertyName(string.Empty);}}
