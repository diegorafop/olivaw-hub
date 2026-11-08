using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
public sealed class UpdateSecurityRemediationReviewMediatorCommand : IRequest<UpdateSecurityRemediationResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public UpdateSecurityRemediationReviewCommand Payload { get; set; } = new();
}
public sealed class UpdateSecurityRemediationReviewHandler(IRepository<SecurityRemediation> repository)
    : IRequestHandler<UpdateSecurityRemediationReviewMediatorCommand, UpdateSecurityRemediationResponse>
{
    public async Task<UpdateSecurityRemediationResponse> Handle(UpdateSecurityRemediationReviewMediatorCommand request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, token);
        if (entity == null || entity.Status != "intervention" || entity.LeaseExpiresAt > DateTime.UtcNow || !request.Payload.ConfirmedStopped)
            return new() { Conflict = true };
        if (entity.ActiveHostKey == null) return new() { Status = entity.Status };
        var result = JsonNode.Parse(entity.ResultJson)!.AsObject();
        result["operatorReview"] = new JsonObject { ["actorUserId"] = request.ActorUserId.ToString(), ["reviewedAt"] = DateTime.UtcNow.ToString("O"), ["note"] = request.Payload.Note };
        entity.ResultJson = result.ToJsonString(); entity.ActiveHostKey = null;
        entity.Message = "Revisão manual registrada. Reserva liberada; o achado continua requerendo intervenção.";
        entity.UpdatedAt = DateTime.UtcNow;entity.Revision++;
        await repository.UpdateAsync(entity, token);
        try { await repository.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true }; }
        return new() { Status = entity.Status };
    }
}
public sealed class UpdateSecurityRemediationReviewValidator : AbstractValidator<UpdateSecurityRemediationReviewMediatorCommand>
{
    public UpdateSecurityRemediationReviewValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();RuleFor(item => item.ActorUserId).NotEmpty();RuleFor(item => item.Payload).NotNull();
        When(item => item.Payload != null, () => {
            RuleFor(item => item.Payload.ConfirmedStopped).Equal(true);
            RuleFor(item => item.Payload.Note).NotEmpty().MinimumLength(20).MaximumLength(1000);
        });
    }
}
