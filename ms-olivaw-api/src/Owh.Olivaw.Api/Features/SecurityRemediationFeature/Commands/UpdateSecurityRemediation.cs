using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Internals;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
public sealed class UpdateSecurityRemediationMediatorCommand : IRequest<UpdateSecurityRemediationResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public UpdateSecurityRemediationCommand Payload { get; set; } = new();
}
public sealed class UpdateSecurityRemediationHandler(IRepository<SecurityRemediation> repository)
    : IRequestHandler<UpdateSecurityRemediationMediatorCommand, UpdateSecurityRemediationResponse>
{
    public async Task<UpdateSecurityRemediationResponse> Handle(UpdateSecurityRemediationMediatorCommand request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.ExecutorUserId == request.ActorUserId && item.DeletedAt == null, token);
        if (entity == null) return new() { Conflict = true };
        var input = request.Payload;
        if (input.Action == "claim")
        {
            if (entity.Status == "executing" && entity.LeaseToken == input.LeaseToken && entity.LeaseExpiresAt > DateTime.UtcNow)
                return new() { Status = entity.Status };
            if (entity.Status != "authorized" || entity.ApprovalExpiresAt <= DateTime.UtcNow) return new() { Conflict = true };
            entity.ActiveHostKey = entity.HostId; entity.Status = "executing"; entity.LeaseToken = input.LeaseToken;
            entity.LeaseExpiresAt = DateTime.UtcNow.AddMinutes(10); entity.Message = "Baley está verificando pré-condições e aplicando o procedimento autorizado.";
        }
        else if (input.Action == "intervention" && entity.Status == "authorized" && entity.LeaseToken == null)
        {
            // Lost claim response may mean no reservation was ever persisted. Abandon safely; never execute it again.
            entity.Status = "intervention"; entity.Message = "Execução não iniciada ou reserva não confirmada; revisão necessária.";
            entity.ResultJson = input.ResultJson;
        }
        else
        {
            if (entity.LeaseToken != input.LeaseToken || entity.LeaseToken == null) return new() { Conflict = true };
            if (entity.Status == input.Action) return new() { Status = entity.Status };
            if (input.Action == "intervention" && entity.Status is "executing" or "validating")
            {
                entity.Status = "intervention"; entity.Message = "Correção não confirmada com segurança; consultar evidências e revisar manualmente.";
            }
            else
            {
                if (entity.LeaseExpiresAt <= DateTime.UtcNow) return new() { Conflict = true };
                if (input.Action == "validating" && entity.Status == "executing")
                {
                    entity.Status = "validating"; entity.Message = "Correção aplicada; conferindo saúde da infraestrutura e ausência do achado.";
                }
                else if (input.Action == "resolved" && entity.Status == "validating" && RemediationRules.HealthProof(input.ResultJson))
                {
                    entity.Status = "resolved"; entity.Message = "Correção e saúde da infraestrutura confirmadas.";
                }
                else return new() { Conflict = true };
            }
            entity.ResultJson = input.ResultJson;
        }
        if (entity.Status == "resolved") entity.ActiveHostKey = null;
        if (entity.Status == "intervention")
        {
            using var result = JsonDocument.Parse(input.ResultJson);
            var uncertain = result.RootElement.TryGetProperty("executionUncertain", out var flag) && flag.ValueKind == JsonValueKind.True;
            if (!uncertain) entity.ActiveHostKey = null;
            else entity.Message = "Execução ou recuperação incerta; correções deste host bloqueadas até revisão manual.";
        }
        entity.UpdatedAt = DateTime.UtcNow; entity.Revision++;
        await repository.UpdateAsync(entity, token);
        try { await repository.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true }; }
        catch (DbUpdateException error) when (error.InnerException is Npgsql.PostgresException { SqlState: "23505" }) { return new() { Conflict = true }; }
        return new() { Status = entity.Status };
    }
}
public sealed class UpdateSecurityRemediationValidator : AbstractValidator<UpdateSecurityRemediationMediatorCommand>
{
    public UpdateSecurityRemediationValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull();
        When(item => item.Payload != null, () => {
            RuleFor(item => item.Payload.LeaseToken).NotEmpty();
            RuleFor(item => item.Payload.Action).Must(value => value is "claim" or "validating" or "resolved" or "intervention");
            RuleFor(item => item.Payload.ResultJson).NotNull().MaximumLength(65536).Must(ValidJson);
        });
    }
    private static bool ValidJson(string value)
    {
        try { using var document = JsonDocument.Parse(value, new JsonDocumentOptions { MaxDepth = 16 }); return document.RootElement.ValueKind == JsonValueKind.Object; }
        catch (Exception error) when (error is JsonException or ArgumentNullException) { return false; }
    }
}
