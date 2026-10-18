using FluentValidation.TestHelper;
using Xunit;
using Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Queries;

public class ReadEventsByFilterValidatorTest
{
    [Fact]
    public void Validate_should_accept_default_pagination() => new ReadEventsByFilterValidator().TestValidate(new ReadEventsByFilterQuery()).ShouldNotHaveAnyValidationErrors();
    [Theory]
    [InlineData(0, 0)]
    [InlineData(10001, 51)]
    public void Validate_should_bound_pagination(int page, int pageSize)
    {
        var result = new ReadEventsByFilterValidator().TestValidate(new ReadEventsByFilterQuery { Page = page, PageSize = pageSize });
        result.ShouldHaveValidationErrorFor(item => item.Page);
        result.ShouldHaveValidationErrorFor(item => item.PageSize);
    }
}
