using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;

public sealed class ReadManagedServiceByFilterMediatorQuery : IRequest<ReadManagedServiceByFilterResponse>
{
    public ReadManagedServiceByFilterQuery Payload { get; set; } = new();
}
public sealed class ReadManagedServiceByFilterHandler(IRepositoryManagedService repository) : IRequestHandler<ReadManagedServiceByFilterMediatorQuery, ReadManagedServiceByFilterResponse>
{
    public async Task<ReadManagedServiceByFilterResponse> Handle(ReadManagedServiceByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var page = await repository.ReadPageAsync(payload.Search, payload.Status, payload.Type, payload.CustomerExternalId, payload.Page, payload.PageSize, cancellationToken);
        return new()
        {
            Page = payload.Page, PageSize = payload.PageSize, Total = page.Total,
            Items = page.Items.Select(item => new ManagedServiceListItem
            {
                ExternalId = item.ExternalId, TechnicalId = item.TechnicalId, Name = item.Name, Type = item.Type,
                Scope = item.Scope, Status = item.Status, CustomerCount = item.CustomerCount
            }).ToList()
        };
    }
}
public sealed class ReadManagedServiceByFilterValidator : AbstractValidator<ReadManagedServiceByFilterQuery>
{
    public ReadManagedServiceByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Search).NotNull().MaximumLength(100);
        RuleFor(item => item.Status).Must(value => value == "" || ManagedService.IsStatus(value));
        RuleFor(item => item.Type).Must(value => value == "" || ManagedService.IsType(value));
        RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty).When(item => item.CustomerExternalId.HasValue);
    }
}
public sealed class ReadManagedServiceByFilterMediatorValidator : AbstractValidator<ReadManagedServiceByFilterMediatorQuery>
{
    public ReadManagedServiceByFilterMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadManagedServiceByFilterValidator()).OverridePropertyName(string.Empty);
    }
}
