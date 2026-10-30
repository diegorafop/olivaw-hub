using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Queries;
public sealed class ReadFinancialAccountByFilterMediatorQuery:IRequest<ReadFinancialAccountByFilterResponse>{public ReadFinancialAccountByFilterQuery Payload{get;set;}=new();}
public sealed class ReadFinancialAccountByFilterHandler(IRepository<FinancialAccount> accounts):IRequestHandler<ReadFinancialAccountByFilterMediatorQuery,ReadFinancialAccountByFilterResponse>
{
 public async Task<ReadFinancialAccountByFilterResponse> Handle(ReadFinancialAccountByFilterMediatorQuery request,CancellationToken token){ValidationHelper.ThrowIfNull(request);var p=request.Payload;var rows=(await accounts.TakeOrderedAsync(x=>x.DeletedAt==null && (p.BeforeId==0 || x.Id<p.BeforeId),51,x=>x.Id,token)).ToList();return new(){Items=rows.Take(50).Select(x=>new FinancialAccountItem{ExternalId=x.ExternalId,Name=x.Name,OpeningBalance=x.OpeningBalance,OpenedAt=x.OpenedAt}).ToList(),NextBeforeId=rows.Count>50?rows[49].Id:null};}
}
public sealed class ReadFinancialAccountByFilterValidator:AbstractValidator<ReadFinancialAccountByFilterQuery>{public ReadFinancialAccountByFilterValidator(){RuleFor(x=>x.BeforeId).GreaterThanOrEqualTo(0);}}
public sealed class ReadFinancialAccountByFilterMediatorValidator:AbstractValidator<ReadFinancialAccountByFilterMediatorQuery>{public ReadFinancialAccountByFilterMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadFinancialAccountByFilterValidator()).OverridePropertyName(string.Empty);}}
