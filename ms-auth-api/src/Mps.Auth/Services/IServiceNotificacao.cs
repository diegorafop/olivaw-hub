using Mps.Auth.Requests;
using Mps.HolosCore.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mps.Auth.Services;

public interface IServiceNotificacao : IServiceHttpClient
{
    public Task<UpsertDestinatarioCommandResponse> UpsertDestinatarioAsync
    (
        UpsertDestinatarioCommand request,
        CancellationToken cancellationToken
    );

    public Task<EnfileirarUsuarioMessageCommandResponse>
        EnfileirarUsuarioMessageLoteAsync
    (
        IEnumerable<EnfileirarUsuarioMessageCommand> request,
        CancellationToken cancellationToken
    );

    public Task<EnfileirarUsuarioMessageCommandResponse>
        EnfileirarUsuarioMessageAsync
    (
        EnfileirarUsuarioMessageCommand request,
        CancellationToken cancellationToken
    );

    public Task<EnfileirarEnviarEmailByEnderecoCommandResponse>
        EnfileirarEnviarEmailByEnderecoAsync
    (
        EnfileirarEnviarEmailByEnderecoCommand request,
        CancellationToken cancellationToken
    );
}