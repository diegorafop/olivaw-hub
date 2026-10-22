using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Npgsql;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Commands;

public sealed class UpdateLeadFollowUpMediatorCommand : IRequest<UpdateLeadFollowUpResult>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public UpdateLeadFollowUpCommand Payload { get; set; } = new();
}
public sealed class UpdateLeadFollowUpResult
{
    public bool Conflict { get; set; }
    public UpdateLeadFollowUpResponse Response { get; set; } = new();
}
public sealed class UpdateLeadFollowUpHandler(IRepository<Lead> repository, IRepository<LeadChange> changes)
    : IRequestHandler<UpdateLeadFollowUpMediatorCommand, UpdateLeadFollowUpResult>
{
    public async Task<UpdateLeadFollowUpResult> Handle(UpdateLeadFollowUpMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        if (lead.FollowUpRevision != request.Payload.Revision) return new() { Conflict = true };
        var validator = new InlineValidator<UpdateLeadFollowUpCommand>();
        validator.RuleFor(item => item.Note).NotEmpty().When(item => item.Status == lead.FollowUpStatus)
            .WithMessage("Informe uma nota ou altere a etapa.");
        await validator.ValidateAndThrowAsync(request.Payload, cancellationToken);
        var change = lead.UpdateFollowUp(request.Payload.Status, request.Payload.Note, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.UpdateAsync(lead, cancellationToken);
        await changes.AddAsync(change, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new() { Conflict = true };
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" }
            or SqlException { Number: 2601 or 2627 })
        {
            return new() { Conflict = true };
        }
        return new() { Response = new() { Revision = lead.FollowUpRevision } };
    }
}
public sealed class UpdateLeadFollowUpValidator : AbstractValidator<UpdateLeadFollowUpCommand>
{
    public UpdateLeadFollowUpValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(item => item.Status).Must(Lead.IsFollowUpStatus).WithMessage("Etapa inválida.");
        RuleFor(item => item.Note).NotNull().MaximumLength(3000);
    }
}
public sealed class UpdateLeadFollowUpMediatorValidator : AbstractValidator<UpdateLeadFollowUpMediatorCommand>
{
    public UpdateLeadFollowUpMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateLeadFollowUpValidator()).OverridePropertyName(string.Empty);
    }
}
