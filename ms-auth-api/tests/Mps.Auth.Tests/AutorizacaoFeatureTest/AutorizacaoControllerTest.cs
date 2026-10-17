using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Mps.Auth.Api.Features.AutorizacaoFeature;
using Mps.Auth.Api.Features.AutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.Auth.Tests.Mocks;
using Mps.HolosCore.Api.Extensions.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.AutorizacaoFeatureTest;

public class AutorizacaoControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly FakerBuilder _fakerBuilder = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["authentication:oidc:standard-claims:sub"] = "usuario_teste"
        }).Build();
    private readonly AutorizacaoController _controller;
    private readonly Guid _usuarioId = Guid.Parse("b8f00faa-ae91-4fc9-a1cd-7fb12770c90a");

    public AutorizacaoControllerTest()
    {
        _controller = new AutorizacaoController(_mediatorMock.Object, _configuration);
        var token = new JwtSecurityToken(claims:
        [
            new Claim("usuario_teste", _usuarioId.ToString())
        ]);
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {new JwtSecurityTokenHandler().WriteToken(token)}";
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [Fact]
    public void Constructor_should_reject_null_mediator()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AutorizacaoController(null!, _configuration)
        );

        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_reject_null_configuration()
    {
        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new AutorizacaoController(_mediatorMock.Object, null!)
        );

        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public async Task Get_should_send_SelecionarAutorizacaoByIdQuery_and_return_ok()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        SelecionarAutorizacaoByIdQueryResponse expected = new() { Tipo = "role", Valor = "PerfilAcesso" };
        using var cancellation = new CancellationTokenSource();
        _mediatorMock.Setup(item => item.Send(request, cancellation.Token))
            .ReturnsAsync(expected);

        var response = await _controller.Get(request, cancellation.Token);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, cancellation.Token), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_should_propagate_failure_for_SelecionarAutorizacaoByIdQuery()
    {
        var request = _fakerBuilder.AutorizacaoFeature.SelecionarAutorizacaoByIdQuery.Generate();
        var expected = new InvalidOperationException("Falha no mediator.");
        using var cancellation = new CancellationTokenSource();
        _mediatorMock.Setup(item => item.Send(request, cancellation.Token)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Get(request, cancellation.Token)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, cancellation.Token), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_should_send_SelecionarAutorizacaoByFilterQuery_and_return_ok()
    {
        var request = new SelecionarAutorizacaoByFilterQuery { Tipo = "role", Valor = "PerfilAcesso" };
        IEnumerable<SelecionarAutorizacaoByFilterQueryResponse> expected = [new SelecionarAutorizacaoByFilterQueryResponse { Tipo = "role", Valor = "PerfilAcesso" }];
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None))
            .ReturnsAsync(expected);

        var response = await _controller.Get(request);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_should_propagate_failure_for_SelecionarAutorizacaoByFilterQuery()
    {
        var request = new SelecionarAutorizacaoByFilterQuery { Tipo = "role", Valor = "PerfilAcesso" };
        var expected = new InvalidOperationException("Falha no mediator.");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Get(request)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Post_should_send_InserirAutorizacaoCommand_and_return_ok()
    {
        var request = new InserirAutorizacaoCommand(Guid.Empty) { Tipo = "role", Valor = "PerfilAcesso" };
        InserirAutorizacaoCommandResponse expected = new() { Id = _usuarioId };
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None))
            .Callback(() =>
            {
                Assert.Equal
                (
                    _usuarioId,
                    EntityExtensions
                        .ToDomain<InserirAutorizacaoCommand,
                            Autorizacao>(request).UsuarioLogadoId
                );
            })
            .ReturnsAsync(expected);

        var response = await _controller.Post(request);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Post_should_propagate_failure_for_InserirAutorizacaoCommand()
    {
        var request = new InserirAutorizacaoCommand(Guid.Empty) { Tipo = "role", Valor = "PerfilAcesso" };
        var expected = new InvalidOperationException("Falha no mediator.");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Post(request)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Put_should_send_AtualizarAutorizacaoCommand_and_return_ok()
    {
        var request = new AtualizarAutorizacaoCommand(Guid.Empty) { Id = _usuarioId, Tipo = "role", Valor = "PerfilAcesso" };
        AtualizarAutorizacaoCommandResponse expected = new() { DataAtualizacao = new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc) };
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None))
            .Callback(() =>
            {
                Assert.Equal(_usuarioId, EntityExtensions.ToDomain<AtualizarAutorizacaoCommand, Autorizacao>(request).UsuarioLogadoId);
            })
            .ReturnsAsync(expected);

        var response = await _controller.Put(request);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Put_should_propagate_failure_for_AtualizarAutorizacaoCommand()
    {
        var request = new AtualizarAutorizacaoCommand(Guid.Empty) { Id = _usuarioId, Tipo = "role", Valor = "PerfilAcesso" };
        var expected = new InvalidOperationException("Falha no mediator.");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Put(request)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_send_RemoverAutorizacaoCommand_and_return_ok()
    {
        var request = new RemoverAutorizacaoCommand { Id = _usuarioId };
        RemoverAutorizacaoCommandResponse expected = new() { Id = _usuarioId };
        using var cancellation = new CancellationTokenSource();
        _mediatorMock.Setup(item => item.Send(request, cancellation.Token))
            .ReturnsAsync(expected);

        var response = await _controller.Delete(request, cancellation.Token);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, cancellation.Token), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_should_propagate_failure_for_RemoverAutorizacaoCommand()
    {
        var request = new RemoverAutorizacaoCommand { Id = _usuarioId };
        var expected = new InvalidOperationException("Falha no mediator.");
        using var cancellation = new CancellationTokenSource();
        _mediatorMock.Setup(item => item.Send(request, cancellation.Token)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => _controller.Delete(request, cancellation.Token)
        );

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, cancellation.Token), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}