using System;
using FluentValidation.TestHelper;
using Xunit;
using Owh.Ltmj.Api.Features.PortfolioFeature.Queries;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest.Queries;

public class ReadEventPhotoValidatorTest
{
    [Fact]
    public void Validate_should_require_event_and_photo_ids()
    {
        var result = new ReadEventPhotoValidator().TestValidate(new ReadEventPhotoQuery());
        result.ShouldHaveValidationErrorFor(item => item.EventId); result.ShouldHaveValidationErrorFor(item => item.PhotoId);
        new ReadEventPhotoValidator().TestValidate(new ReadEventPhotoQuery { EventId = Guid.NewGuid(), PhotoId = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
