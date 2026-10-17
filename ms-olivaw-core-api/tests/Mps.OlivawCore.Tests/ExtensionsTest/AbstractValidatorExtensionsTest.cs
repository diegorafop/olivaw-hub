using FluentValidation;
using FluentValidation.TestHelper;
using Mps.OlivawCore.Api.Extensions.Validations;
using System.Linq.Expressions;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class AbstractValidatorExtensionsTest
{
    [Theory]
    [InlineData("NotFoundFor", "não encontrado")]
    [InlineData("DuplicateKeyFor", "já cadastrado")]
    [InlineData("NotExpired", "expirado")]
    [InlineData("NotConfirmed", "já foi confirmado")]
    [InlineData("EqualsToExpected", "diferente do valor esperado")]
    public void Predicate_rules_should_use_the_supplied_condition_and_shared_message(string rule, string message)
    {
        var request = new Sample { Name = "invalid" };
        var validator = new InlineValidator<Sample>();
        var builder = validator.RuleFor(item => item.Name);
        switch (rule)
        {
            case "NotFoundFor": builder.NotFoundFor(item => item.Name, request, name => name == "valid"); break;
            case "DuplicateKeyFor": builder.DuplicateKeyFor(item => item.Name, request, name => name == "valid"); break;
            case "NotExpired": builder.NotExpired(item => item.Name, request, name => name == "valid"); break;
            case "NotConfirmed": builder.NotConfirmed(item => item.Name, request, name => name == "valid"); break;
            default: builder.EqualsToExpected(item => item.Name, request, name => name == "valid"); break;
        }
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Name);
        Assert.Contains(message, Assert.Single(result.Errors).ErrorMessage);
        validator.TestValidate(new Sample { Name = "valid" }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("value", true)]
    public void NotEmptyWhenNotNull_should_allow_omission_but_reject_blank_values(string? name, bool valid)
    {
        var validator = new InlineValidator<Sample>();
        validator.RuleFor(item => item.Name).NotEmptyWhenNotNull(item => item.Name);
        Assert.Equal(valid, validator.Validate(new Sample { Name = name! }).IsValid);
    }

    [Fact]
    public void NotEmpty_and_NotNull_should_return_specific_field_errors()
    {
        var validator = new InlineValidator<Sample>();
        Expression<Func<Sample, string>> uniqueName = item => item.UniqueName;
        RuleBuilderExtensions.NotEmpty(validator.RuleFor(uniqueName), item => item.UniqueName);
        Expression<Func<Sample, string>> confirmation = item => item.UniqueConfirmation;
        RuleBuilderExtensions.NotNull(
            validator.RuleFor(confirmation),
            item => item.UniqueConfirmation);
        var result = validator.TestValidate(new Sample { UniqueName = "", UniqueConfirmation = null! });
        result.ShouldHaveValidationErrorFor(item => item.UniqueName).WithErrorMessage("Campo 'Sample.UniqueName' está vazio.");
        result.ShouldHaveValidationErrorFor(item => item.UniqueConfirmation).WithErrorMessage("Campo 'Sample.UniqueConfirmation' é nulo.");
    }

    [Theory]
    [InlineData("user@example.test", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void EmailAddress_should_validate_format_and_require_content(string value, bool valid)
    {
        var validator = new InlineValidator<Sample>();
        validator.RuleFor(item => item.Name).EmailAddress(item => item.Name);
        Assert.Equal(valid, validator.Validate(new Sample { Name = value }).IsValid);
    }

    [Fact]
    public void IsInEnum_should_reject_undefined_values()
    {
        var validator = new InlineValidator<Sample>();
        validator.RuleFor(item => item.Day).IsInEnum(item => item.Day);
        validator.TestValidate(new Sample { Day = (DayOfWeek)99 }).ShouldHaveValidationErrorFor(item => item.Day);
        validator.TestValidate(new Sample { Day = DayOfWeek.Monday }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Equal_should_compare_two_properties()
    {
        var validator = new InlineValidator<Sample>();
        validator.RuleFor(item => item.Name).Equal(item => item.Name, item => item.Confirmation);
        validator.TestValidate(new Sample { Name = "a", Confirmation = "b" }).ShouldHaveValidationErrorFor(item => item.Name);
        validator.TestValidate(new Sample { Name = "a", Confirmation = "a" }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GreaterThanMinimum_should_use_the_date_predicate()
    {
        var minimum = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var validator = new InlineValidator<Sample>();
        validator.RuleFor(item => item.Date).GreaterThanMinimum(item => item.Date, date => date > minimum, minimum);
        validator.TestValidate(new Sample { Date = minimum }).ShouldHaveValidationErrorFor(item => item.Date);
        validator.TestValidate(new Sample { Date = minimum.AddDays(1) }).ShouldNotHaveAnyValidationErrors();
    }

    public class Sample
    {
        public string Name { get; set; } = "";
        public string UniqueName { get; set; } = "";
        public string Confirmation { get; set; } = "";
        public string UniqueConfirmation { get; set; } = "";
        public DayOfWeek Day { get; set; }
        public DateTime Date { get; set; }
    }
}