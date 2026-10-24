using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Commands;
public sealed class CreateInventoryResourceMediatorCommand : IRequest<CreateInventoryResourceResult>
{
    public CreateInventoryResourceCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateInventoryResourceResult { public string? Conflict { get; set; } public CreateInventoryResourceResponse Response { get; set; } = new(); }
public sealed class CreateInventoryResourceHandler(IRepository<InventoryResource> repository) : IRequestHandler<CreateInventoryResourceMediatorCommand, CreateInventoryResourceResult>
{
    public async Task<CreateInventoryResourceResult> Handle(CreateInventoryResourceMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var data = InventoryRules.ToDomain(request.Payload);
        var now = DateTime.UtcNow;
        var key = InventoryResource.IdentityKey(data);
        if (await repository.ExistsAsync(item => item.TechnicalId == request.Payload.TechnicalId, cancellationToken)) return new() { Conflict = "Identificador técnico já cadastrado." };
        if (key != null && await repository.ExistsAsync(item => item.ProviderIdentityKey == key, cancellationToken)) return new() { Conflict = "Este recurso do provedor já está cadastrado; vincule o existente." };
        var resource = InventoryResource.Create(request.Payload.TechnicalId, data, request.ActorUserId, request.ActorName, now);
        await repository.AddAsync(resource, cancellationToken);
        if (!await InventoryRules.SaveAsync(repository, cancellationToken)) return new() { Conflict = "O inventário mudou ou o recurso já existe. Reabra a ficha e confira." };
        return new() { Response = new() { ExternalId = resource.ExternalId, Revision = resource.Revision } };
    }
}
public sealed class CreateInventoryResourceValidator : AbstractValidator<CreateInventoryResourceCommand>
{
    public CreateInventoryResourceValidator()
    {
        RuleFor(item => item.Technology).NotNull().MaximumLength(80);
        RuleFor(item => item.TechnologyVersion).NotNull().MaximumLength(40);
        RuleFor(item => item.LogicalName).NotNull().MaximumLength(120);
        RuleFor(item => item.Technology).Empty().When(item => !InventoryResourceData.SupportsTechnology(item.Type));
        RuleFor(item => item.TechnologyVersion).Empty().When(item => !InventoryResourceData.SupportsTechnology(item.Type));
        RuleFor(item => item.Technology).NotEmpty().When(item => !string.IsNullOrEmpty(item.TechnologyVersion));
        RuleFor(item => item.LogicalName).Empty().When(item => !InventoryResourceData.SupportsLogicalName(item.Type));
        RuleFor(item => item.RenewalAt).Null().When(item => item.Type != "domain");
        RuleFor(item => item.CertificateExpiresAt).Null().When(item => item.Type != "certificate");
        RuleFor(item => item.RenewalAt).Must(InventoryResourceData.IsCalendarDate);
        RuleFor(item => item.CertificateExpiresAt).Must(InventoryResourceData.IsCalendarDate);
        RuleFor(item => item.BackupIntervalHours).Null().When(item => item.Type != "backup");
        RuleFor(item => item.BackupRetentionDays).Null().When(item => item.Type != "backup");
        RuleFor(item => item.BackupIntervalHours).InclusiveBetween(1, 87600);
        RuleFor(item => item.BackupRetentionDays).InclusiveBetween(1, 36500);
        RuleFor(item => item.Name).NotNull().MaximumLength(120);
        RuleFor(item => item.Type).NotNull().MaximumLength(30);
        RuleFor(item => item.Purpose).NotNull().MaximumLength(1000);
        RuleFor(item => item.Provider).NotNull().MaximumLength(80);
        RuleFor(item => item.AccountReference).NotNull().MaximumLength(120);
        RuleFor(item => item.Region).NotNull().MaximumLength(80);
        RuleFor(item => item.ProviderResourceId).NotNull().MaximumLength(180);
        RuleFor(item => item.Owner).NotNull().MaximumLength(120);
        RuleFor(item => item.Payer).NotNull().MaximumLength(120);
        RuleFor(item => item.Operator).NotNull().MaximumLength(120);
        RuleFor(item => item.Scope).NotNull().MaximumLength(20);
        RuleFor(item => item.Status).NotNull().MaximumLength(20);
        RuleFor(item => item.Source).NotNull().MaximumLength(500);
        RuleFor(item => item.Monitoring).NotNull().MaximumLength(20);
        RuleFor(item => item.ReferenceUrl).NotNull().MaximumLength(300);
        RuleFor(item => item.CpuCapacity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.CpuCapacity).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.MemoryCapacityGiB).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.MemoryCapacityGiB).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.StorageCapacityGiB).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000000000M);
        RuleFor(item => item.StorageCapacityGiB).Must(value => !value.HasValue || decimal.Round(value.Value, 3) == value.Value);
        RuleFor(item => item.Name).NotEmpty();
        RuleFor(item => item.Type).Must(InventoryResourceData.IsType);
        RuleFor(item => item.Status).Must(InventoryResourceData.IsStatus);
        RuleFor(item => item.Scope).Must(InventoryResourceData.IsScope);
        RuleFor(item => item.Monitoring).Must(InventoryResourceData.IsMonitoring);
        RuleFor(item => item.ReferenceUrl).Must(InventoryResourceData.IsReferenceUrl);
        RuleFor(item => item.Source).NotEmpty().When(item => item.Status == "verified");
        RuleFor(item => item.Provider).NotEmpty().When(item => !string.IsNullOrEmpty(item.ProviderResourceId));
        RuleFor(item => item.TechnicalId).NotEmpty().MaximumLength(80).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}
public sealed class CreateInventoryResourceMediatorValidator : AbstractValidator<CreateInventoryResourceMediatorCommand>
{
    public CreateInventoryResourceMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateInventoryResourceValidator()).OverridePropertyName(string.Empty);
    }
}
