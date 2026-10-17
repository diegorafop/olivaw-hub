using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Mps.Auth.Api.Features.VersaoFeature.Queries;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.VersaoFeatureTest.Queries;

public class SelecionarVersaoExternalTest
{
    private readonly IConfiguration _configuration;
    private readonly SelecionarVersaoExternalQueryValidator _validator;
    private readonly SelecionarVersaoExternalQueryHandler _handler;

    private readonly FakerBuilder _fakerBuilder;

    public SelecionarVersaoExternalTest()
    {
        _fakerBuilder = new FakerBuilder();

        _configuration = _fakerBuilder.Configuration;
        _validator = new SelecionarVersaoExternalQueryValidator();

        _handler = new SelecionarVersaoExternalQueryHandler
        (
            _configuration
        );
    }

    [Fact]
    public async Task Handle_should_have_error()
    {
        SelecionarVersaoExternalQuery? query = null;

        Task action() => _handler.Handle(query, CancellationToken.None);

        var response = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
                .NullFor<SelecionarVersaoExternalQuery>()).Message,
            response.Message
        );
    }

    [Fact]
    public async Task Handle_should_not_have_error()
    {
        var request = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoExternalQuery
            .Generate();

        var response = await _handler.Handle
        (
            request,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.IsType<SelecionarVersaoExternalQueryResponse>(response);
        Assert.Equal(_configuration.GetValue<string>("swagger:version"), response.Versao);
        Assert.Equal(_configuration.GetValue<string>("swagger:title"), response.Titulo);
    }

    [Fact]
    public void Should_not_have_error_all_validate()
    {
        var request = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoExternalQuery
            .Generate();

        var response = _validator.TestValidate(request);

        response.ShouldNotHaveAnyValidationErrors();
    }
}