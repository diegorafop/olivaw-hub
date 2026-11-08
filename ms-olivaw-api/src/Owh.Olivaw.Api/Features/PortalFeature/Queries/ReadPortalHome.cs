using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalHomeMediatorQuery : IRequest<ReadPortalHomeResponse> {public ReadPortalHomeQuery Payload {get;set;}=new();public Guid ActorUserId {get;set;}public bool Preview {get;set;} }
public sealed class ReadPortalHomeHandler(PortalAccess access,IRepository<Customer> customers,IRepository<PortalUsageSample> samples,IRepository<SupportRequest> requests,IRepository<CustomerPlan> plans) : IRequestHandler<ReadPortalHomeMediatorQuery,ReadPortalHomeResponse>
{
    public async Task<ReadPortalHomeResponse> Handle(ReadPortalHomeMediatorQuery request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;
        var customer=request.Preview ? await InventoryRules.RequireAsync(customers,p.CustomerExternalId,token) : (await access.RequireAsync(request.ActorUserId,p.CustomerExternalId,false,false,token)).Customer;
        // Dias UTC encerrados: não extrapolar visualizações de um dia incompleto.
        var end=DateTime.UtcNow.Date;var start=p.Period=="day"?end.AddDays(-1):p.Period=="week"?end.AddDays(-7):p.Period=="year"?end.AddYears(-1):end.AddDays(-30);
        var rows=(await samples.TakeOrderedAsync(x=>x.CustomerId==customer.Id && x.DeletedAt==null && x.Day>=start && x.Day<end,367,x=>x.Day,token)).ToList();
        var support=(await requests.TakeOrderedAsync(x=>x.CustomerId==customer.Id && x.DeletedAt==null,5001,x=>x.Id,token)).ToList();
        if(support.Count>5000)throw new IdentityFailure(422,"Volume de chamados excede esta consulta. Consulte o atendimento paginado.");
        bool Done(SupportRequest r)=>r.Status is "delivered" or "closed";
        bool Open(SupportRequest r)=>!Done(r) && r.Status!="cancelled";
        var help=support.Where(x=>x.Category!="improvement").ToArray();var improvements=support.Where(x=>x.Category=="improvement").ToArray();
        var response=new ReadPortalHomeResponse {From=start,To=end,ExpectedDays=(int)(end-start).TotalDays,ObservedDays=rows.Count(x=>x.PageViews.HasValue),PageViews=rows.Any(x=>x.PageViews.HasValue)?rows.Sum(x=>x.PageViews??0):null,OpenRequests=help.Count(Open),WaitingRequests=help.Count(x=>x.Status=="waiting"),CompletedRequests=help.Count(Done),WorkingImprovements=improvements.Count(x=>x.Status is "working" or "validation"),QueuedImprovements=improvements.Count(x=>Open(x) && x.Status is not ("working" or "validation")),CompletedImprovements=improvements.Count(Done)};
        var latest=(await samples.TakeOrderedAsync(x=>x.CustomerId==customer.Id && x.DeletedAt==null && x.StorageUsedMiB.HasValue && x.Day<end,1,x=>x.Day,token)).FirstOrDefault();
        var plan=(await plans.TakeOrderedAsync(x=>x.CustomerId==customer.Id && x.DeletedAt==null && x.StartsAt<=DateTime.UtcNow,1,x=>x.StartsAt,token)).FirstOrDefault();
        response.StorageUsedMiB=latest?.StorageUsedMiB;response.ObservedAt=latest?.Day;response.StorageLimitMiB=plan?.StorageLimitGiB*1024;
        if(response.StorageUsedMiB.HasValue && response.StorageLimitMiB>0)response.StoragePercent=decimal.Round(response.StorageUsedMiB.Value/response.StorageLimitMiB.Value*100,1);
        // Os pontos ausentes continuam null; a UI não desenha zero ou liga lacunas como leituras reais.
        for(var date=start;date<end;date=date.AddDays(1)){var row=rows.SingleOrDefault(x=>x.Day==date);response.Series.Add(new(){Date=date,StorageUsedMiB=row?.StorageUsedMiB,PageViews=row?.PageViews});}
        return response;
    }
}
public sealed class ReadPortalHomeValidator : AbstractValidator<ReadPortalHomeQuery>{public ReadPortalHomeValidator(){RuleFor(x=>x.CustomerExternalId).NotEmpty();RuleFor(x=>x.Period).Must(v=>v is "day" or "week" or "month" or "year");}}
public sealed class ReadPortalHomeMediatorValidator : AbstractValidator<ReadPortalHomeMediatorQuery>{public ReadPortalHomeMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadPortalHomeValidator()).OverridePropertyName(string.Empty);}}
