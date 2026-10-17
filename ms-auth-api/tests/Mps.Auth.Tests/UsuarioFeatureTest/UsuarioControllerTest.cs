using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest;

public class UsuarioControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly UsuarioController _controller;

    public UsuarioControllerTest()
    {
        _controller = new UsuarioController(_mediatorMock.Object, ControllerTestContext.Configuration);
        ControllerTestContext.Configure(_controller);
    }

    [Fact]
    public void Constructor_should_have_error_when_mediator_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsuarioController(null!, ControllerTestContext.Configuration));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public void Constructor_should_have_error_when_configuration_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsuarioController(_mediatorMock.Object, null!));
        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public async Task Get_by_id_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() };
        var expected = new SelecionarUsuarioByIdQueryResponse();
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_filter_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioByFilterQuery();
        IEnumerable<SelecionarUsuarioByFilterQueryResponse> expected = new[] { new SelecionarUsuarioByFilterQueryResponse() };
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_verificar_should_send_request_and_return_response()
    {
        var request = new VerificarExistenciaUsuarioQuery();
        var expected = new VerificarExistenciaUsuarioQueryResponse();
        await AssertSuccess(request, expected, () => _controller.Get(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_inserir_should_send_request_and_return_response()
    {
        var request = new InserirUsuarioCommand();
        var expected = new InserirUsuarioCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_upsert_should_send_request_and_return_response()
    {
        var request = new UpsertUsuarioCommand();
        IEnumerable<UpsertUsuarioCommandResponse> expected = new[] { new UpsertUsuarioCommandResponse() };
        await AssertSuccess(request, expected, () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Post_upsert_holos_should_send_request_and_return_response()
    {
        var request = new UpsertUsuarioHolosCommand();
        var expected = new UpsertUsuarioHolosCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Post(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_atualizar_should_send_request_and_return_response()
    {
        var request = new AtualizarUsuarioCommand();
        var expected = new AtualizarUsuarioCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_perfil_should_send_request_and_return_response()
    {
        var request = new AtualizarPerfilCommand();
        var expected = new AtualizarPerfilCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_senha_should_send_request_and_return_response()
    {
        var request = new AlterarSenhaCommand();
        var expected = new AlterarSenhaCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_senha_externo_should_send_request_and_return_response()
    {
        var request = new AlterarSenhaByExternoCommand();
        var expected = new AlterarSenhaByExternoCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_perfil_senha_should_send_request_and_return_response()
    {
        var request = new AlterarPerfilSenhaCommand();
        var expected = new AlterarPerfilSenhaCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_should_send_request_and_return_response()
    {
        var request = new RemoverUsuarioCommand { Id = Guid.NewGuid() };
        var expected = new RemoverUsuarioCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Delete(request, CancellationToken.None));
    }

    [Fact]
    public async Task Alternar_should_send_request_and_return_response()
    {
        var request = new AlternarUsuarioCommand();
        var expected = new AlternarUsuarioCommandResponse();
        await AssertSuccess(request, expected, () => _controller.Alternar(request, CancellationToken.None));
    }

    [Fact]
    public async Task Recuperar_senha_should_send_request_and_return_response()
    {
        var request = new RecuperarSenhaCommand();
        var expected = new RecuperarSenhaCommandResponse();
        await AssertSuccess(request, expected, () => _controller.RecuperarSenha(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_old_should_send_request_and_return_response()
    {
        var request = new SelecionarUsuarioByIdManyQuery();
        IEnumerable<SelecionarUsuarioByIdManyQueryResponse> expected = new[] { new SelecionarUsuarioByIdManyQueryResponse() };
        await AssertSuccess(request, expected, () => _controller.GetOld(request));
    }

    [Fact]
    public async Task Put_status_integracao_should_send_request_and_return_response()
    {
        var request = new AtualizarUsuarioStatusIntegracaoEnumPendenteCommand();
        IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse> expected = new[] { new AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse() };
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Put_status_integracao_processo_admissao_should_send_request_and_return_response()
    {
        var request = new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand();
        IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse> expected = new[] { new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse() };
        await AssertSuccess(request, expected, () => _controller.Put(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_by_id_should_propagate_mediator_error()
    {
        var request = new SelecionarUsuarioByIdQuery { Id = Guid.NewGuid() };
        var expected = new InvalidOperationException("fatal error");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Get(request, CancellationToken.None));

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    private async Task AssertSuccess<TRequest, TResponse>(TRequest request, TResponse expected, Func<Task<ActionResult>> action)
        where TRequest : IRequest<TResponse>
    {
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ReturnsAsync(expected);
        var response = await action();
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}
