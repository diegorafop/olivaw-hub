using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature;
using Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UsuarioPerfilAcessoControllerTest
{
    [Fact]
    public async Task Inserir_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioPerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new InserirUsuarioPerfilAcessoCommand();
        var response = new InserirUsuarioPerfilAcessoResponse();

        InserirUsuarioPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<InserirUsuarioPerfilAcessoCommand>(), token))
            .Callback<IRequest<InserirUsuarioPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<InserirUsuarioPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Inserir(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<InserirUsuarioPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Atualizar_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioPerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new AtualizarUsuarioPerfilAcessoCommand();
        var response = new AtualizarUsuarioPerfilAcessoResponse();
        var id = Guid.NewGuid();
        AtualizarUsuarioPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<AtualizarUsuarioPerfilAcessoCommand>(), token))
            .Callback<IRequest<AtualizarUsuarioPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<AtualizarUsuarioPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Atualizar(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<AtualizarUsuarioPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Remover_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioPerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new RemoverUsuarioPerfilAcessoCommand();
        var response = new RemoverUsuarioPerfilAcessoResponse();
        var id = Guid.NewGuid();
        RemoverUsuarioPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<RemoverUsuarioPerfilAcessoCommand>(), token))
            .Callback<IRequest<RemoverUsuarioPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<RemoverUsuarioPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Remover(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<RemoverUsuarioPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioPerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarUsuarioPerfilAcessoByIdQuery();
        var response = new SelecionarUsuarioPerfilAcessoByIdResponse();
        var id = Guid.NewGuid();
        SelecionarUsuarioPerfilAcessoByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarUsuarioPerfilAcessoByIdQuery>(), token))
            .Callback<IRequest<SelecionarUsuarioPerfilAcessoByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarUsuarioPerfilAcessoByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarUsuarioPerfilAcessoByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioPerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarUsuarioPerfilAcessoByFilterQuery();
        var response = new SelecionarUsuarioPerfilAcessoByFilterResponse();

        SelecionarUsuarioPerfilAcessoByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarUsuarioPerfilAcessoByFilterQuery>(), token))
            .Callback<IRequest<SelecionarUsuarioPerfilAcessoByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarUsuarioPerfilAcessoByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarUsuarioPerfilAcessoByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
