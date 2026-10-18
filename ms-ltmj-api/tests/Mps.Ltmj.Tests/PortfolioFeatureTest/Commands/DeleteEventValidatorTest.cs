using System;
using FluentValidation.TestHelper;
using Xunit;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class DeleteEventValidatorTest
{
    [Fact]
    public void Validate_should_require_id()
    {
        new DeleteEventValidator().TestValidate(new DeleteEventCommand()).ShouldHaveValidationErrorFor(item => item.Id);
        new DeleteEventValidator().TestValidate(new DeleteEventCommand { Id = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
