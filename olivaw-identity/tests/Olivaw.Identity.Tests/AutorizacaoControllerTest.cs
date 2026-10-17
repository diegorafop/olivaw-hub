using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Features.AutorizacaoFeature;
using Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;
using Olivaw.Identity.Api.Features.AutorizacaoFeature.Queries;
using Xunit;

namespace Olivaw.Identity.Tests;

public class AutorizacaoControllerTest
{
    [Fact]
    public async Task Inserir_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new InserirAutorizacaoCommand();
        var response = new InserirAutorizacaoResponse();

        InserirAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<InserirAutorizacaoCommand>(), token))
            .Callback<IRequest<InserirAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<InserirAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Inserir(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<InserirAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Atualizar_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new AtualizarAutorizacaoCommand();
        var response = new AtualizarAutorizacaoResponse();
        var id = Guid.NewGuid();
        AtualizarAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<AtualizarAutorizacaoCommand>(), token))
            .Callback<IRequest<AtualizarAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<AtualizarAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Atualizar(id, request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<AtualizarAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Remover_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new RemoverAutorizacaoCommand();
        var response = new RemoverAutorizacaoResponse();
        var id = Guid.NewGuid();
        RemoverAutorizacaoCommand? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<RemoverAutorizacaoCommand>(), token))
            .Callback<IRequest<RemoverAutorizacaoResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<RemoverAutorizacaoCommand>(item)).ReturnsAsync(response);
        var result = await controller.Remover(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<RemoverAutorizacaoCommand>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarById_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarAutorizacaoByIdQuery();
        var response = new SelecionarAutorizacaoByIdResponse();
        var id = Guid.NewGuid();
        SelecionarAutorizacaoByIdQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarAutorizacaoByIdQuery>(), token))
            .Callback<IRequest<SelecionarAutorizacaoByIdResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarAutorizacaoByIdQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(id, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Equal(id, sent.Id);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarAutorizacaoByIdQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelecionarByFilter_should_delegate_request_and_cancellation_to_mediator()
    {
        var mediator = new Mock<IMediator>();
        var controller = new AutorizacaoController(mediator.Object);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var request = new SelecionarAutorizacaoByFilterQuery();
        var response = new SelecionarAutorizacaoByFilterResponse();

        SelecionarAutorizacaoByFilterQuery? sent = null;
        mediator.Setup(item => item.Send(It.IsAny<SelecionarAutorizacaoByFilterQuery>(), token))
            .Callback<IRequest<SelecionarAutorizacaoByFilterResponse>, CancellationToken>((item, cancellation) => sent = Assert.IsType<SelecionarAutorizacaoByFilterQuery>(item)).ReturnsAsync(response);
        var result = await controller.Selecionar(request, token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        Assert.NotNull(sent);
        Assert.Same(request, sent);
        mediator.Verify(item => item.Send(It.IsAny<SelecionarAutorizacaoByFilterQuery>(), token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

}
