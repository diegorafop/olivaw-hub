using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.PerfilAcessoFeature;
using Olivaw.Identity.Api.Features.PerfilAcessoFeature.Commands;
using Olivaw.Identity.Api.Features.PerfilAcessoFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class PerfilAcessoControllerTest
{
    [Fact]
    public async Task Inserir_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new InserirPerfilAcessoCommand();
        var response = new InserirPerfilAcessoResponse();

        InserirPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<InserirPerfilAcessoCommand>(), token))
            .Callback<IRequest<InserirPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<InserirPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Inserir(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<InserirPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Atualizar_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new AtualizarPerfilAcessoCommand();
        var response = new AtualizarPerfilAcessoResponse();
        var id = Guid.NewGuid();
        AtualizarPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<AtualizarPerfilAcessoCommand>(), token))
            .Callback<IRequest<AtualizarPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<AtualizarPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Atualizar(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<AtualizarPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Remover_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new RemoverPerfilAcessoCommand();
        var response = new RemoverPerfilAcessoResponse();
        var id = Guid.NewGuid();
        RemoverPerfilAcessoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<RemoverPerfilAcessoCommand>(), token))
            .Callback<IRequest<RemoverPerfilAcessoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<RemoverPerfilAcessoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Remover(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<RemoverPerfilAcessoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarPerfilAcessoByIdQuery();
        var response = new SelecionarPerfilAcessoByIdResponse();
        var id = Guid.NewGuid();
        SelecionarPerfilAcessoByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarPerfilAcessoByIdQuery>(), token))
            .Callback<IRequest<SelecionarPerfilAcessoByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarPerfilAcessoByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarPerfilAcessoByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarPerfilAcessoByFilterQuery();
        var response = new SelecionarPerfilAcessoByFilterResponse();

        SelecionarPerfilAcessoByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarPerfilAcessoByFilterQuery>(), token))
            .Callback<IRequest<SelecionarPerfilAcessoByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarPerfilAcessoByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarPerfilAcessoByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
