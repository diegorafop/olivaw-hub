using MassTransit;

using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Api.Helpers;
using Mps.Auth.Contracts;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Domain.Model;
using Mps.Auth.Requests;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Https;
using Mps.HolosCore.Api.Extensions.Serializations;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Mps.Auth.Api.Extensions.Entities;

public static class UsuarioExtensions
{
    public static Expression<Func<UsuarioAutenticacao, bool>>
        SearchPredicate
    (
        this DeslogarUsuarioCommand request
    )
    {
        Expression<Func<UsuarioAutenticacao, bool>> expression = item => item
            .Usuario
            .IdExterno
            .Equals(request.UsuarioLogadoId);

        return expression.And(item => item
            .Token
            .Equals(request.Token));
    }

    public static RecuperarSenhaCommandResponse
        ToRecuperarSenhaCommandResponse
    (
        this RecuperarSenhaCommand request
    )
    {
        var response = request.Convert<RecuperarSenhaCommandResponse>();

        response.MensagemValidacao = "Verifique seu email para redefinir a senha.";

        return response;
    }

    public static SelecionarUsuarioByIdQueryResponse
        ToSelecionarUsuarioByIdQueryResponse
    (
        this Usuario entity,
        SelecionarUsuarioCargoByUsuarioIdQueryResponse request,
        IConfiguration configuration
    )
    {
        var response = entity.Map<SelecionarUsuarioByIdQueryResponse>();

        if (request is not null)
        {
            response.CargoId = request.CargoId;
            response.CargoNome = request.CargoNome;
        }

        response.Matricula = entity.GetMatricula(configuration);

        return response;
    }

    public static EnfileirarUsuarioMessageCommand
        ToEnfileirarUsuarioMessageCommand
    (
        this Usuario entity
    )
    {
        var response = entity.Map<EnfileirarUsuarioMessageCommand>();

        if (int.TryParse(entity.IntegracaoId, out int matricula))
            response.Matricula = matricula;

        return response;
    }

    public static IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse>
        ToSelecionarUsuarioCargoByUsuarioIdQueryResponse
    (
        this IEnumerable<Usuario> entities
    )
    {
        return entities.Select(entity => entity.ToSelecionarUsuarioCargoByFilterQueryResponse());
    }

    private static SelecionarUsuarioCargoByFilterQueryResponse
        ToSelecionarUsuarioCargoByFilterQueryResponse
    (
        this Usuario entity
    )
    {
        var response = entity.Map<SelecionarUsuarioCargoByFilterQueryResponse>();

        response.UsuarioId = entity.IdExterno;
        response.CargoId = Guid.Empty;
        response.CargoNome = "Sem cargo";

        return response;
    }

    public static SelecionarUsuarioCargoByUsuarioIdQueryResponse
        ToSelecionarUsuarioCargoByUsuarioIdQueryResponse
    (
        this Usuario entity
    )
    {
        var response = entity.Map<SelecionarUsuarioCargoByUsuarioIdQueryResponse>();

        response.UsuarioId = entity.IdExterno;
        response.CargoId = Guid.Empty;
        response.CargoNome = "Sem cargo";

        return response;
    }

    public static Usuario ToDomain
    (
        this InserirUsuarioCommand request
    )
    {
        Usuario entity = EntityExtensions
            .ToDomain<InserirUsuarioCommand,
                Usuario>(request);

        entity.Senha = CryptographyHelper.EnhancedHashPassword(entity.Senha);

        return entity;
    }

    public static void Update
    (
        this Usuario entity,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        Usuario usuarioRequest = request.ToDomain(configuration);

        if (!usuarioRequest.IsUsuarioEspecial(configuration))
        {
            entity.Nome = usuarioRequest.Nome;
            entity.Email = usuarioRequest.Email;
        }
        entity.UsuarioLogadoId = usuarioRequest.UsuarioLogadoId;

        UsuarioAutorizacaos(ref entity, usuarioRequest);
    }

    public static void Update
    (
        this Usuario entity,
        AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        Usuario usuarioRequest = request.ToDomain(configuration);

        if (!usuarioRequest.IsUsuarioEspecial(configuration))
        {
            entity.Nome = usuarioRequest.Nome;
            entity.Email = usuarioRequest.Email;
        }
        entity.UsuarioLogadoId = usuarioRequest.UsuarioLogadoId;

        UsuarioAutorizacaos(ref entity, usuarioRequest);
    }

    public static void Update
    (
        this Usuario entity,
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        IConfiguration configuration
    )
    {
        Usuario usuarioRequest = request.ToDomain(configuration);

        if (!usuarioRequest.IsUsuarioEspecial(configuration))
        {
            entity.Nome = usuarioRequest.Nome;
            entity.Email = usuarioRequest.Email;
        }

        entity.UsuarioLogadoId = usuarioRequest.UsuarioLogadoId;
    }

    public static bool IsUsuarioEspecial
    (
        this Usuario usuarioRequest,
        IConfiguration configuration
    )
    {
        if (GetLoginFuncionario(configuration).Equals(usuarioRequest.Login))
            return true;

        if (GetLoginGestor(configuration).Equals(usuarioRequest.Login))
            return true;

        return false;
    }

    private static string GetLoginFuncionario
    (
        IConfiguration configuration
    )
    {
        return configuration.GetValue<string>("constants:login:funcionario");
    }

    private static string GetLoginGestor
    (
        IConfiguration configuration
    )
    {
        return configuration.GetValue<string>("constants:login:gestor");
    }

    public static EnfileirarEnviarEmailByEnderecoCommand ToEnfileirarEnviarEmailByEnderecoCommand
    (
        this Usuario entity,
        IConfiguration configuration
    )
    {
        var command = new EnfileirarEnviarEmailByEnderecoCommand
        {
            Destinatarios = [entity.Email],
            Assunto = "Recuperação de senha",
            // TODO -  Mudar para um modelo de texto
            Mensagem = GetMensagem(entity.IdExterno, configuration),
            ComCopias = [],
            Anexos = []
        };

        return command;
    }
    private static string GetMensagem(Guid usuarioId, IConfiguration configuration)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append("<html>");
        stringBuilder.Append("<body>");
        stringBuilder.Append("<h1>Recuperação de Senha</h1>");
        stringBuilder.Append("<p>Olá,</p>");
        stringBuilder.Append("<p>Recebemos uma solicitação para redefinir sua senha. Clique no link abaixo para criar uma nova senha:</p>");
        stringBuilder.Append("<p>");
        stringBuilder.Append("<a href='");
        stringBuilder.Append(GetLinkRedefinirSenha(usuarioId, configuration));
        stringBuilder.Append("'>");
        stringBuilder.Append("Redefinir Senha");
        stringBuilder.Append("</a>");
        stringBuilder.Append("</p>");
        stringBuilder.Append("<p>Se você não solicitou a redefinição, ignore este e-mail. Sua senha permanecerá a mesma.</p>");
        stringBuilder.Append("<p>Essa é uma mensagem automática <strong> por favor, não responda!</strong></p>");
        stringBuilder.Append("</body>");
        stringBuilder.Append("</html>");

        return stringBuilder.ToString();
    }


    private static StringBuilder GetLinkRedefinirSenha
    (
        Guid usuarioId,
        IConfiguration configuration
    )
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(configuration.GetValue<string>("services:holos-ng:baseAddress"));
        stringBuilder.Append("/login/redefinir-senha/");
        // TODO - Criar Id da recuperacao e remover o do id do usuario
        stringBuilder.Append(usuarioId);

        return stringBuilder;
    }

    public static string SimulaModeloDeTexto
    (
        IConfiguration configuration
    )
    {
        string rota = configuration["services:holos-ng:baseAddress"];
        var mensagem = "<h1>Link de recuperação de email</h1><p>Esse link temporario redireciona para a tela de recuperação de senha. Após alterar sua senha, acesse o sistema.</p>"
                        + rota;
        return mensagem;
    }

    private static void UsuarioAutorizacaos
    (
        ref Usuario entity,
        Usuario usuarioRequest
    )
    {
        DateTime dateTime = DateTime.UtcNow;

        IList<UsuarioAutorizacao> autorizacaos = [];

        foreach (var autorizacaoRequest in usuarioRequest.Autorizacoes)
        {
            var autorizacaoExistente = entity
                .Autorizacoes
                .FirstOrDefault(item => item
                    .Autorizacao
                    .Tipo
                    .Equals(autorizacaoRequest
                        .Autorizacao
                        .Tipo));

            if (autorizacaoExistente == null)
                autorizacaos.Add(entity.CreateAutorizacao(autorizacaoRequest.Autorizacao));
            else
            {
                autorizacaoExistente.Autorizacao.Valor = autorizacaoRequest.Autorizacao.Valor;
                autorizacaoExistente.UsuarioLogadoId = usuarioRequest.UsuarioLogadoId;
                autorizacaoExistente.DataAtualizacao = dateTime;
            }
        }

        var entityAutorizacaos = entity
            .Autorizacoes
            .ToList();

        entityAutorizacaos.AddRange(autorizacaos);
        entity.Autorizacoes = entityAutorizacaos;
    }

    public static UsuarioAutorizacao CreateAutorizacao
    (
        this Usuario usuario,
        Autorizacao autorizacao
    )
    {
        UsuarioAutorizacao entity = EntityExtensions
                .ToDomain<Usuario,
                    UsuarioAutorizacao>(usuario);

        entity.UsuarioId = usuario.Id;
        entity.Autorizacao = autorizacao;

        return entity;
    }

    public static Usuario ToDomain
    (
        this AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);
        Usuario entity = claims.ToUsuario(request, configuration);

        entity.Autorizacoes = [.. claims
            .ToAutorizacaos(request, configuration)
            .Select(autorizacao =>
            {
                UsuarioAutorizacao usuarioAutorizacao = EntityExtensions
                    .ToDomain<AutenticarUsuarioComAdCommand,
                        UsuarioAutorizacao>(request);

                usuarioAutorizacao.Usuario = entity;
                usuarioAutorizacao.Autorizacao = autorizacao;

                return usuarioAutorizacao;
            })];

        return entity;
    }

    public static Usuario ToDomain
    (
        this AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        Usuario entity = claims.ToUsuario(request, configuration);
        entity.Autorizacoes = [.. claims
            .ToAutorizacaos(request, configuration)
            .Select(autorizacao =>
            {
                UsuarioAutorizacao usuarioAutorizacao = EntityExtensions
                    .ToDomain<AutenticarUsuarioComGovCommand,
                        UsuarioAutorizacao>(request);

                usuarioAutorizacao.Usuario = entity;
                usuarioAutorizacao.Autorizacao = autorizacao;

                return usuarioAutorizacao;
            })];

        return entity;
    }

    public static Usuario ToDomain
    (
        this AutenticarUsuarioComGovAutorizacaoManyCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        return claims.ToUsuario
        (
            request,
            configuration
        );
    }

    public static Usuario ToDomain
    (
        this AutenticarImpersonateCommand request,
        PerfilAcesso perfil
    )
    {
        Usuario entity = EntityExtensions
            .ToDomain<AutenticarImpersonateCommand,
                Usuario>(request);

        UsuarioPerfilAcesso usuarioPerfilAcesso = EntityExtensions
                    .ToDomain<AutenticarImpersonateCommand,
                        UsuarioPerfilAcesso>(request);

        usuarioPerfilAcesso.UsuarioId = entity.Id;
        usuarioPerfilAcesso.PerfilAcessoId = perfil.Id;

        List<UsuarioPerfilAcesso> itens = [];
        itens.Add(usuarioPerfilAcesso);

        entity.Perfils = itens;

        return entity;
    }

    public static InserirUsuarioInternalCommand ToInserirUsuarioInternalCommand
    (
        this AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var command = request.Convert<InserirUsuarioInternalCommand>();

        command.Login = TryGetLogin(claims, request, configuration);
        command.Nome = TryGetNome(claims, request, configuration);
        command.Email = TryGetEmail(claims, request, configuration);
        command.StatusIntegracaoEnum = StatusIntegracaoEnum.Pendente;

        return command;
    }

    public static InserirUsuarioInternalCommand ToInserirUsuarioInternalCommand
    (
        this AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);
        request.NivelConta = claims.GetNivelConta(configuration);

        var response = request.Convert<InserirUsuarioInternalCommand>();

        response.Login = TryGetLogin(claims, configuration);
        response.Nome = TryGetNome(claims, configuration);
        response.Email = TryGetEmail(claims, configuration);

        return response;
    }

    public static InserirUsuarioInternalCommand ToInserirUsuarioInternalCommand
    (
        this AutenticarImpersonateCommand request
    )
    {
        var response = request.Convert<InserirUsuarioInternalCommand>();

        response.Login = request.Login;
        response.Nome = request.Nome;
        response.Email = request.Email;

        return response;
    }

    public static InserirUsuarioInternalCommand ToInserirUsuarioInternalCommand
    (
        this AutenticarUsuarioComGovAutorizacaoManyCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var response = request.Convert<InserirUsuarioInternalCommand>();

        response.Login = TryGetLogin(claims, configuration);
        response.Nome = TryGetNome(claims, configuration);
        response.Email = TryGetEmail(claims, configuration);

        return response;
    }

    public static string GetNivelConta
    (
        this IEnumerable<Claim> claims,
        IConfiguration configuration
    )
    {
        var item = claims.GetValue
        (
            configuration,
            "authentication:oidc:custom-claims:reliability_info"
        );

        if (string.IsNullOrEmpty(item))
            return string.Empty;

        var reliability = item.Deserialize<ReliabilityInfo>();

        if (reliability is null)
            return string.Empty;

        return reliability.Level;
    }

    public static UsuarioModel ToUsuarioModel
    (
        this AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var command = request.Convert<UsuarioModel>();

        command.Login = TryGetLogin(claims, request, configuration);
        command.Nome = TryGetNome(claims, request, configuration);
        command.Email = TryGetEmail(claims, request, configuration);

        return command;
    }

    public static UsuarioModel ToUsuarioModel
    (
        this AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var response = request.Convert<UsuarioModel>();

        response.Login = TryGetLogin(claims, configuration);
        response.Nome = TryGetNome(claims, configuration);
        response.Email = TryGetEmail(claims, configuration);

        return response;
    }

    public static UsuarioModel ToUsuarioModel
    (
        this AutenticarImpersonateCommand request
    )
    {
        var response = request.Convert<UsuarioModel>();

        response.Login = request.Login;
        response.Nome = request.Nome;
        response.Email = request.Email;

        return response;
    }

    public static UsuarioModel ToUsuarioModel
    (
        this AutenticarUsuarioComGovAutorizacaoManyCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var response = request.Convert<UsuarioModel>();

        response.Login = TryGetLogin(claims, configuration);
        response.Nome = TryGetNome(claims, configuration);
        response.Email = TryGetEmail(claims, configuration);

        return response;
    }

    public static IList<UsuarioAutorizacao> ToManyUsuarioAutorizacao
    (
        this Usuario entity,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        IEnumerable<Claim> claims = JwtSecurityTokenHelper.GetClaimsByIdToken(request.IdToken);

        var autorizacoes = claims
            .ToAutorizacaos(request, configuration)
            .Select(item =>
            {
                UsuarioAutorizacao usuarioAutorizacao = EntityExtensions
                    .ToDomain<AutenticarUsuarioComAdCommand,
                        UsuarioAutorizacao>(request);

                usuarioAutorizacao.Usuario = entity;
                usuarioAutorizacao.Autorizacao = item;

                return usuarioAutorizacao;
            })
            .ToList();

        entity.Autorizacoes = autorizacoes;

        return autorizacoes;
    }

    public static Usuario ToUsuario
    (
        this IEnumerable<Claim> claims,
        AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        Usuario entity = EntityExtensions
            .ToDomain<AutenticarUsuarioComGovCommand,
                Usuario>(request);

        entity.Login = TryGetLogin(claims, configuration);
        entity.Nome = TryGetNome(claims, configuration);
        entity.Email = TryGetEmail(claims, configuration);

        entity.DataRemocao = null;
        return entity;
    }

    public static Usuario ToUsuario
    (
        this IEnumerable<Claim> claims,
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        IConfiguration configuration
    )
    {
        Usuario entity = EntityExtensions
            .ToDomain<AutenticarUsuarioComGovAutorizacaoManyCommand,
                Usuario>(request);

        entity.Login = TryGetLogin(claims, configuration);
        entity.Nome = TryGetNome(claims, configuration);
        entity.Email = TryGetEmail(claims, configuration);

        entity.DataRemocao = null;
        return entity;
    }

    public static Usuario ToUsuario
    (
        this IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        Usuario entity = EntityExtensions
            .ToDomain<AutenticarUsuarioComAdCommand,
                Usuario>(request);

        entity.Login = TryGetLogin(claims, request, configuration);
        entity.Nome = TryGetNome(claims, request, configuration);
        entity.Email = TryGetEmail(claims, request, configuration);

        entity.DataRemocao = null;
        return entity;
    }

    private static string TryGetEmail
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        string email = GetValue(claims, configuration, "authentication:oidc:standard-claims:email");
        return string.IsNullOrEmpty(email) ? GetEmail(request) : email;
    }

    private static string TryGetEmail
    (
        IEnumerable<Claim> claims,
        IConfiguration configuration
    )
    {
        string email = GetValue(claims, configuration, "authentication:oidc:standard-claims:email");
        return string.IsNullOrEmpty(email) ? string.Empty : email;
    }

    private static string TryGetNome
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        string nome = GetValue(claims, configuration, "authentication:oidc:standard-claims:name");
        return string.IsNullOrEmpty(nome) ? GetNome(request) : nome;
    }

    private static string TryGetNome
    (
        IEnumerable<Claim> claims,
        IConfiguration configuration
    )
    {
        string nome = GetValue(claims, configuration, "authentication:oidc:standard-claims:name");
        return string.IsNullOrEmpty(nome) ? string.Empty : nome;
    }

    private static string TryGetLogin
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        string login = GetValue(claims, configuration, "authentication:oidc:standard-claims:username");
        return string.IsNullOrEmpty(login) ? GetLogin(request) : login;
    }

    private static string TryGetLogin
    (
        IEnumerable<Claim> claims,
        IConfiguration configuration
    )
    {
        string login = GetValue(claims, configuration, "authentication:oidc:standard-claims:username");
        return string.IsNullOrEmpty(login) ? string.Empty : login;
    }

    private static string GetNome
    (
        AutenticarUsuarioComAdCommand request
    )
    {
        return string.IsNullOrEmpty(request.Nome) ? string.Empty : request.Nome.Trim();
    }

    private static string GetEmail
    (
        AutenticarUsuarioComAdCommand request
    )
    {
        return string.IsNullOrEmpty(request.Email) ? string.Empty : request.Email.Trim();
    }

    private static string GetLogin
    (
        AutenticarUsuarioComAdCommand request
    )
    {
        return string.IsNullOrEmpty(request.Login) ? string.Empty : request.Login.Trim();
    }

    private static string GetValue
    (
        this IEnumerable<Claim> claims,
        IConfiguration configuration,
        string claimName
    )
    {
        var email = claims.GetValue(configuration.GetValue<string>(claimName));
        return string.IsNullOrEmpty(email) ? string.Empty : email.Trim();
    }

    public static void Update
    (
        this AtualizarUsuarioCommand request,
        Usuario entity
    )
    {
        entity.Login = request.Login;
        entity.Nome = request.Nome;
        entity.Email = request.Email;
        entity.IntegracaoId = request.IntegracaoId;
        entity.SetStatusIntegracaoEnum(request);

        entity.Update(request.UsuarioLogadoId);
    }

    public static void SetStatusIntegracaoEnum
    (
        this Usuario entity,
        AtualizarUsuarioCommand request
    )
    {
        if (!request.StatusIntegracaoEnum.Equals(StatusIntegracaoEnum.None))
            entity.StatusIntegracaoEnum = request.StatusIntegracaoEnum;
    }

    public static void UpdatePerfil
    (
        this AtualizarPerfilCommand request,
        Usuario entity
    )
    {
        entity.Login = request.Login;
        entity.Nome = request.Nome;
        entity.Email = request.Email;
        entity.UsuarioLogadoId = request.UsuarioLogadoId;
    }
    private static void UpdateSenha
    (
        Usuario entity,
        string senha,
        Guid usuarioLogadoId
    )
    {
        entity.Senha = CryptographyHelper.EnhancedHashPassword(senha);
        entity.DataAtualizacao = DateTime.UtcNow;
        entity.UsuarioLogadoId = usuarioLogadoId;
    }

    public static void UpdateSenha
    (
        this AlterarSenhaCommand request,
        Usuario entity
    )
    {
        UpdateSenha(entity, request.SenhaNova, request.UsuarioLogadoId);
    }

    public static void UpdateSenha
    (
        this AlterarSenhaByExternoCommand request,
        Usuario entity
    )
    {
        UpdateSenha(entity, request.SenhaNova, request.UsuarioLogadoId);
    }

    public static void UpdatePerfilSenha
    (
        this AlterarPerfilSenhaCommand request,
        Usuario entity
    )
    {
        entity.Senha = CryptographyHelper.EnhancedHashPassword(request.Senha);
        entity.UsuarioLogadoId = request.UsuarioLogadoId;
    }

    public static UsuarioAutenticacao ToInsertUsuarioAutenticacao
    (
        this Usuario entity
    )
    {
        UsuarioAutenticacao usuarioAutenticacao = EntityExtensions
            .ToDomain<Usuario,
                UsuarioAutenticacao>(entity);

        usuarioAutenticacao.Usuario = entity;
        usuarioAutenticacao.Token = CryptographyHelper.GerarTokenAtualizacao();

        return usuarioAutenticacao;
    }

    public static UsuarioAutenticacao ToUsuarioAutenticacao
    (
        this Usuario? entity
    )
    {
        if (entity is null)
            return default;

        var usuarioAutenticacao = entity.Replicate<UsuarioAutenticacao>(entity.UsuarioLogadoId);

        if (usuarioAutenticacao.UsuarioId == 0)
            usuarioAutenticacao.Usuario = entity;
        else
            usuarioAutenticacao.UsuarioId = entity.Id;

        usuarioAutenticacao.Token = CryptographyHelper.GerarTokenAtualizacao();

        return usuarioAutenticacao;
    }

    public static AutenticarUsuarioExternalCommandResponse
        ToAutenticarUsuarioExternalCommandResponse
    (
        this Usuario entity,
        string refreshToken,
        IConfiguration configuration
    )
    {
        var response = entity.Map<AutenticarUsuarioExternalCommandResponse>();

        response.Token = CryptographyHelper.GetJwtSecurityToken(entity, configuration);
        response.TokenRefresh = refreshToken;
        response.Ativo = entity.DataRemocao == null;
        response.UsuarioId = entity.IdExterno;

        return response;
    }

    public static AutenticarUsuarioComAdCommandResponse
        ToAutenticarUsuarioComAdCommandResponse
    (
        this Usuario entity,
        string refreshToken,
        IConfiguration configuration
    )
    {
        var response = entity.Map<AutenticarUsuarioComAdCommandResponse>();
        response.Token = CryptographyHelper.GetJwtSecurityToken(entity, configuration);
        response.TokenRefresh = refreshToken;
        return response;
    }

    public static AutenticarUsuarioComGovCommandResponse
        ToAutenticarUsuarioComGovCommandResponse
    (
        this Usuario entity,
        string refreshToken,
        IConfiguration configuration
    )
    {
        var response = entity.Map<AutenticarUsuarioComGovCommandResponse>();
        response.Token = CryptographyHelper.GetJwtSecurityToken(entity, configuration);
        response.TokenRefresh = refreshToken;
        return response;
    }

    public static AutenticarUsuarioComGovAutorizacaoManyCommandResponse
        ToAutenticarUsuarioComGovAutorizacaoManyCommandResponse
    (
        this Usuario entity,
        string refreshToken,
        IConfiguration configuration
    )
    {
        var response = entity.Map<AutenticarUsuarioComGovAutorizacaoManyCommandResponse>();
        response.Token = CryptographyHelper.GetJwtSecurityToken(entity, configuration);
        response.TokenRefresh = refreshToken;
        return response;
    }

    public static AutenticarImpersonateCommandResponse
        ToAutenticarImpersonateCommandResponse
    (
        this Usuario entity,
        string refreshToken,
        IConfiguration configuration
    )
    {
        var response = entity.Map<AutenticarImpersonateCommandResponse>();

        response.Token = CryptographyHelper.GetJwtSecurityToken(entity, configuration);
        response.TokenRefresh = refreshToken;
        response.Ativo = entity.DataRemocao == null;
        response.UsuarioId = entity.IdExterno;

        return response;
    }

    public static SelecionarUsuarioByFilterQueryResponse
        ToSelecionarUsuarioByFilterQueryResponse
    (
        this Usuario entity,
        SelecionarUsuarioCargoByFilterQueryResponse usuarioCargo,
        IConfiguration configuration
    )
    {
        var response = entity.Map<SelecionarUsuarioByFilterQueryResponse>();

        string matricula = entity.GetMatricula(configuration);

        if (int.TryParse(matricula, out int matriculaConvertida))
            response.Matricula = matriculaConvertida;

        response.CargoId = usuarioCargo.CargoId;
        response.CargoNome = usuarioCargo.CargoNome;

        return response;
    }

    public static UpsertDestinatarioExternalCommand
        ToUpsertDestinatarioExternalCommand
    (
        this ConsumeContext<EnfileirarUpsertDestinatarioContract> request
    )
    {
        var response = request
            .Message
            .Map<UpsertDestinatarioExternalCommand>();

        response.UsuarioLogadoId = request
            .Message
            .UsuarioLogadoId;

        return response;
    }

    public static UpsertDocumentoUsuarioPreferenciaExternalCommand
        ToUpsertDocumentoUsuarioPreferenciaExternalCommand
    (
        this ConsumeContext<EnfileirarUpsertDocumentoUsuarioPreferenciaContract> request
    )
    {
        return request
            .Message
            .Map<UpsertDocumentoUsuarioPreferenciaExternalCommand>();
    }

    public static UpsertDocumentoUsuarioPreferenciaExternalCommandResponse
        ToUpsertDocumentoUsuarioPreferenciaExternalCommandResponse
    (
        this UpsertDocumentoUsuarioPreferenciaCommandResponse response
    )
    {
        return EntityExtensions
            .Convert<UpsertDocumentoUsuarioPreferenciaCommandResponse,
                UpsertDocumentoUsuarioPreferenciaExternalCommandResponse>(response);
    }

    public static UpsertDestinatarioExternalCommandResponse
        ToUpsertDestinatarioExternalCommandResponse
    (
        this UpsertDestinatarioCommandResponse response
    )
    {
        return EntityExtensions
            .Convert<UpsertDestinatarioCommandResponse,
                UpsertDestinatarioExternalCommandResponse>(response);
    }

    public static EnfileirarUpsertDestinatarioContract
        ToEnfileirarUpsertDestinatarioContract
    (
        this Usuario usuario
    )
    {
        var contract = usuario.Map<EnfileirarUpsertDestinatarioContract>();

        contract.UsuarioLogadoId = usuario.IdExterno;
        contract.Endereco = usuario.Email;

        return contract;
    }


    private static string GetMatricula
    (
        this Usuario entity,
        IConfiguration configuration
    )
    {
        if (!string.IsNullOrEmpty(entity.IntegracaoId))
            return entity.IntegracaoId;

        if (entity.Autorizacoes is null)
            return string.Empty;

        var claimType = configuration.GetValue<string>
        (
            "authentication:oidc:custom-claims:matricula"
        );

        var matricula = entity
            .Autorizacoes
            .Where(item => item.Autorizacao is not null)
            .Select(item => item.Autorizacao)
            .FirstOrDefault(item => item
                .Tipo
                .Equals(claimType));

        if (matricula is null)
            return string.Empty;

        return matricula.Valor;
    }


    public static InserirUsuarioInternalCommand
        ToInserirUsuarioInternalCommand
    (
        this UpsertUsuarioCommand request,
        UpsertUsuarioItemCommand item
    )
    {
        var command = EntityExtensions.Convert<
            UpsertUsuarioItemCommand,
            InserirUsuarioInternalCommand>(item);

        command.UsuarioLogadoId = request.UsuarioLogadoId;

        return command;
    }

    public static InserirUsuarioInternalCommand
        ToInserirUsuarioInternalCommand
    (
        this UpsertUsuarioHolosCommand request
    )
    {
        return EntityExtensions.Convert<
            UpsertUsuarioHolosCommand,
            InserirUsuarioInternalCommand>(request);
    }

    public static bool AnyPerfil
    (
        this Usuario entity
    )
    {
        return entity.Perfils?.Any() ?? false;
    }

    public static Usuario ToDomain
    (
        this InserirUsuarioInternalCommand request
    )
    {
        var entity = request.ToDomain<Usuario>();

        entity.Senha = CryptographyHelper.EnhancedHashPassword(entity.Senha);

        if (request.Id.HasValue)
            entity.IdExterno = request.Id.Value;

        return entity;
    }

    public static IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse>
        ToAtualizarUsuarioStatusIntegracaoPendenteCommandResponse
    (
        this IEnumerable<Usuario> itens
    )
    {
        return itens.Select
        (
            item => item.ToAtualizarUsuarioStatusIntegracaoPendenteCommandResponse()
        );
    }

    public static AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse
        ToAtualizarUsuarioStatusIntegracaoPendenteCommandResponse
    (
        this Usuario item
    )
    {
        var response = item.Map<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse>();

        if (!string.IsNullOrEmpty(item.IntegracaoId))
            response.Matricula = int.Parse(item.IntegracaoId);

        return response;
    }

    public static bool EqualsPendenteProcessoAdmissao
    (
        this Usuario usuario
    )
    {
        return usuario
            .StatusIntegracaoEnum
            .Equals(StatusIntegracaoEnum.PendenteProcessoAdmissao);
    }

    public static IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse>
        ToAtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse
    (
        this IEnumerable<Usuario> itens
    )
    {
        return itens.Select
        (
            item => item.ToAtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse()
        );
    }

    public static AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse
        ToAtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse
    (
        this Usuario item
    )
    {
        return item.Map<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse>();
    }

    public static SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery
        ToSelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery
    (
        this IEnumerable<Usuario> itens
    )
    {
        var usuarioIdMany = itens.Select(item => item.IdExterno);

        return new SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery
        {
            UsuarioIdMany = usuarioIdMany
        };
    }

    public static EnfileirarUpsertDocumentoUsuarioPreferenciaContract
        ToEnfileirarUpsertDocumentoUsuarioPreferenciaContract
    (
        this Usuario entity,
        IConfiguration configuration
    )
    {
        var contract = EntityExtensions
            .Convert<Usuario,
                EnfileirarUpsertDocumentoUsuarioPreferenciaContract>(entity);

        contract.UsuarioId = entity.IdExterno;

        contract.Interessado = configuration.GetValue<bool>
        (
            "constants:documento-usuario-preferencia:interessado"
        );

        return contract;
    }
}