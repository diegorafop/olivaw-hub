using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.FinancialFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Queries;
public sealed class ReadFinancialEntryByIdMediatorQuery:IRequest<ReadFinancialEntryByIdResponse>{public ReadFinancialEntryByIdQuery Payload{get;set;}=new();}
public sealed class ReadFinancialEntryByIdHandler(IRepository<FinancialEntry> entries,IRepository<FinancialSettlement> settlements):IRequestHandler<ReadFinancialEntryByIdMediatorQuery,ReadFinancialEntryByIdResponse>
{
 public async Task<ReadFinancialEntryByIdResponse> Handle(ReadFinancialEntryByIdMediatorQuery request,CancellationToken token){ValidationHelper.ThrowIfNull(request);var entry=await InventoryRules.RequireAsync(entries,request.Payload.ExternalId,token,x=>x.Customer!,x=>x.Expense!);var rows=(await settlements.TakeOrderedAsync(x=>x.FinancialEntryId==entry.Id,501,x=>x.Id,token,x=>x.FinancialAccount)).ToArray();InventoryRules.RequireGraph(rows.Length>500);return new(){Entry=entry.Map(),Settlements=rows.Select(x=>new FinancialSettlementItem{ExternalId=x.ExternalId,AccountName=x.FinancialAccount.Name,Amount=x.Amount,IsRefund=x.IsRefund,PaidAt=x.PaidAt,Source=x.Source}).ToList()};}
}
public sealed class ReadFinancialEntryByIdValidator:AbstractValidator<ReadFinancialEntryByIdQuery>{public ReadFinancialEntryByIdValidator(){RuleFor(x=>x.ExternalId).NotEmpty();}}
public sealed class ReadFinancialEntryByIdMediatorValidator:AbstractValidator<ReadFinancialEntryByIdMediatorQuery>{public ReadFinancialEntryByIdMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadFinancialEntryByIdValidator()).OverridePropertyName(string.Empty);}}
