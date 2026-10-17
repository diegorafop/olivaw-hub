using FluentValidation.TestHelper;
using Moq;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.UsuarioFeatureTest.Commands;

public class AtualizarUsuarioStatusIntegracaoEnumPendenteTest
{
    private readonly Mock<IRepository<Usuario>> _repositoryMock = new(MockBehavior.Strict);
    private readonly AtualizarUsuarioStatusIntegracaoEnumPendenteCommandHandler _handler;
    private readonly AtualizarUsuarioStatusIntegracaoEnumPendenteCommandValidator _validator = new();

    public AtualizarUsuarioStatusIntegracaoEnumPendenteTest() => _handler = new AtualizarUsuarioStatusIntegracaoEnumPendenteCommandHandler(_repositoryMock.Object);

    [Fact]
    public void Constructor_should_reject_null_repository()
    {
        Assert.Throws<ArgumentNullException>(() => new AtualizarUsuarioStatusIntegracaoEnumPendenteCommandHandler(null!));
    }

    [Fact]
    public async Task Handle_should_reject_null_request_without_repository_calls()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));

        Assert.Equal(MessageHelper.NullFor<AtualizarUsuarioStatusIntegracaoEnumPendenteCommand>(), exception.ParamName);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_move_pending_users_to_processing()
    {
        var usuarioLogadoId = Guid.NewGuid();
        var request = new AtualizarUsuarioStatusIntegracaoEnumPendenteCommand(usuarioLogadoId);
        var users = new List<Usuario>
        {
            new Usuario { Id = 1, IdExterno = Guid.NewGuid(), IntegracaoId = "101", StatusIntegracaoEnum = StatusIntegracaoEnum.Pendente },
            new Usuario { Id = 2, IdExterno = Guid.NewGuid(), IntegracaoId = "202", StatusIntegracaoEnum = StatusIntegracaoEnum.Pendente }
        };
        _repositoryMock.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None)).ReturnsAsync(users);
        _repositoryMock.Setup(item => item.UpdateCollectionAsync(users, CancellationToken.None)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(2);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, System.Linq.Enumerable.Count(response));
        foreach (var user in users)
        {
            Assert.Equal(StatusIntegracaoEnum.Processando, user.StatusIntegracaoEnum);
            Assert.Equal(usuarioLogadoId, user.UsuarioLogadoId);
        }
        _repositoryMock.Verify(item => item.GetAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.UpdateCollectionAsync(users, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(item => item.SaveChangesAsync(CancellationToken.None), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Validate_should_reject_empty_logged_user()
    {
        var result = _validator.TestValidate(new AtualizarUsuarioStatusIntegracaoEnumPendenteCommand());

        Assert.NotEmpty(result.Errors);
    }
}