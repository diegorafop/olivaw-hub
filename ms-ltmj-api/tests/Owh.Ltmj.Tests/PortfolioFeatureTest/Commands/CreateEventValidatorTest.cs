using System;
using FluentValidation.TestHelper;
using Xunit;
using Owh.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class CreateEventValidatorTest
{
    [Fact]
    public void Validate_should_accept_valid_request()
    {
        new CreateEventValidator().TestValidate(new CreateEventCommand { Title = "Evento", Description = "Descrição", Category = "Casamentos" }).ShouldNotHaveAnyValidationErrors();
    }
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_should_require_title_and_description(string value)
    {
        var request = new CreateEventCommand { Title = "Evento", Description = "Descrição", Category = "Casamentos" }; request.Title = value; request.Description = value;
        var result = new CreateEventValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Title);
        result.ShouldHaveValidationErrorFor(item => item.Description);
    }
    [Fact]
    public void Validate_should_reject_unknown_category_and_excessive_lengths()
    {
        var request = new CreateEventCommand { Title = "Evento", Description = "Descrição", Category = "Casamentos" }; request.Category = "Outra";
        request.Title = new string('a', 151); request.Description = new string('a', 4001);
        var result = new CreateEventValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Title);
        result.ShouldHaveValidationErrorFor(item => item.Description);
        result.ShouldHaveValidationErrorFor(item => item.Category);
    }
}
