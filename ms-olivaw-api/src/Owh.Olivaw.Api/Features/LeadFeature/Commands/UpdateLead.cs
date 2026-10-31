using System.Security.Cryptography;
using System.Text;
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

public sealed class UpdateLeadMediatorCommand : IRequest<UpdateLeadResult>
{
    public Guid ExternalId { get; set; }
    public string WriteKey { get; set; } = string.Empty;
    public UpdateLeadCommand Payload { get; set; } = new();
}
public enum LeadWriteOutcome { Saved, Forbidden, Conflict }
public sealed class UpdateLeadResult
{
    public LeadWriteOutcome Outcome { get; set; }
    public UpdateLeadResponse Response { get; set; } = new();
}
public sealed class UpdateLeadHandler(IRepository<Lead> repository) : IRequestHandler<UpdateLeadMediatorCommand, UpdateLeadResult>
{
    private readonly IRepository<Lead> _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<UpdateLeadResult> Handle(UpdateLeadMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await _repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.WriteKey));
        if (lead != null)
        {
            var previous = JsonSerializer.Deserialize<UpdateLeadCommand>(lead.Snapshot, JsonOptions);
            request.Payload.Acquisition = previous?.Acquisition;
            request.Payload.Proposal = previous?.Proposal;
        }
        var snapshot = JsonSerializer.Serialize(request.Payload, JsonOptions);
        var snapshotValidator = new InlineValidator<string>();
        snapshotValidator.RuleFor(item => item).MaximumLength(32000).OverridePropertyName("Snapshot");
        await snapshotValidator.ValidateAndThrowAsync(snapshot, cancellationToken);
        if (lead != null)
        {
            if (!CryptographicOperations.FixedTimeEquals(hash, Convert.FromHexString(lead.WriteKeyHash)))
            {
                return new UpdateLeadResult { Outcome = LeadWriteOutcome.Forbidden };
            }
            if (request.Payload.Revision < lead.Revision || (request.Payload.Revision == lead.Revision && snapshot != lead.Snapshot))
            {
                return new UpdateLeadResult { Outcome = LeadWriteOutcome.Conflict };
            }
            if (request.Payload.Revision == lead.Revision)
            {
                return Saved(lead);
            }
            lead.Apply(request.Payload.Revision, snapshot, DateTime.UtcNow);
            await _repository.UpdateAsync(lead, cancellationToken);
        }
        else
        {
            lead = Lead.Create(request.ExternalId, Convert.ToHexString(hash), request.Payload.Revision, snapshot, DateTime.UtcNow);
            await _repository.AddAsync(lead, cancellationToken);
        }
        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new UpdateLeadResult { Outcome = LeadWriteOutcome.Conflict };
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" }
            or SqlException { Number: 2601 or 2627 })
        {
            return new UpdateLeadResult { Outcome = LeadWriteOutcome.Conflict };
        }
        return Saved(lead);
    }

    private static UpdateLeadResult Saved(Lead lead) => new()
    {
        Outcome = LeadWriteOutcome.Saved,
        Response = new UpdateLeadResponse { ExternalId = lead.ExternalId, Revision = lead.Revision, SavedAt = lead.UpdatedAt ?? lead.CreatedAt }
    };
}

public sealed class UpdateLeadValidator : AbstractValidator<UpdateLeadCommand>
{
    private static bool HasContact(UpdateLeadCommand item)
    {
        if (!string.IsNullOrWhiteSpace(item.Email)) return true;
        return !string.IsNullOrWhiteSpace(item.Whatsapp);
    }

    public UpdateLeadValidator()
    {
        RuleFor(item => item.Acquisition).Null();
        RuleFor(item => item.Proposal).Null();
        RuleFor(item => item.SchemaVersion).Equal(1);
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.StartedAt).NotEmpty();
        RuleFor(item => item.Entry).Must(value => value is "studio" or "discovery");
        RuleFor(item => item.Stage).Equal("submitted");
        RuleFor(item => item.Email).MaximumLength(254).EmailAddress()
            .When(item => !string.IsNullOrWhiteSpace(item.Email));
        RuleFor(item => item.Whatsapp).Matches("^[1-9][0-9]9[0-9]{8}$")
            .When(item => !string.IsNullOrEmpty(item.Whatsapp));
        RuleFor(item => item).Must(HasContact)
            .WithMessage("Informe ao menos um contato: e-mail ou WhatsApp com DDD.");
        RuleFor(item => item.Business).NotNull().MaximumLength(120);
        RuleFor(item => item.Requirements).NotNull().MaximumLength(3000);
        RuleFor(item => item.Plan).Must(value => value is null or "Bronze" or "Prata" or "Ouro" or "Diamante");
        RuleFor(item => item.Excluded).NotNull().Must(items => items is { Length: <= 12 });
        RuleForEach(item => item.Excluded).NotEmpty().MaximumLength(160);
        RuleFor(item => item.Events).NotNull().Must(items => items is { Length: <= 40 });
        RuleForEach(item => item.Events).NotNull().ChildRules(events =>
        {
            events.RuleFor(item => item.Name).NotEmpty().MaximumLength(40);
            events.RuleFor(item => item.Value).NotNull().MaximumLength(160);
            events.RuleFor(item => item.At).NotEmpty();
        });
        RuleFor(item => item.Project).NotNull();
        When(item => item.Project != null, () => RuleFor(item => item.Project!).ChildRules(project =>
        {
            project.RuleFor(item => item.Source).Must(value => value is "studio" or "discovery");
            project.RuleFor(item => item.Title).NotEmpty().MaximumLength(160);
            project.RuleFor(item => item.Context).NotNull().MaximumLength(1000);
            project.RuleFor(item => item.PresetId).MaximumLength(40);
            project.RuleFor(item => item.Choices).NotNull().Must(items => items is { Length: <= 12 });
            project.RuleForEach(item => item.Choices).NotEmpty().MaximumLength(160);
            project.RuleFor(item => item.BlockIds).NotNull().Must(items => items is { Length: <= 12 });
            project.RuleForEach(item => item.BlockIds).NotEmpty().MaximumLength(40);
        }));
        RuleFor(item => item).Must(item => JsonSerializer.Serialize(item).Length <= 32000).WithMessage("O resumo excede o tamanho permitido.");
    }
}
public sealed class UpdateLeadMediatorValidator : AbstractValidator<UpdateLeadMediatorCommand>
{
    public UpdateLeadMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.WriteKey).NotEmpty().Matches("^[a-f0-9]{64}$");
        RuleFor(item => item.Payload).NotNull();
        RuleFor(item => item.Payload).SetValidator(new UpdateLeadValidator()).OverridePropertyName(string.Empty);
    }
}
