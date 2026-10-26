using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalUsageSampleMediatorCommand : IRequest<CreatePortalUsageSampleResult> { public CreatePortalUsageSampleCommand Payload {get;set;}=new(); public Guid ActorUserId {get;set;} }
public sealed class CreatePortalUsageSampleResult { public string? Conflict {get;set;} public CreatePortalUsageSampleResponse Response {get;set;}=new(); }
public sealed class CreatePortalUsageSampleHandler(IRepository<PortalUsageSample> samples,IRepository<Customer> customers) : IRequestHandler<CreatePortalUsageSampleMediatorCommand,CreatePortalUsageSampleResult>
{
    public async Task<CreatePortalUsageSampleResult> Handle(CreatePortalUsageSampleMediatorCommand request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;
        var customer=await InventoryRules.RequireAsync(customers,p.CustomerExternalId,token);
        var old=await samples.GetFirstOrDefaultAsync(x=>x.SampleKey==p.SampleKey,token);
        if(old!=null) return old.CustomerId==customer.Id && old.Day==p.Day && old.StorageUsedMiB==p.StorageUsedMiB && old.PageViews==p.PageViews && old.Source==p.Source ? new(){Response=new(){ExternalId=old.ExternalId}} : new(){Conflict="Chave de amostra já utilizada."};
        if(await samples.ExistsAsync(x=>x.CustomerId==customer.Id && x.Day==p.Day && x.DeletedAt==null,token))return new(){Conflict="Já existe uma amostra consolidada para esta empresa e dia."};
        var sample=PortalUsageSample.Create(customer,p.SampleKey,p.Day,p.StorageUsedMiB,p.PageViews,p.Source,request.ActorUserId,DateTime.UtcNow);
        await samples.AddAsync(sample,token);
        if(!await InventoryRules.SaveAsync(samples,token))return new(){Conflict="Amostra concorrente; confira o registro antes de reenviar."};
        return new(){Response=new(){ExternalId=sample.ExternalId}};
    }
}
public sealed class CreatePortalUsageSampleValidator : AbstractValidator<CreatePortalUsageSampleCommand>
{
    public CreatePortalUsageSampleValidator(){RuleFor(x=>x.CustomerExternalId).NotEmpty();RuleFor(x=>x.SampleKey).NotEmpty();RuleFor(x=>x.Day).Must(v=>v.Kind==DateTimeKind.Utc && v.TimeOfDay==TimeSpan.Zero && v<DateTime.UtcNow.Date);RuleFor(x=>x.StorageUsedMiB).InclusiveBetween(0,1000000000).PrecisionScale(18,3,true);RuleFor(x=>x.PageViews).InclusiveBetween(0,1000000000000);RuleFor(x=>x).Must(x=>x.StorageUsedMiB.HasValue || x.PageViews.HasValue).WithMessage("Informe ao menos uma métrica.");RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);}
}
public sealed class CreatePortalUsageSampleMediatorValidator : AbstractValidator<CreatePortalUsageSampleMediatorCommand>{public CreatePortalUsageSampleMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreatePortalUsageSampleValidator()).OverridePropertyName(string.Empty);}}
