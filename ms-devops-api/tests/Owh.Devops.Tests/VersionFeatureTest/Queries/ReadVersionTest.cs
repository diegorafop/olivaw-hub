using FluentValidation.TestHelper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Owh.Devops.Api.Features.VersionFeature.Queries;
using Owh.OlivawCore.Api.Helpers;
using Owh.Devops.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Owh.Devops.Tests.VersionFeatureTest.Queries;

public class ReadVersionTest
{
    private readonly IConfiguration _configuration;
    private readonly ReadVersionQueryValidator _validator;
    private readonly ReadVersionQueryHandler _handler;

    private readonly FakerBuilder _fakerBuilder;

    public ReadVersionTest()
    {
        _fakerBuilder = new FakerBuilder();

        _configuration = _fakerBuilder.Configuration;
        _validator = new ReadVersionQueryValidator();

        _handler = new ReadVersionQueryHandler
        (
            _configuration
        );
    }

    [Fact]
    public async Task Handle_should_have_error()
    {
        ReadVersionQuery? query = null;

        Task action() => _handler.Handle(query, CancellationToken.None);

        var response = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
            .NullFor<ReadVersionQuery>()).Message,
            response.Message
        );
    }

    [Fact]
    public async Task Handle_should_not_have_error()
    {
        var request = _fakerBuilder
            .VersionFeature
            .ReadVersionQuery
            .Generate();

        var response = await _handler.Handle
        (
            request,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.IsType<ReadVersionQueryResponse>(response);
        Assert.Equal(_configuration.GetValue<string>("swagger:version"), response.Version);
        Assert.Equal(_configuration.GetValue<string>("swagger:title"), response.Title);
    }

    [Fact]
    public void Should_not_have_error_all_validate()
    {
        var request = _fakerBuilder
            .VersionFeature
            .ReadVersionQuery
            .Generate();

        var response = _validator.TestValidate(request);

        response.ShouldNotHaveAnyValidationErrors();
    }
}