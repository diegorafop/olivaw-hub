using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Queries;

public sealed class ReadCustomerByFilterMediatorQuery : IRequest<ReadCustomerByFilterResponse>
{
    public ReadCustomerByFilterQuery Payload { get; set; } = new();
}
public sealed class ReadCustomerByFilterHandler(IRepositoryCustomer repository) : IRequestHandler<ReadCustomerByFilterMediatorQuery, ReadCustomerByFilterResponse>
{
    public async Task<ReadCustomerByFilterResponse> Handle(ReadCustomerByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var result = await repository.ReadPageAsync(payload.Search ?? string.Empty, payload.Status ?? string.Empty, payload.Page, payload.PageSize, cancellationToken);
        return new ReadCustomerByFilterResponse
        {
            Page = payload.Page, PageSize = payload.PageSize, Total = result.Total,
            Items = result.Items.Select(item => new CustomerListItem { ExternalId = item.ExternalId, Business = item.Business, Owner = item.Owner, Status = item.Status, CreatedAt = item.CreatedAt }).ToList()
        };
    }
}
public sealed class ReadCustomerByFilterValidator : AbstractValidator<ReadCustomerByFilterQuery>
{
    public ReadCustomerByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Search).MaximumLength(100);
        RuleFor(item => item.Status).Must(value => value is null or "" or "onboarding" or "active" or "paused" or "closed");
    }
}
public sealed class ReadCustomerByFilterMediatorValidator : AbstractValidator<ReadCustomerByFilterMediatorQuery>
{
    public ReadCustomerByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadCustomerByFilterValidator()).OverridePropertyName(string.Empty); }
}
