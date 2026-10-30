using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Internals;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
namespace Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
public sealed class ReadAwsInvoiceByIdMediatorQuery : IRequest<ReadAwsInvoiceByIdResponse> { public ReadAwsInvoiceByIdQuery Payload { get; set; } = new(); }
public sealed class ReadAwsInvoiceByIdHandler(IRepository<AwsInvoice> repository,IRepository<AwsInvoiceExpense> links) : IRequestHandler<ReadAwsInvoiceByIdMediatorQuery,ReadAwsInvoiceByIdResponse>
{
    public async Task<ReadAwsInvoiceByIdResponse> Handle(ReadAwsInvoiceByIdMediatorQuery request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var entity=await InventoryRules.RequireAsync(repository,request.Payload.ExternalId,token);
        var expenses=await links.TakeOrderedAsync(x=>x.AwsInvoiceId==entity.Id && x.DeletedAt==null,101,x=>x.Id,token,x=>x.Expense);
        return new(){Invoice=entity.ToItem(),RawData=entity.RawData,Expenses=expenses.Select(x=>new AwsInvoiceExpenseItem{ExternalId=x.Expense.ExternalId,Description=x.Expense.Description,AmountBrl=x.Expense.AmountBrl,Kind=x.Expense.Kind}).ToList()};
    }
}
public sealed class ReadAwsInvoiceByIdValidator : AbstractValidator<ReadAwsInvoiceByIdQuery> { public ReadAwsInvoiceByIdValidator(){RuleFor(x=>x.ExternalId).NotEmpty();} }
public sealed class ReadAwsInvoiceByIdMediatorValidator : AbstractValidator<ReadAwsInvoiceByIdMediatorQuery> { public ReadAwsInvoiceByIdMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadAwsInvoiceByIdValidator()).OverridePropertyName(string.Empty);} }
