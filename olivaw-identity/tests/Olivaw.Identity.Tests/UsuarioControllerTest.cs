using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.UsuarioFeature;
using Olivaw.Identity.Api.Features.UsuarioFeature.Commands;
using Olivaw.Identity.Api.Features.UsuarioFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class UsuarioControllerTest
{
    [Fact]
    public async Task Inserir_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new InserirUsuarioCommand();
        var response = new InserirUsuarioResponse();

        InserirUsuarioCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<InserirUsuarioCommand>(), token))
            .Callback<IRequest<InserirUsuarioResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<InserirUsuarioCommand>(item)).ReturnsAsync(response);
        var result = await controller.Inserir(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<InserirUsuarioCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Atualizar_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new AtualizarUsuarioCommand();
        var response = new AtualizarUsuarioResponse();
        var id = Guid.NewGuid();
        AtualizarUsuarioCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<AtualizarUsuarioCommand>(), token))
            .Callback<IRequest<AtualizarUsuarioResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<AtualizarUsuarioCommand>(item)).ReturnsAsync(response);
        var result = await controller.Atualizar(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<AtualizarUsuarioCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Remover_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new RemoverUsuarioCommand();
        var response = new RemoverUsuarioResponse();
        var id = Guid.NewGuid();
        RemoverUsuarioCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<RemoverUsuarioCommand>(), token))
            .Callback<IRequest<RemoverUsuarioResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<RemoverUsuarioCommand>(item)).ReturnsAsync(response);
        var result = await controller.Remover(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<RemoverUsuarioCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarUsuarioByIdQuery();
        var response = new SelecionarUsuarioByIdResponse();
        var id = Guid.NewGuid();
        SelecionarUsuarioByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarUsuarioByIdQuery>(), token))
            .Callback<IRequest<SelecionarUsuarioByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarUsuarioByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarUsuarioByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new UsuarioController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarUsuarioByFilterQuery();
        var response = new SelecionarUsuarioByFilterResponse();

        SelecionarUsuarioByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarUsuarioByFilterQuery>(), token))
            .Callback<IRequest<SelecionarUsuarioByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarUsuarioByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarUsuarioByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
