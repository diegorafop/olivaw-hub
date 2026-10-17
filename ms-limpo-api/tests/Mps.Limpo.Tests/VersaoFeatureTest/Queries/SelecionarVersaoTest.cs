using FluentValidation.TestHelper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Mps.Limpo.Api.Features.VersaoFeature.Queries;
using Mps.HolosCore.Api.Helpers;
using Mps.Limpo.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Limpo.Tests.VersaoFeatureTest.Queries;

public class SelecionarVersaoTest
{
    private readonly IConfiguration _configuration;
    private readonly SelecionarVersaoQueryValidator _validator;
    private readonly SelecionarVersaoQueryHandler _handler;

    private readonly FakerBuilder _fakerBuilder;

    public SelecionarVersaoTest()
    {
        _fakerBuilder = new FakerBuilder();

        _configuration = _fakerBuilder.Configuration;
        _validator = new SelecionarVersaoQueryValidator();

        _handler = new SelecionarVersaoQueryHandler
        (
            _configuration
        );
    }

    [Fact]
    public async Task Handle_should_have_error()
    {
        SelecionarVersaoQuery? query = null;

        Task action() => _handler.Handle(query, CancellationToken.None);

        var response = await Assert.ThrowsAsync<ArgumentNullException>(action);

        Assert.Equal
        (
            new ArgumentNullException(MessageHelper
            .NullFor<SelecionarVersaoQuery>()).Message,
            response.Message
        );
    }

    [Fact]
    public async Task Handle_should_not_have_error()
    {
        var request = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoQuery
            .Generate();

        var response = await _handler.Handle
        (
            request,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.IsType<SelecionarVersaoQueryResponse>(response);
        Assert.Equal(_configuration.GetValue<string>("swagger:version"), response.Versao);
        Assert.Equal(_configuration.GetValue<string>("swagger:title"), response.Titulo);
    }

    [Fact]
    public void Should_not_have_error_all_validate()
    {
        var request = _fakerBuilder
            .VersaoFeature
            .SelecionarVersaoQuery
            .Generate();

        var response = _validator.TestValidate(request);

        response.ShouldNotHaveAnyValidationErrors();
    }
}