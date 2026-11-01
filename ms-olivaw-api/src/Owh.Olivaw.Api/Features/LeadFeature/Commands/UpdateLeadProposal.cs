using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Commands;

public sealed class UpdateLeadProposalMediatorCommand : IRequest<UpdateLeadProposalResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public UpdateLeadProposalCommand Payload { get; set; } = new();
}
public sealed class UpdateLeadProposalHandler(IRepository<Lead> repository) : IRequestHandler<UpdateLeadProposalMediatorCommand, UpdateLeadProposalResponse>
{
    public async Task<UpdateLeadProposalResponse> Handle(UpdateLeadProposalMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        var validator = new InlineValidator<UpdateLeadProposalMediatorCommand>();
        validator.RuleFor(item => lead).NotNull().OverridePropertyName("ExternalId")
            .WithMessage(MessageHelper.NotFoundFor<Lead, Guid>(item => item.ExternalId, request.ExternalId));
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        ArgumentNullException.ThrowIfNull(lead);
        if (lead.Revision != request.Payload.Revision) return new() { Conflict = true, Revision = lead.Revision };
        // Preserve the original journey and unknown fields instead of rewriting the public draft.
        var snapshot = System.Text.Json.Nodes.JsonNode.Parse(lead.Snapshot)!.AsObject();
        snapshot["proposal"] = JsonSerializer.SerializeToNode(request.Payload.Proposal, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        snapshot["revision"] = lead.Revision + 1;
        var serialized = snapshot.ToJsonString();
        var sizeValidator = new InlineValidator<string>();
        sizeValidator.RuleFor(item => item).MaximumLength(32000).OverridePropertyName("Proposal");
        await sizeValidator.ValidateAndThrowAsync(serialized, cancellationToken);
        lead.Apply(lead.Revision + 1, serialized, DateTime.UtcNow);
        lead.ActorUserId = request.ActorUserId;
        await repository.UpdateAsync(lead, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true, Revision = request.Payload.Revision }; }
        return new() { Revision = lead.Revision };
    }
}
public sealed class UpdateLeadProposalValidator : AbstractValidator<UpdateLeadProposalCommand>
{
    public UpdateLeadProposalValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.Proposal).NotNull();
        When(item => item.Proposal != null, () => RuleFor(item => item.Proposal).ChildRules(proposal =>
        {
            proposal.RuleFor(item => item.Title).NotEmpty().MaximumLength(160);
            proposal.RuleFor(item => item.Status).Must(value => value is "draft" or "reviewed" or "sent" or "negotiation" or "accepted" or "declined");
            proposal.RuleFor(item => item.Platform).NotNull().MaximumLength(500);
            proposal.RuleFor(item => item.RequirementsAnalysis).NotNull().MaximumLength(3000);
            proposal.RuleFor(item => item.TechnicalApproach).NotNull().MaximumLength(3000);
            proposal.RuleFor(item => item.Assumptions).NotNull().MaximumLength(3000);
            proposal.RuleFor(item => item.ExternalCosts).NotNull().MaximumLength(2000);
            proposal.RuleFor(item => item.Deadline).NotNull().MaximumLength(500);
            proposal.RuleFor(item => item.Terms).NotNull().MaximumLength(2000);
            proposal.RuleFor(item => item.Continuity).NotNull().MaximumLength(3000);
            proposal.RuleFor(item => item.ReservePercent).InclusiveBetween(0, 100);
            proposal.RuleFor(item => item.DiscountPercent).InclusiveBetween(0, 100);
            proposal.RuleFor(item => item.Installments).InclusiveBetween(1, 36);
            proposal.RuleFor(item => item.Rows).NotNull().Must(rows => rows is { Length: >= 1 and <= 40 });
            proposal.RuleForEach(item => item.Rows).NotNull().ChildRules(row =>
            {
                row.RuleFor(item => item.Description).NotEmpty().MaximumLength(500);
                row.RuleFor(item => item.Role).NotEmpty().MaximumLength(120);
                row.RuleFor(item => item.Solution).Must(value => value is "analysis" or "plugin" or "integration" or "custom" or "validation");
                row.RuleFor(item => item.Hours).InclusiveBetween(0.25m, 10000m);
                row.RuleFor(item => item.HourlyRate).InclusiveBetween(0.01m, 100000m);
            });
        }));
    }
}
public sealed class UpdateLeadProposalMediatorValidator : AbstractValidator<UpdateLeadProposalMediatorCommand>
{
    public UpdateLeadProposalMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateLeadProposalValidator()).OverridePropertyName(string.Empty);
    }
}
