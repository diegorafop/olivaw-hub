using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature;
using Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class PerfilAcessoAutorizacaoControllerTest
{
    [Fact]
    public async Task Inserir_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoAutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new InserirPerfilAcessoAutorizacaoCommand();
        var response = new InserirPerfilAcessoAutorizacaoResponse();

        InserirPerfilAcessoAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<InserirPerfilAcessoAutorizacaoCommand>(), token))
            .Callback<IRequest<InserirPerfilAcessoAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<InserirPerfilAcessoAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Inserir(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<InserirPerfilAcessoAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Atualizar_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoAutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new AtualizarPerfilAcessoAutorizacaoCommand();
        var response = new AtualizarPerfilAcessoAutorizacaoResponse();
        var id = Guid.NewGuid();
        AtualizarPerfilAcessoAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<AtualizarPerfilAcessoAutorizacaoCommand>(), token))
            .Callback<IRequest<AtualizarPerfilAcessoAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<AtualizarPerfilAcessoAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Atualizar(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<AtualizarPerfilAcessoAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Remover_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoAutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new RemoverPerfilAcessoAutorizacaoCommand();
        var response = new RemoverPerfilAcessoAutorizacaoResponse();
        var id = Guid.NewGuid();
        RemoverPerfilAcessoAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<RemoverPerfilAcessoAutorizacaoCommand>(), token))
            .Callback<IRequest<RemoverPerfilAcessoAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<RemoverPerfilAcessoAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Remover(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<RemoverPerfilAcessoAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoAutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarPerfilAcessoAutorizacaoByIdQuery();
        var response = new SelecionarPerfilAcessoAutorizacaoByIdResponse();
        var id = Guid.NewGuid();
        SelecionarPerfilAcessoAutorizacaoByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarPerfilAcessoAutorizacaoByIdQuery>(), token))
            .Callback<IRequest<SelecionarPerfilAcessoAutorizacaoByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarPerfilAcessoAutorizacaoByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarPerfilAcessoAutorizacaoByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new PerfilAcessoAutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarPerfilAcessoAutorizacaoByFilterQuery();
        var response = new SelecionarPerfilAcessoAutorizacaoByFilterResponse();

        SelecionarPerfilAcessoAutorizacaoByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarPerfilAcessoAutorizacaoByFilterQuery>(), token))
            .Callback<IRequest<SelecionarPerfilAcessoAutorizacaoByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarPerfilAcessoAutorizacaoByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarPerfilAcessoAutorizacaoByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
