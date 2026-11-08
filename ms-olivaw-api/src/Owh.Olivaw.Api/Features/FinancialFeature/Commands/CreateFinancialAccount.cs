using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Commands;
public sealed class CreateFinancialAccountMediatorCommand:IRequest<CreateFinancialAccountResult>{public CreateFinancialAccountCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}}
public sealed class CreateFinancialAccountResult{public string? Conflict{get;set;}public CreateFinancialAccountResponse Response{get;set;}=new();}
public sealed class CreateFinancialAccountHandler(IRepository<FinancialAccount> accounts):IRequestHandler<CreateFinancialAccountMediatorCommand,CreateFinancialAccountResult>
{
 public async Task<CreateFinancialAccountResult> Handle(CreateFinancialAccountMediatorCommand request,CancellationToken token){ValidationHelper.ThrowIfNull(request);var p=request.Payload;var existing=await accounts.GetFirstOrDefaultAsync(x=>x.AccountKey==p.AccountKey,token);if(existing!=null)return existing.Name==p.Name && existing.OpeningBalance==p.OpeningBalance && existing.OpenedAt==p.OpenedAt ? new(){Response=new(){ExternalId=existing.ExternalId}}:new(){Conflict="Chave utilizada para outra conta."};var entity=FinancialAccount.Create(p.AccountKey,p.Name,p.OpeningBalance,p.OpenedAt,request.ActorUserId,DateTime.UtcNow);await accounts.AddAsync(entity,token);if(!await InventoryRules.SaveAsync(accounts,token))return new(){Conflict="Conta registrada por outro envio. Confira a lista."};return new(){Response=new(){ExternalId=entity.ExternalId}};}
}
public sealed class CreateFinancialAccountValidator:AbstractValidator<CreateFinancialAccountCommand>{public CreateFinancialAccountValidator(){RuleFor(x=>x.AccountKey).NotEmpty();RuleFor(x=>x.Name).NotEmpty().MaximumLength(120);RuleFor(x=>x.OpeningBalance).InclusiveBetween(-100000000,100000000).PrecisionScale(18,2,true);RuleFor(x=>x.OpenedAt).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v.Year>=2000 && v<=DateTime.UtcNow);}}
public sealed class CreateFinancialAccountMediatorValidator:AbstractValidator<CreateFinancialAccountMediatorCommand>{public CreateFinancialAccountMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreateFinancialAccountValidator()).OverridePropertyName(string.Empty);}}
