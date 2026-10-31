using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Commands;
public sealed class CreateFinancialEntryMediatorCommand:IRequest<CreateFinancialEntryResult>{public CreateFinancialEntryCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}}
public sealed class CreateFinancialEntryResult{public string? Conflict{get;set;}public CreateFinancialEntryResponse Response{get;set;}=new();}
public sealed class CreateFinancialEntryHandler(IRepository<FinancialEntry> entries,IRepository<Customer> customers,IRepository<Expense> expenses):IRequestHandler<CreateFinancialEntryMediatorCommand,CreateFinancialEntryResult>
{
 public async Task<CreateFinancialEntryResult> Handle(CreateFinancialEntryMediatorCommand request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;var existing=await entries.GetFirstOrDefaultAsync(x=>x.EntryKey==p.EntryKey,token,x=>x.Customer!,x=>x.Expense!);
  if(existing!=null)return existing.Description==p.Description && existing.Direction==p.Direction && existing.Category==p.Category && existing.Amount==p.Amount && existing.DueAt==p.DueAt && existing.Source==p.Source && existing.Customer?.ExternalId==p.CustomerExternalId && existing.Expense?.ExternalId==p.ExpenseExternalId ? new(){Response=new(){ExternalId=existing.ExternalId}} : new(){Conflict="Chave já utilizada para outra obrigação."};
  var customer=p.CustomerExternalId.HasValue?await InventoryRules.RequireAsync(customers,p.CustomerExternalId.Value,token):null;var expense=p.ExpenseExternalId.HasValue?await InventoryRules.RequireAsync(expenses,p.ExpenseExternalId.Value,token):null;
  InventoryRules.Require(expense==null || (p.Direction=="payable" && expense.Kind=="cost"),"ExpenseExternalId","Somente custo pode originar conta a pagar.");
  if(expense!=null && await entries.ExistsAsync(x=>x.ExpenseId==expense.Id,token))return new(){Conflict="Já existe uma obrigação vinculada à despesa."};
  var entity=FinancialEntry.Create(p.EntryKey,p.Description,p.Direction,p.Category,p.Amount,p.DueAt,p.Source,customer,expense,request.ActorUserId,DateTime.UtcNow);await entries.AddAsync(entity,token);if(!await InventoryRules.SaveAsync(entries,token))return new(){Conflict="Obrigação registrada em outro envio; confira a lista."};return new(){Response=new(){ExternalId=entity.ExternalId}};
 }
}
public sealed class CreateFinancialEntryValidator:AbstractValidator<CreateFinancialEntryCommand>{public CreateFinancialEntryValidator(){RuleFor(x=>x.EntryKey).NotEmpty();RuleFor(x=>x.Description).NotEmpty().MaximumLength(160);RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);RuleFor(x=>x.Direction).Must(v=>v is "payable" or "receivable");RuleFor(x=>x.Category).Must(v=>v is "infrastructure" or "tools" or "labor" or "tax" or "subscription" or "other");RuleFor(x=>x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000).PrecisionScale(18,2,true);RuleFor(x=>x.DueAt).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000 && v.Year<=2100);RuleFor(x=>x.CustomerExternalId).NotEqual(Guid.Empty);RuleFor(x=>x.ExpenseExternalId).NotEqual(Guid.Empty);}}
public sealed class CreateFinancialEntryMediatorValidator:AbstractValidator<CreateFinancialEntryMediatorCommand>{public CreateFinancialEntryMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreateFinancialEntryValidator()).OverridePropertyName(string.Empty);}}
