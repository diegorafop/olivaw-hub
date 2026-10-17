using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoTest
{
    private readonly Mock<IRepository<Usuario>> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IServiceDocumento> _serviceMock = new(MockBehavior.Strict);
    private readonly AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandHandler _handler;

    public AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoTest() => _handler = new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandHandler(_repositoryMock.Object, _serviceMock.Object);

    [Fact]
    public void Constructor_should_reject_null_dependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandHandler(null!, _serviceMock.Object));
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandHandler(_repositoryMock.Object, null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_dependency_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        Assert.Equal(MessageHelper.NullFor<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_move_users_that_completed_admission_process()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand(usuarioLogadoId);
        var userId = Guid.NewGuid();
        var users = new List<Usuario> { new Usuario { IdExterno = userId, StatusIntegracaoEnum = StatusIntegracaoEnum.PendenteProcessoAdmissao } };
        _repositoryMock.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>())).ReturnsAsync(users);
        _serviceMock.Setup(item => item.SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyAsync(It.IsAny<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery>(), CancellationToken.None)).ReturnsAsync(new[] { new SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse { UsuarioId = userId } });
        _repositoryMock.Setup(item => item.UpdateCollectionAsync(users, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Single(response);
        Assert.Equal(StatusIntegracaoEnum.Pendente, users[0].StatusIntegracaoEnum);
        Assert.Equal(usuarioLogadoId, users[0].UsuarioLogadoId);
        _repositoryMock.Verify(item => item.GetAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None, It.IsAny<Expression<Func<Usuario, object>>[]>()), Times.Once);
        _serviceMock.Verify(item => item.SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyAsync(It.IsAny<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateCollectionAsync(users, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _serviceMock.VerifyNoOtherCalls();
    }
}