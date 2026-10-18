using System;
using FluentValidation.TestHelper;
using Xunit;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UpdateEventVisibilityValidatorTest
{
    [Fact]
    public void Validate_should_require_id()
    {
        new UpdateEventVisibilityValidator().TestValidate(new UpdateEventVisibilityCommand()).ShouldHaveValidationErrorFor(item => item.Id);
        new UpdateEventVisibilityValidator().TestValidate(new UpdateEventVisibilityCommand { Id = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
