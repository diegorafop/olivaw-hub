using FluentValidation;
using MediatR;
using System.Security.Cryptography;
using System.Text.Json;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Commands;
namespace Owh.Olivaw.Api.Features.AwsInvoiceFeature.Commands;
public sealed class CreateAwsInvoiceMediatorCommand : IRequest<CreateAwsInvoiceResult> { public CreateAwsInvoiceCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } }
public sealed class CreateAwsInvoiceResult { public string? Conflict { get; set; } public CreateAwsInvoiceResponse Response { get; set; } = new(); }
public sealed class CreateAwsInvoiceHandler(IRepository<AwsInvoice> invoices, IRepository<Expense> expenses, IRepository<AwsInvoiceExpense> links) : IRequestHandler<CreateAwsInvoiceMediatorCommand,CreateAwsInvoiceResult>
{
    public async Task<CreateAwsInvoiceResult> Handle(CreateAwsInvoiceMediatorCommand request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p=request.Payload;
        var digest=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { p.Issuer,p.AccountReference,p.InvoiceNumber,p.BillingYear,p.BillingMonth,p.Currency,p.DocumentStatus,p.Completeness,p.GrossAmount,p.CreditAmount,p.TaxAmount,p.NetAmount,p.ExchangeRateToBrl,p.ExchangeRateNature,p.Source,p.RawData,Expenses=p.ExpenseExternalIds.OrderBy(x=>x).ToArray() })));
        var existing=await invoices.GetFirstOrDefaultAsync(x=>x.SubmissionKey==p.SubmissionKey || x.Digest==digest,token);
        if(existing!=null) return existing.Digest!=digest ? new(){Conflict="Chave já utilizada para outro documento."}:new(){Response=new(){ExternalId=existing.ExternalId}};
        var entity=AwsInvoice.Create(p.SubmissionKey,digest,p.Issuer,p.AccountReference,p.InvoiceNumber,p.BillingYear,p.BillingMonth,p.Currency,p.DocumentStatus,p.Completeness,p.GrossAmount,p.CreditAmount,p.TaxAmount,p.NetAmount,p.ExchangeRateToBrl,p.ExchangeRateNature,p.Source,p.RawData,request.ActorUserId,DateTime.UtcNow);
        foreach(var id in p.ExpenseExternalIds)
        {
            var expense=await InventoryRules.RequireAsync(expenses,id,token);
            if(await links.GetFirstOrDefaultAsync(x=>x.ExpenseId==expense.Id,token)!=null) return new(){Conflict="Despesa já vinculada a outra evidência; confira antes de importar."};
            InventoryRules.Require(expense.OccurredAt.Year==p.BillingYear && expense.OccurredAt.Month==p.BillingMonth,"ExpenseExternalIds","A despesa precisa pertencer à competência do documento.");
            entity.Link(expense,request.ActorUserId,DateTime.UtcNow);
        }
        await invoices.AddAsync(entity,token);
        if(!await InventoryRules.SaveAsync(invoices,token)) return new(){Conflict="Documento ou vínculo já registrado por outro envio. Consulte antes de repetir."};
        return new(){Response=new(){ExternalId=entity.ExternalId}};
    }
}
public sealed class CreateAwsInvoiceValidator : AbstractValidator<CreateAwsInvoiceCommand>
{
    public CreateAwsInvoiceValidator()
    {
        RuleFor(x=>x.SubmissionKey).NotEmpty();RuleFor(x=>x.Issuer).NotEmpty().MaximumLength(200);RuleFor(x=>x.AccountReference).NotEmpty().MaximumLength(120);RuleFor(x=>x.InvoiceNumber).MaximumLength(120);
        RuleFor(x=>x.BillingYear).InclusiveBetween(2000,2100);RuleFor(x=>x.BillingMonth).InclusiveBetween(1,12);RuleFor(x=>x.Currency).Must(v=>v is "BRL" or "USD");
        RuleFor(x=>x.DocumentStatus).Must(v=>v is "provisional" or "issued" or "paid");RuleFor(x=>x.Completeness).Must(v=>v is "complete" or "issuer-section" or "partial");
        RuleFor(x=>x).Must(p=>p.DocumentStatus!="paid" || (p.Completeness=="complete" && !string.IsNullOrWhiteSpace(p.InvoiceNumber))).WithName("DocumentStatus");
        RuleFor(x=>x.GrossAmount).InclusiveBetween(0,1000000000).PrecisionScale(24,10,true);
RuleFor(x=>x.CreditAmount).InclusiveBetween(0,1000000000).PrecisionScale(24,10,true);
RuleFor(x=>x.TaxAmount).InclusiveBetween(0,1000000000).PrecisionScale(24,10,true);
RuleFor(x=>x.NetAmount).InclusiveBetween(0,1000000000).PrecisionScale(24,10,true);
        RuleFor(x=>x).Must(p=>!p.GrossAmount.HasValue || !p.CreditAmount.HasValue || !p.TaxAmount.HasValue || !p.NetAmount.HasValue || Math.Abs(p.GrossAmount.Value-p.CreditAmount.Value+p.TaxAmount.Value-p.NetAmount.Value)<=0.01m).WithName("NetAmount");
        RuleFor(x=>x.ExchangeRateToBrl).GreaterThan(0).LessThanOrEqualTo(1000).PrecisionScale(18,6,true);RuleFor(x=>x.ExchangeRateToBrl).Equal(1).When(x=>x.Currency=="BRL");
        RuleFor(x=>x.ExchangeRateNature).Must(v=>v is "unknown" or "managerial" or "payment");RuleFor(x=>x.ExchangeRateNature).Equal("unknown").When(x=>!x.ExchangeRateToBrl.HasValue);
        RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);RuleFor(x=>x.RawData).NotEmpty().MaximumLength(1000000).Must(IsObjectJson);
        RuleFor(x=>x.ExpenseExternalIds).NotNull().Must(v=>v!=null && v.Count<=100 && v.Distinct().Count()==v.Count);RuleForEach(x=>x.ExpenseExternalIds).NotEmpty();
    }
    private static bool IsObjectJson(string value) { try { using var doc=JsonDocument.Parse(value,new JsonDocumentOptions{MaxDepth=48});return doc.RootElement.ValueKind==JsonValueKind.Object; } catch(ArgumentException){return false;} catch(JsonException){return false;} }
}
public sealed class CreateAwsInvoiceMediatorValidator : AbstractValidator<CreateAwsInvoiceMediatorCommand> { public CreateAwsInvoiceMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreateAwsInvoiceValidator()).OverridePropertyName(string.Empty);} }
