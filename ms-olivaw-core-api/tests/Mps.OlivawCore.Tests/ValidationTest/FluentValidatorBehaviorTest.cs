using FluentValidation;
using MediatR;
using Mps.OlivawCore.Api.Validation;
using Xunit;

namespace Mps.OlivawCore.Tests.ValidationTest;

public class FluentValidatorBehaviorTest
{
    [Fact]
    public async Task Handle_should_validate_before_trimming_and_forward_token_and_result()
    {
        using var source = new CancellationTokenSource();
        var request = new Request { Name = "  OlivawCore  " };
        var validator = new InlineValidator<Request>();
        validator.RuleFor(item => item.Name).MustAsync((name, token) =>
        {
            Assert.Equal("  OlivawCore  ", name);
            Assert.Equal(source.Token, token);
            return Task.FromResult(true);
        });
        var behavior = new FluentValidatorBehavior<Request, string>([validator]);
        var result = await behavior.Handle(request, token =>
        {
            Assert.Equal(source.Token, token);
            Assert.Equal("OlivawCore", request.Name);
            Assert.Null(request.Optional);
            Assert.Equal(7, request.Count);
            return Task.FromResult("handled");
        }, source.Token);
        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_should_aggregate_validation_errors_without_calling_next_or_mutating_request()
    {
        var first = new InlineValidator<Request>();
        first.RuleFor(item => item.Name).Must(item => false).WithMessage("first");
        var second = new InlineValidator<Request>();
        second.RuleFor(item => item.Count).Must(item => false).WithMessage("second");
        var behavior = new FluentValidatorBehavior<Request, string>([first, second]);
        var request = new Request { Name = "  invalid  " };
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(request,
            token => throw new InvalidOperationException("Next must not run"), TestContext.Current.CancellationToken));
        Assert.Equal(new[] { "first", "second" }, exception.Errors.Select(item => item.ErrorMessage));
        Assert.Equal("  invalid  ", request.Name);
    }

    [Fact]
    public async Task Handle_should_work_without_validators_and_propagate_cancellation()
    {
        var behavior = new FluentValidatorBehavior<Request, string>([]);
        Assert.Equal("ok", await behavior.Handle(new Request(), token => Task.FromResult("ok"), TestContext.Current.CancellationToken));
        using var source = new CancellationTokenSource();
        source.Cancel();
        var validator = new InlineValidator<Request>();
        validator.RuleFor(item => item.Name).NotEmpty();
        var withValidator = new FluentValidatorBehavior<Request, string>([validator]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => withValidator.Handle(new Request(),
            token => throw new InvalidOperationException("Next must not run"), source.Token));
        Assert.Throws<ArgumentNullException>(() => new FluentValidatorBehavior<Request, string>(null!));
    }

    public class Request : IRequest<string>
    {
        public string Name { get; set; } = "name";
        public string? Optional { get; set; }
        public int Count { get; set; } = 7;
    }
}
