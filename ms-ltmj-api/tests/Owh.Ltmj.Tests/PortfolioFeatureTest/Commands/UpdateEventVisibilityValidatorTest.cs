using System;
using FluentValidation.TestHelper;
using Xunit;
using Owh.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UpdateEventVisibilityValidatorTest
{
    [Fact]
    public void Validate_should_require_id()
    {
        new UpdateEventVisibilityValidator().TestValidate(new UpdateEventVisibilityCommand()).ShouldHaveValidationErrorFor(item => item.Id);
        new UpdateEventVisibilityValidator().TestValidate(new UpdateEventVisibilityCommand { Id = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
