using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.ReadyFeature.Queries;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using Mps.HolosCore.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.ReadyFeatureTest.Queries;

public class SelecionarReadyTest
{
    private readonly Mock<IRepository<Mps.Auth.Domain.Usuario>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IProviderAuthentication> _providerMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceDocumento> _documentoMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceUsuario> _usuarioMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceNotificacao> _notificacaoMock = new(MockBehavior.Strict);
    private readonly IConfiguration _configuration = CreateConfiguration();
    private readonly SelecionarReadyQueryValidator _validator = new();

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            null!, _providerMock.Object, _configuration, _documentoMock.Object,
            _usuarioMock.Object, _notificacaoMock.Object
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            _repositoryMock.Object, null!, _configuration, _documentoMock.Object,
            _usuarioMock.Object, _notificacaoMock.Object
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            _repositoryMock.Object, _providerMock.Object, null!, _documentoMock.Object,
            _usuarioMock.Object, _notificacaoMock.Object
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            _repositoryMock.Object, _providerMock.Object, _configuration, null!,
            _usuarioMock.Object, _notificacaoMock.Object
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            _repositoryMock.Object, _providerMock.Object, _configuration, _documentoMock.Object,
            null!, _notificacaoMock.Object
        ));
        Assert.Throws<ArgumentNullException>(() => new SelecionarReadyQueryHandler
        (
            _repositoryMock.Object, _providerMock.Object, _configuration, _documentoMock.Object,
            _usuarioMock.Object, null!
        ));
    }

    [Fact]
    public async Task Handle_should_collect_health_details_and_propagate_token()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        ConfigureHealthyDependencies(token);
        var handler = CreateHandler();

        var response = await handler.Handle(new SelecionarReadyQuery(), token);

        Assert.Equal(HttpRequestHelper.HealthStatusPass, response.Status);
        Assert.Equal(5, response.Details.Count);
        Assert.All(response.Details, item => Assert.Equal(HttpRequestHelper.HealthStatusPass, item.Status));
        _repositoryMock.Verify(item => item.CanConnectAsync(token), Times.Once);
        _providerMock.Verify(item => item.HealthAsync(token), Times.Once);
        _providerMock.VerifyGet(item => item.BaseAddress, Times.Once);
        _documentoMock.Verify(item => item.HealthAsync(token), Times.Once);
        _documentoMock.VerifyGet(item => item.BaseAddress, Times.Once);
        _usuarioMock.Verify(item => item.HealthAsync(token), Times.Once);
        _usuarioMock.VerifyGet(item => item.BaseAddress, Times.Once);
        _notificacaoMock.Verify(item => item.HealthAsync(token), Times.Once);
        _notificacaoMock.VerifyGet(item => item.BaseAddress, Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _providerMock.VerifyNoOtherCalls();
        _documentoMock.VerifyNoOtherCalls();
        _usuarioMock.VerifyNoOtherCalls();
        _notificacaoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_report_failure_when_a_dependency_is_unhealthy()
    {
        ConfigureHealthyDependencies(CancellationToken.None);
        _documentoMock.Setup(item => item.HealthAsync(CancellationToken.None)).ReturnsAsync(false);
        var handler = CreateHandler();

        var response = await handler.Handle(new SelecionarReadyQuery(), CancellationToken.None);

        Assert.Equal(HttpRequestHelper.HealthStatusFail, response.Status);
        Assert.Contains(response.Details, item => item.Status == HttpRequestHelper.HealthStatusFail);
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_calling_dependencies()
    {
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => handler.Handle(null!, CancellationToken.None)
        );

        Assert.Equal(MessageHelper.NullFor<SelecionarReadyQuery>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _providerMock.VerifyNoOtherCalls();
        _documentoMock.VerifyNoOtherCalls();
        _usuarioMock.VerifyNoOtherCalls();
        _notificacaoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_accept_request()
    {
        var result = _validator.TestValidate(new SelecionarReadyQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }

    private void ConfigureHealthyDependencies(CancellationToken token)
    {
        _repositoryMock.Setup(item => item.CanConnectAsync(token)).ReturnsAsync(true);
        _providerMock.Setup(item => item.HealthAsync(token)).ReturnsAsync(true);
        _providerMock.SetupGet(item => item.BaseAddress).Returns("https://auth.test");
        _documentoMock.Setup(item => item.HealthAsync(token)).ReturnsAsync(true);
        _documentoMock.SetupGet(item => item.BaseAddress).Returns("https://documento.test");
        _usuarioMock.Setup(item => item.HealthAsync(token)).ReturnsAsync(true);
        _usuarioMock.SetupGet(item => item.BaseAddress).Returns("https://usuario.test");
        _notificacaoMock.Setup(item => item.HealthAsync(token)).ReturnsAsync(true);
        _notificacaoMock.SetupGet(item => item.BaseAddress).Returns("https://notificacao.test");
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new Dictionary<string, string?>
                {
                    ["DatabaseProvider"] = "PostgreSql",
                    ["ConnectionStrings:DbContext"] =
                        "Host=postgres.test;Port=5432;Database=auth;SearchPath=ms_auth;User Id=test;Password=test"
                }
            )
            .Build();
    }

    private SelecionarReadyQueryHandler CreateHandler
    (
        IRepository<Mps.Auth.Domain.Usuario>? repository = null,
        IProviderAuthentication? provider = null,
        IConfiguration? configuration = null,
        IServiceDocumento? documento = null,
        IServiceUsuario? usuario = null,
        IServiceNotificacao? notificacao = null
    )
    {
        return new SelecionarReadyQueryHandler
        (
            repository ?? _repositoryMock.Object,
            provider ?? _providerMock.Object,
            configuration ?? _configuration,
            documento ?? _documentoMock.Object,
            usuario ?? _usuarioMock.Object,
            notificacao ?? _notificacaoMock.Object
        );
    }
}