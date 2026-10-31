using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Commands;
public sealed class CreateFinancialTransferMediatorCommand:IRequest<CreateFinancialTransferResult>{public CreateFinancialTransferCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}}
public sealed class CreateFinancialTransferResult{public string? Conflict{get;set;}public CreateFinancialTransferResponse Response{get;set;}=new();}
public sealed class CreateFinancialTransferHandler(IRepository<FinancialTransfer> transfers,IRepository<FinancialAccount> accounts):IRequestHandler<CreateFinancialTransferMediatorCommand,CreateFinancialTransferResult>
{
 public async Task<CreateFinancialTransferResult> Handle(CreateFinancialTransferMediatorCommand request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;var existing=await transfers.GetFirstOrDefaultAsync(x=>x.TransferKey==p.TransferKey,token,x=>x.FromAccount,x=>x.ToAccount);
  if(existing!=null)return existing.FromAccount.ExternalId==p.FromAccountExternalId && existing.ToAccount.ExternalId==p.ToAccountExternalId && existing.Amount==p.Amount && existing.TransferredAt==p.TransferredAt && existing.Source==p.Source?new(){Response=new(){ExternalId=existing.ExternalId}}:new(){Conflict="Chave já utilizada para outra transferência."};
  var from=await InventoryRules.RequireAsync(accounts,p.FromAccountExternalId,token);var to=await InventoryRules.RequireAsync(accounts,p.ToAccountExternalId,token);
  InventoryRules.Require(p.TransferredAt>=from.OpenedAt && p.TransferredAt>=to.OpenedAt,"TransferredAt","Data anterior ao saldo inicial de uma conta.");
  var entity=FinancialTransfer.Create(p.TransferKey,from,to,p.Amount,p.TransferredAt,p.Source,request.ActorUserId,DateTime.UtcNow);await transfers.AddAsync(entity,token);if(!await InventoryRules.SaveAsync(transfers,token))return new(){Conflict="Transferência já registrada. Confira o histórico."};return new(){Response=new(){ExternalId=entity.ExternalId}};
 }
}
public sealed class CreateFinancialTransferValidator:AbstractValidator<CreateFinancialTransferCommand>{public CreateFinancialTransferValidator(){RuleFor(x=>x.TransferKey).NotEmpty();RuleFor(x=>x.FromAccountExternalId).NotEmpty();RuleFor(x=>x.ToAccountExternalId).NotEmpty().NotEqual(x=>x.FromAccountExternalId);RuleFor(x=>x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000).PrecisionScale(18,2,true);RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);RuleFor(x=>x.TransferredAt).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000 && v<=DateTime.UtcNow);}}
public sealed class CreateFinancialTransferMediatorValidator:AbstractValidator<CreateFinancialTransferMediatorCommand>{public CreateFinancialTransferMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreateFinancialTransferValidator()).OverridePropertyName(string.Empty);}}
