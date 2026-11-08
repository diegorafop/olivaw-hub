using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Commands;
public sealed class CreateFinancialSettlementMediatorCommand:IRequest<CreateFinancialSettlementResult>{public CreateFinancialSettlementCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}}
public sealed class CreateFinancialSettlementResult{public string? Conflict{get;set;}public CreateFinancialSettlementResponse Response{get;set;}=new();}
public sealed class CreateFinancialSettlementHandler(IRepository<FinancialSettlement> settlements,IRepository<FinancialEntry> entries,IRepository<FinancialAccount> accounts):IRequestHandler<CreateFinancialSettlementMediatorCommand,CreateFinancialSettlementResult>
{
 public async Task<CreateFinancialSettlementResult> Handle(CreateFinancialSettlementMediatorCommand request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;
  var existing=await settlements.GetFirstOrDefaultAsync(x=>x.SettlementKey==p.SettlementKey,token,x=>x.FinancialEntry,x=>x.FinancialAccount);
  if(existing!=null)return existing.FinancialEntry.ExternalId==p.EntryExternalId && existing.FinancialAccount.ExternalId==p.AccountExternalId && existing.Amount==p.Amount && existing.IsRefund==p.IsRefund && existing.PaidAt==p.PaidAt && existing.Source==p.Source?new(){Response=new(){ExternalId=existing.ExternalId,Revision=existing.FinancialEntry.Revision}}:new(){Conflict="Chave já utilizada para outra baixa."};
  var entry=await InventoryRules.RequireAsync(entries,p.EntryExternalId,token);var account=await InventoryRules.RequireAsync(accounts,p.AccountExternalId,token);
  if(entry.Revision!=p.Revision)return new(){Conflict="A obrigação mudou. Reabra o detalhe antes de baixar."};
  InventoryRules.Require(entry.Status!="cancelled" && (p.IsRefund ? p.Amount<=entry.SettledAmount:p.Amount<=entry.Amount-entry.SettledAmount),"Amount","Valor excede o saldo disponível para baixa ou estorno.");
  InventoryRules.Require(p.PaidAt>=account.OpenedAt,"PaidAt","Data anterior ao saldo inicial da conta.");
  var entity=FinancialSettlement.Create(p.SettlementKey,entry,account,p.Amount,p.IsRefund,p.PaidAt,p.Source,request.ActorUserId,DateTime.UtcNow);
  entry.Settle(p.Amount,p.IsRefund,request.ActorUserId,DateTime.UtcNow);await entries.UpdateAsync(entry,token);await settlements.AddAsync(entity,token);
  if(!await InventoryRules.SaveAsync(settlements,token))return new(){Conflict="Baixa concorrente. Consulte o histórico antes de repetir."};return new(){Response=new(){ExternalId=entity.ExternalId,Revision=entry.Revision}};
 }
}
public sealed class CreateFinancialSettlementValidator:AbstractValidator<CreateFinancialSettlementCommand>{public CreateFinancialSettlementValidator(){RuleFor(x=>x.SettlementKey).NotEmpty();RuleFor(x=>x.EntryExternalId).NotEmpty();RuleFor(x=>x.AccountExternalId).NotEmpty();RuleFor(x=>x.Revision).GreaterThan(0);RuleFor(x=>x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000).PrecisionScale(18,2,true);RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);RuleFor(x=>x.PaidAt).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000 && v<=DateTime.UtcNow);}}
public sealed class CreateFinancialSettlementMediatorValidator:AbstractValidator<CreateFinancialSettlementMediatorCommand>{public CreateFinancialSettlementMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreateFinancialSettlementValidator()).OverridePropertyName(string.Empty);}}
