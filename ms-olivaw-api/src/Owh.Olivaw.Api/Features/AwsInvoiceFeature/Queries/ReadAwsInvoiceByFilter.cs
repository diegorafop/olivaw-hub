using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Internals;
namespace Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
public sealed class ReadAwsInvoiceByFilterMediatorQuery : IRequest<ReadAwsInvoiceByFilterResponse> { public ReadAwsInvoiceByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadAwsInvoiceByFilterHandler(IRepository<AwsInvoice> repository) : IRequestHandler<ReadAwsInvoiceByFilterMediatorQuery,ReadAwsInvoiceByFilterResponse>
{
    public async Task<ReadAwsInvoiceByFilterResponse> Handle(ReadAwsInvoiceByFilterMediatorQuery request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;
        var rows=(await repository.TakeOrderedAsync(x=>x.DeletedAt==null && (!p.BillingYear.HasValue || x.BillingYear==p.BillingYear) && (!p.BillingMonth.HasValue || x.BillingMonth==p.BillingMonth) && (p.BeforeId==0 || x.Id<p.BeforeId),51,x=>x.Id,token)).OrderByDescending(x=>x.Id).ToList();
        return new(){Items=rows.Take(50).Select(x=>x.ToItem()).ToList(),NextBeforeId=rows.Count>50 ? rows[49].Id : null};
    }
}
public sealed class ReadAwsInvoiceByFilterValidator : AbstractValidator<ReadAwsInvoiceByFilterQuery> { public ReadAwsInvoiceByFilterValidator(){RuleFor(x=>x.BillingYear).InclusiveBetween(2000,2100);RuleFor(x=>x.BillingMonth).InclusiveBetween(1,12);RuleFor(x=>x.BeforeId).GreaterThanOrEqualTo(0);} }
public sealed class ReadAwsInvoiceByFilterMediatorValidator : AbstractValidator<ReadAwsInvoiceByFilterMediatorQuery> { public ReadAwsInvoiceByFilterMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadAwsInvoiceByFilterValidator()).OverridePropertyName(string.Empty);} }
