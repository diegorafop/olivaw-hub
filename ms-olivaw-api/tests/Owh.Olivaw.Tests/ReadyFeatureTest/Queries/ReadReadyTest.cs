using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;
using Owh.Olivaw.Api.Features.ReadyFeature.Queries;
using Owh.Olivaw.Dados;
using Xunit;

namespace Owh.Olivaw.Tests.ReadyFeatureTest.Queries;

public sealed class ReadReadyTest
{
    [Theory]
    [InlineData(true, "pass", "connection ok")]
    [InlineData(false, "fail", "connection failure")]
    public async Task Handle_should_report_database_result_in_detail_and_overall_status(bool connected, string status, string output)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new Mock<IWriteDbContext>(MockBehavior.Strict);
        context.Setup(item => item.CanConnectAsync(cancellation.Token)).ReturnsAsync(connected);
        var handler = new ReadReadyQueryHandler(context.Object, Configuration());

        var response = await handler.Handle(new ReadReadyQuery(), cancellation.Token);

        Assert.Equal(status, response.Status);
        var detail = Assert.Single(response.Details);
        Assert.Equal(status, detail.Status);
        Assert.Equal("postgres", detail.Service);
        Assert.Equal("datastore", detail.ComponentType);
        Assert.Equal("readiness.example.test", detail.Address);
        Assert.Equal(output, detail.Output);
        Assert.True(DateTime.TryParseExact(detail.Time, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _));
        context.Verify(item => item.CanConnectAsync(cancellation.Token), Times.Once);
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_null_request_before_probing_database()
    {
        var context = new Mock<IWriteDbContext>(MockBehavior.Strict);
        var handler = new ReadReadyQueryHandler(context.Object, Configuration());

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));

        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_propagate_cancellation_without_retrying()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new Mock<IWriteDbContext>(MockBehavior.Strict);
        context.Setup(item => item.CanConnectAsync(cancellation.Token)).ThrowsAsync(new OperationCanceledException(cancellation.Token));
        var handler = new ReadReadyQueryHandler(context.Object, Configuration());

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(new(), cancellation.Token));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
        context.Verify(item => item.CanConnectAsync(cancellation.Token), Times.Once);
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_propagate_probe_error_without_reporting_a_healthy_result()
    {
        var expected = new InvalidOperationException("Simulated probe failure");
        var context = new Mock<IWriteDbContext>(MockBehavior.Strict);
        context.Setup(item => item.CanConnectAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expected);
        var handler = new ReadReadyQueryHandler(context.Object, Configuration());

        Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new(), CancellationToken.None)));
        context.Verify(item => item.CanConnectAsync(CancellationToken.None), Times.Once);
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_should_reject_missing_dependencies(bool missingContext)
    {
        if (missingContext)
        {
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => new ReadReadyQueryHandler(null!, Configuration())).ParamName);
            return;
        }
        Assert.Equal("configuration", Assert.Throws<ArgumentNullException>(() => new ReadReadyQueryHandler(Mock.Of<IWriteDbContext>(), null!)).ParamName);
    }

    [Fact]
    public void Validator_should_accept_the_empty_readiness_contract()
    {
        new ReadReadyQueryValidator().TestValidate(new ReadReadyQuery()).ShouldNotHaveAnyValidationErrors();
    }

    private static IConfiguration Configuration() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["DatabaseProvider"] = "PostgreSql",
        ["ConnectionStrings:DbContext"] = "Host=readiness.example.test;Database=unused"
    }).Build();
}
