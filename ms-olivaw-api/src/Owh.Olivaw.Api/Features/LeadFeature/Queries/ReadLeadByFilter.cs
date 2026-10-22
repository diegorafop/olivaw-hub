using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.LeadFeature.Queries;

public sealed class ReadLeadByFilterMediatorQuery : IRequest<ReadLeadByFilterResponse>
{
    public ReadLeadByFilterQuery Payload { get; set; } = new();
}
public sealed class ReadLeadByFilterHandler(IRepositoryLead repository) : IRequestHandler<ReadLeadByFilterMediatorQuery, ReadLeadByFilterResponse>
{
    public async Task<ReadLeadByFilterResponse> Handle(ReadLeadByFilterMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var payload = request.Payload;
        var result = await repository.ReadAsync(new LeadReadFilter
        {
            Page = payload.Page, PageSize = payload.PageSize, Search = payload.Search, Entry = payload.Entry,
            From = payload.From?.UtcDateTime, Until = payload.Until?.UtcDateTime
        }, cancellationToken);
        return new ReadLeadByFilterResponse
        {
            Page = payload.Page, PageSize = payload.PageSize, Total = result.Total, AsOf = DateTimeOffset.UtcNow,
            Items = result.Items.Select(item => new LeadListItem
            {
                ExternalId = item.ExternalId, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt,
                Business = item.Business, Title = item.Title, Entry = item.Entry, Email = item.Email, Whatsapp = item.Whatsapp
            }).ToList()
        };
    }
}
public sealed class ReadLeadByFilterValidator : AbstractValidator<ReadLeadByFilterQuery>
{
    public ReadLeadByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 100000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
        RuleFor(item => item.Search).MaximumLength(100);
        RuleFor(item => item.Entry).Must(value => value is null or "" or "studio" or "discovery");
        RuleFor(item => item.Until).GreaterThan(item => item.From)
            .When(item => item.From.HasValue && item.Until.HasValue);
    }
}
public sealed class ReadLeadByFilterMediatorValidator : AbstractValidator<ReadLeadByFilterMediatorQuery>
{
    public ReadLeadByFilterMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadLeadByFilterValidator()).OverridePropertyName(string.Empty);
    }
}
