using System.Security.Cryptography;
using System.Text.Json;
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

public sealed class CreateLeadMediatorCommand : IRequest<CreateLeadResponse>
{
    public CreateLeadCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
}
public sealed class CreateLeadHandler(IRepository<Lead> repository) : IRequestHandler<CreateLeadMediatorCommand, CreateLeadResponse>
{
    public async Task<CreateLeadResponse> Handle(CreateLeadMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var existing = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == payload.ExternalId, cancellationToken);
        if (existing != null) return new() { ExternalId = payload.ExternalId, Conflict = true };
        var now = DateTime.UtcNow;
        var snapshot = new UpdateLeadCommand
        {
            SchemaVersion = 1, Revision = 1, StartedAt = now, Stage = "submitted",
            Entry = payload.Initiative == "outbound" ? "prospecting" : "manual",
            Business = payload.Business, Email = payload.Email, Whatsapp = payload.Whatsapp,
            Requirements = payload.Requirements,
            Project = new() { Source = "discovery", Title = payload.Title },
            Acquisition = new()
            {
                Initiative = payload.Initiative, Origin = payload.Origin,
                ExternalReference = payload.ExternalReference, SourceUrl = payload.SourceUrl
            }
        };
        var lead = Lead.Create(payload.ExternalId, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), 1,
            JsonSerializer.Serialize(snapshot, new JsonSerializerOptions(JsonSerializerDefaults.Web)), now);
        lead.ActorUserId = request.ActorUserId;
        await repository.AddAsync(lead, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" }
            or SqlException { Number: 2601 or 2627 })
        {
            return new() { ExternalId = payload.ExternalId, Conflict = true };
        }
        return new() { ExternalId = lead.ExternalId };
    }
}
public sealed class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.Initiative).Must(value => value is "inbound" or "outbound");
        RuleFor(item => item.Origin).NotEmpty().MaximumLength(120);
        RuleFor(item => item.ExternalReference).NotNull().MaximumLength(120);
        RuleFor(item => item.SourceUrl).NotNull().MaximumLength(1000)
            .Must(value => value == "" || (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http"));
        RuleFor(item => item.Title).NotEmpty().MaximumLength(160);
        RuleFor(item => item.Business).NotNull().MaximumLength(120);
        RuleFor(item => item.Requirements).NotEmpty().MaximumLength(10000);
        RuleFor(item => item.Email).NotNull().MaximumLength(254).EmailAddress().When(item => !string.IsNullOrEmpty(item.Email));
        RuleFor(item => item.Whatsapp).Matches("^[1-9][0-9]9[0-9]{8}$").When(item => !string.IsNullOrEmpty(item.Whatsapp));
    }
}
public sealed class CreateLeadMediatorValidator : AbstractValidator<CreateLeadMediatorCommand>
{
    public CreateLeadMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateLeadValidator()).OverridePropertyName(string.Empty);
    }
}
