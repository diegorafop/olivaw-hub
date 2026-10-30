using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Commands;
public sealed class UpdateFinancialEntryMediatorCommand:IRequest<UpdateFinancialEntryResult>{public UpdateFinancialEntryCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}}
public sealed class UpdateFinancialEntryResult{public string? Conflict{get;set;}public UpdateFinancialEntryResponse Response{get;set;}=new();}
public sealed class UpdateFinancialEntryHandler(IRepository<FinancialEntry> entries):IRequestHandler<UpdateFinancialEntryMediatorCommand,UpdateFinancialEntryResult>
{
 public async Task<UpdateFinancialEntryResult> Handle(UpdateFinancialEntryMediatorCommand request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;var entry=await InventoryRules.RequireAsync(entries,p.ExternalId,token);
  if(entry.Revision!=p.Revision)return new(){Conflict="Registro alterado; reabra antes de cancelar."};
  InventoryRules.Require(entry.SettledAmount==0,"CancellationReason","Estorne as baixas antes de cancelar.");entry.Cancel(p.CancellationReason,request.ActorUserId,DateTime.UtcNow);await entries.UpdateAsync(entry,token);if(!await InventoryRules.SaveAsync(entries,token))return new(){Conflict="Registro alterado por outra operação."};return new(){Response=new(){ExternalId=entry.ExternalId,Revision=entry.Revision}};
 }
}
public sealed class UpdateFinancialEntryValidator:AbstractValidator<UpdateFinancialEntryCommand>{public UpdateFinancialEntryValidator(){RuleFor(x=>x.ExternalId).NotEmpty();RuleFor(x=>x.Revision).GreaterThan(0);RuleFor(x=>x.CancellationReason).NotEmpty().MaximumLength(500);}}
public sealed class UpdateFinancialEntryMediatorValidator:AbstractValidator<UpdateFinancialEntryMediatorCommand>{public UpdateFinancialEntryMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new UpdateFinancialEntryValidator()).OverridePropertyName(string.Empty);}}
