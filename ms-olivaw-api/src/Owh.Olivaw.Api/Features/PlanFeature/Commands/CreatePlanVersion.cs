using FluentValidation;
using MediatR;
using System.Security.Cryptography;
using System.Text.Json;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreatePlanVersionMediatorCommand : IRequest<CreatePlanVersionResult> { public CreatePlanVersionCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } }
public sealed class CreatePlanVersionResult { public string? Conflict { get; set; } public CreatePlanVersionResponse Response { get; set; } = new(); }
public sealed class CreatePlanVersionHandler(IRepository<PlanVersion> versions,IRepository<PlanStudy> studies) : IRequestHandler<CreatePlanVersionMediatorCommand,CreatePlanVersionResult>
{
    public async Task<CreatePlanVersionResult> Handle(CreatePlanVersionMediatorCommand request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;
        var digest=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(p)));
        var previous=await versions.GetFirstOrDefaultAsync(x=>x.PublicationKey==p.PublicationKey,token);
        if(previous!=null)return previous.Digest==digest ? new(){Response=new(){ExternalId=previous.ExternalId,Version=previous.Version}} : new(){Conflict="Chave já utilizada em outra versão."};
        var last=(await versions.TakeOrderedAsync(x=>x.PlanKey==p.PlanKey,1,x=>x.Version,token)).FirstOrDefault();
        var study=p.PlanStudyExternalId.HasValue ? await InventoryRules.RequireAsync(studies,p.PlanStudyExternalId.Value,token):null;
        var entity=PlanVersion.Create(p.PublicationKey,digest,p.PlanKey,(last?.Version ?? 0)+1,p.MonthlyPrice,p.StorageLimitGiB,p.MemoryLimitMiB,p.CpuLimit,p.Conditions,p.ApprovalReference,p.EffectiveAt,study,request.ActorUserId,DateTime.UtcNow);
        await versions.AddAsync(entity,token);
        if(!await InventoryRules.SaveAsync(versions,token)) return new(){Conflict="Uma versão já foi publicada nesta vigência ou houve concorrência. Confira o histórico."};
        return new(){Response=new(){ExternalId=entity.ExternalId,Version=entity.Version}};
    }
}
public sealed class CreatePlanVersionValidator : AbstractValidator<CreatePlanVersionCommand>
{
    public CreatePlanVersionValidator()
    {
        RuleFor(x=>x.PublicationKey).NotEmpty();RuleFor(x=>x.PlanKey).Must(PlanVersion.IsPlan);RuleFor(x=>x.MonthlyPrice).InclusiveBetween(0,100000000).PrecisionScale(18,2,true);
        RuleFor(x=>x.StorageLimitGiB).GreaterThan(0).LessThanOrEqualTo(1000000000).PrecisionScale(18,3,true);RuleFor(x=>x.MemoryLimitMiB).GreaterThan(0).LessThanOrEqualTo(1000000000).PrecisionScale(18,3,true);RuleFor(x=>x.CpuLimit).GreaterThan(0).LessThanOrEqualTo(1024).PrecisionScale(7,3,true);
        RuleFor(x=>x.Conditions).NotEmpty().MaximumLength(3000);RuleFor(x=>x.ApprovalReference).NotEmpty().MaximumLength(300);RuleFor(x=>x.PlanStudyExternalId).NotEqual(Guid.Empty);
        RuleFor(x=>x.EffectiveAt).Must(v=>v.Kind==DateTimeKind.Utc && v.Day==1 && v.TimeOfDay==TimeSpan.Zero);
    }
}
public sealed class CreatePlanVersionMediatorValidator : AbstractValidator<CreatePlanVersionMediatorCommand>{public CreatePlanVersionMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.Payload).NotNull().SetValidator(new CreatePlanVersionValidator()).OverridePropertyName(string.Empty);}}
