using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Queries;
public sealed class ReadInventoryResourceByIdMediatorQuery : IRequest<ReadInventoryResourceByIdResponse> { public ReadInventoryResourceByIdQuery Payload { get; set; } = new(); }
public sealed class ReadInventoryResourceByIdHandler(IRepository<InventoryResource> repository, IRepositoryInventoryResource inventory, IRepository<InventoryResourceChange> changes) : IRequestHandler<ReadInventoryResourceByIdMediatorQuery, ReadInventoryResourceByIdResponse>
{
    public async Task<ReadInventoryResourceByIdResponse> Handle(ReadInventoryResourceByIdMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var resource = await InventoryRules.RequireAsync(repository, request.Payload.ExternalId, cancellationToken);
        var context = await inventory.ReadContextAsync(resource.Id, cancellationToken);
        InventoryRules.RequireGraph(context.LimitExceeded);
        var history = await changes.TakeOrderedAsync(item => item.InventoryResourceId == resource.Id && item.DeletedAt == null, 20, item => item.Revision, cancellationToken);
        return new()
        {
            ExternalId = resource.ExternalId, TechnicalId = resource.TechnicalId, Revision = resource.Revision,
            ProviderIdentityLocked = resource.ProviderIdentityKey != null, VerifiedAt = resource.VerifiedAt,
            Name = resource.Name,
            Type = resource.Type,
            Purpose = resource.Purpose,
            Provider = resource.Provider,
            AccountReference = resource.AccountReference,
            Region = resource.Region,
            ProviderResourceId = resource.ProviderResourceId,
            Owner = resource.Owner,
            Payer = resource.Payer,
            Operator = resource.Operator,
            Scope = resource.Scope,
            Status = resource.Status,
            Source = resource.Source,
            Monitoring = resource.Monitoring,
            ReferenceUrl = resource.ReferenceUrl,
            CpuCapacity = resource.CpuCapacity,
            MemoryCapacityGiB = resource.MemoryCapacityGiB,
            StorageCapacityGiB = resource.StorageCapacityGiB,
            Technology = resource.Technology,
            TechnologyVersion = resource.TechnologyVersion,
            LogicalName = resource.LogicalName,
            RenewalAt = resource.RenewalAt,
            CertificateExpiresAt = resource.CertificateExpiresAt,
            BackupIntervalHours = resource.BackupIntervalHours,
            BackupRetentionDays = resource.BackupRetentionDays,
            CustomerCount = context.CustomerCount, Customers = context.Customers.Select(item => new InventoryCustomerItem { ExternalId = item.ExternalId, Business = item.Business }).ToList(),
            ReservedCpu = context.ReservedCpu, ReservedMemoryGiB = context.ReservedMemoryGiB, ReservedStorageGiB = context.ReservedStorageGiB, IncompleteReservations = context.IncompleteReservations,
            OriginCount = context.OriginCount, Origins = context.Origins.Select(InventoryReadMapping.Origin).ToList(),
            History = history.Select(item => new InventoryResourceHistoryItem
            {
                Revision = item.Revision, ActorName = item.ActorName, CreatedAt = item.CreatedAt, VerifiedAt = item.VerifiedAt,
                Name = item.Name,
                Type = item.Type,
                Purpose = item.Purpose,
                Provider = item.Provider,
                AccountReference = item.AccountReference,
                Region = item.Region,
                ProviderResourceId = item.ProviderResourceId,
                Owner = item.Owner,
                Payer = item.Payer,
                Operator = item.Operator,
                Scope = item.Scope,
                Status = item.Status,
                Source = item.Source,
                Monitoring = item.Monitoring,
                ReferenceUrl = item.ReferenceUrl,
                CpuCapacity = item.CpuCapacity,
                MemoryCapacityGiB = item.MemoryCapacityGiB,
                StorageCapacityGiB = item.StorageCapacityGiB,
                Technology = item.Technology,
                TechnologyVersion = item.TechnologyVersion,
                LogicalName = item.LogicalName,
                RenewalAt = item.RenewalAt,
                CertificateExpiresAt = item.CertificateExpiresAt,
                BackupIntervalHours = item.BackupIntervalHours,
                BackupRetentionDays = item.BackupRetentionDays,
            }).ToList()
        };
    }
}
public sealed class ReadInventoryResourceByIdValidator : AbstractValidator<ReadInventoryResourceByIdQuery>
{
    public ReadInventoryResourceByIdValidator() { RuleFor(item => item.ExternalId).NotEmpty(); }
}
public sealed class ReadInventoryResourceByIdMediatorValidator : AbstractValidator<ReadInventoryResourceByIdMediatorQuery>
{
    public ReadInventoryResourceByIdMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadInventoryResourceByIdValidator()).OverridePropertyName(string.Empty); }
}
