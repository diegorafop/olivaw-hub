using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Commands;

public sealed class UpdateLeadQualificationMediatorCommand : IRequest<UpdateLeadQualificationResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public UpdateLeadQualificationCommand Payload { get; set; } = new();
}
public sealed class UpdateLeadQualificationHandler(IRepository<Lead> repository)
    : IRequestHandler<UpdateLeadQualificationMediatorCommand, UpdateLeadQualificationResponse>
{
    public async Task<UpdateLeadQualificationResponse> Handle(UpdateLeadQualificationMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        var validator = new InlineValidator<UpdateLeadQualificationMediatorCommand>();
        validator.RuleFor(item => lead).NotNull().OverridePropertyName("ExternalId")
            .WithMessage(MessageHelper.NotFoundFor<Lead, Guid>(item => item.ExternalId, request.ExternalId));
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        ArgumentNullException.ThrowIfNull(lead);
        if (lead.Revision != request.Payload.Revision) return new() { Conflict = true, Revision = lead.Revision };
        var snapshot = JsonNode.Parse(lead.Snapshot)!.AsObject();
        var now = DateTime.UtcNow;
        snapshot["qualification"] = JsonSerializer.SerializeToNode(request.Payload.Qualification, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        snapshot["qualificationUpdatedAt"] = now;
        snapshot["qualificationUpdatedBy"] = request.ActorName;
        snapshot["revision"] = lead.Revision + 1;
        var serialized = snapshot.ToJsonString();
        var sizeValidator = new InlineValidator<string>();
        sizeValidator.RuleFor(item => item).MaximumLength(32000).OverridePropertyName("Qualification");
        await sizeValidator.ValidateAndThrowAsync(serialized, cancellationToken);
        lead.Apply(lead.Revision + 1, serialized, now);
        lead.ActorUserId = request.ActorUserId;
        await repository.UpdateAsync(lead, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true, Revision = request.Payload.Revision }; }
        return new() { Revision = lead.Revision, UpdatedAt = now };
    }
}
public sealed class UpdateLeadQualificationValidator : AbstractValidator<UpdateLeadQualificationCommand>
{
    private static bool IsWebUrl(string value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
        return uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo);
    }
    public UpdateLeadQualificationValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.Qualification).NotNull();
        When(item => item.Qualification != null, () => RuleFor(item => item.Qualification).ChildRules(qualification =>
        {
            qualification.RuleFor(item => item.SchemaVersion).Equal(1);
            qualification.RuleFor(item => item.Segment).NotNull().MaximumLength(100);
            qualification.RuleFor(item => item.Region).NotNull().MaximumLength(120);
            qualification.RuleFor(item => item.Phone).NotNull().MaximumLength(40);
            qualification.RuleFor(item => item.Whatsapp).NotNull().MaximumLength(11)
                .Must(value => string.IsNullOrEmpty(value) || System.Text.RegularExpressions.Regex.IsMatch(value, "^[1-9][0-9]9[0-9]{8}$"));
            qualification.RuleFor(item => item.WebsiteUrl).NotNull().MaximumLength(2048).Must(IsWebUrl);
            qualification.RuleFor(item => item.ContactFormUrl).NotNull().MaximumLength(2048).Must(IsWebUrl);
            qualification.RuleFor(item => item.SourceUrl).NotNull().MaximumLength(2048).Must(IsWebUrl);
            qualification.RuleFor(item => item.Presence).Must(value => value is "unknown" or "website" or "social-only" or "not-located");
            qualification.RuleFor(item => item.Priority).Must(value => value is "unassessed" or "high" or "normal" or "low");
            qualification.RuleFor(item => item.Confidence).Must(value => value is "pending" or "confirmed" or "inconclusive");
            qualification.RuleFor(item => item.Tags).NotNull().Must(tags => tags is { Length: <= 12 });
            qualification.RuleForEach(item => item.Tags).NotEmpty().MaximumLength(60);
            qualification.RuleFor(item => item.PortfolioReference).NotNull().MaximumLength(120);
            qualification.RuleFor(item => item.Facts).NotNull().MaximumLength(1500);
            qualification.RuleFor(item => item.Positives).NotNull().MaximumLength(1500);
            qualification.RuleFor(item => item.Observations).NotNull().MaximumLength(1500);
            qualification.RuleFor(item => item.Questions).NotNull().MaximumLength(1500);
            qualification.RuleFor(item => item.Approach).NotNull().MaximumLength(3000);
            qualification.RuleFor(item => item.ApproachStatus).Must(value => value is "draft" or "reviewed");
        }));
    }
}
public sealed class UpdateLeadQualificationMediatorValidator : AbstractValidator<UpdateLeadQualificationMediatorCommand>
{
    public UpdateLeadQualificationMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateLeadQualificationValidator()).OverridePropertyName(string.Empty);
    }
}
