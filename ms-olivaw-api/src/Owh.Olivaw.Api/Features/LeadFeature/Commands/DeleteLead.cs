using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Commands;

public sealed class DeleteLeadMediatorCommand : IRequest<DeleteLeadResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DeleteLeadCommand Payload { get; set; } = new();
}
public sealed class DeleteLeadHandler(IRepository<Lead> repository, IRepository<LeadChange> changes)
    : IRequestHandler<DeleteLeadMediatorCommand, DeleteLeadResponse>
{
    public async Task<DeleteLeadResponse> Handle(DeleteLeadMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstAsync(item => item.ExternalId == request.ExternalId, cancellationToken);
        if (lead.DeletedAt.HasValue) return new() { AlreadyDeleted = true };
        if (lead.Revision != request.Payload.Revision || lead.FollowUpRevision != request.Payload.FollowUpRevision)
            return new() { Conflict = true };
        var change = lead.Delete(request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.UpdateAsync(lead, cancellationToken);
        await changes.AddAsync(change, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true }; }
        return new();
    }
}
public sealed class DeleteLeadValidator : AbstractValidator<DeleteLeadCommand>
{
    public DeleteLeadValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.FollowUpRevision).InclusiveBetween(0, long.MaxValue - 1);
    }
}
public sealed class DeleteLeadMediatorValidator : AbstractValidator<DeleteLeadMediatorCommand>
{
    public DeleteLeadMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new DeleteLeadValidator()).OverridePropertyName(string.Empty);
    }
}
