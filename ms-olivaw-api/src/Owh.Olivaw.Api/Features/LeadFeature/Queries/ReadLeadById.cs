using System.Text.Json;
using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Queries;

public sealed class ReadLeadByIdMediatorQuery : IRequest<ReadLeadByIdResponse>
{
    public ReadLeadByIdQuery Payload { get; set; } = new();
}
public sealed class ReadLeadByIdHandler(IRepository<Lead> repository) : IRequestHandler<ReadLeadByIdMediatorQuery, ReadLeadByIdResponse>
{
    public async Task<ReadLeadByIdResponse> Handle(ReadLeadByIdMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ExternalId && item.DeletedAt == null, cancellationToken);
        var validator = new InlineValidator<ReadLeadByIdMediatorQuery>();
        validator.RuleFor(item => lead).NotNull().OverridePropertyName("ExternalId").WithMessage(MessageHelper.NotFoundFor<Lead, Guid>(item => item.ExternalId, request.Payload.ExternalId));
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        ArgumentNullException.ThrowIfNull(lead);
        UpdateLeadCommand? snapshot = null;
        try
        {
            snapshot = JsonSerializer.Deserialize<UpdateLeadCommand>(lead.Snapshot, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException) { /* Preserve metadata without exposing an unreadable raw snapshot. */ }
        return new ReadLeadByIdResponse
        {
            ExternalId = lead.ExternalId, CreatedAt = lead.CreatedAt, UpdatedAt = lead.UpdatedAt,
            Snapshot = snapshot, ContentUnavailable = snapshot == null,
            FollowUpStatus = lead.FollowUpStatus, FollowUpRevision = lead.FollowUpRevision, FollowUpUpdatedAt = lead.FollowUpUpdatedAt
        };
    }
}
public sealed class ReadLeadByIdValidator : AbstractValidator<ReadLeadByIdQuery>
{
    public ReadLeadByIdValidator() { RuleFor(item => item.ExternalId).NotEmpty(); }
}
public sealed class ReadLeadByIdMediatorValidator : AbstractValidator<ReadLeadByIdMediatorQuery>
{
    public ReadLeadByIdMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadLeadByIdValidator()).OverridePropertyName(string.Empty);
    }
}
