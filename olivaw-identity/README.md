# Olivaw Identity

Módulo compartilhado de identidade e acesso. Esta pasta agrupa três **bibliotecas**, não um microservice: não contém `Program.cs`, servidor próprio, conexão de banco nem migrations.

| Projeto | Responsabilidade |
| --- | --- |
| `src/Olivaw.Identity` | Entidades e contrato de persistência; .NET Standard 2.1 |
| `src/Olivaw.Identity.Dados` | Mapeamentos EF Core e adaptação ao contexto do consumidor; .NET 10 |
| `src/Olivaw.Identity.Api` | Controllers, CQS, validação, autenticação e autorização; .NET 10 |
| `tests/Olivaw.Identity.Tests` | Testes unitários e testes HTTP com SQLite em memória |

As dependências seguem `Api → Dados → Identity → OlivawCore`. O core genérico permanece independente de identidade. No monorepo, o template consome os projetos por `ProjectReference`; não há publicação NuGet nem cópia manual de DLLs. Futuramente, as mesmas bibliotecas podem ser distribuídas por pacote.

## Modelo de acesso

- `Usuario`: login, nome, email, hash de senha e estado ativo.
- `PerfilAcesso`: role, como Atendente ou Administrador.
- `Autorizacao`: permissão com chave estável em minúsculas, como `pedidos.cancelar`.
- `UsuarioPerfilAcesso`: associação de usuário e perfil.
- `PerfilAcessoAutorizacao`: associação de perfil e permissão.

As permissões efetivas são a união das permissões dos perfis ativos. Não há concessão direta ao usuário. O usuário `root` é o administrador inicial e possui acesso integral; sua condição especial não pode ser atribuída pelo body HTTP. Apenas usuários com `identity.gerenciar`, além do root, administram os cadastros do módulo.

## Integração no host

O template já contém os registros abaixo. Ao criar outro consumidor, referencie os projetos e registre o contexto concreto do próprio cliente antes do módulo:

```csharp
builder.Services.AddOlivawIdentity<WriteDbContext>(builder.Configuration);
```

No `OnModelCreating` do contexto, antes das convenções de nomes:

```csharp
modelBuilder.AddOlivawIdentity();
```

O host disponibiliza controllers, middleware de autenticação/autorização e `UseRateLimiter()` após o roteamento. O módulo adiciona sua assembly como MVC application part e registra seus handlers MediatR. Ele não adiciona o pipeline do core que aplica `Trim()` indiscriminadamente às strings: senhas preservam todos os caracteres. O template mantém esse pipeline apenas para suas próprias features.

Os cinco recursos usam tabelas `identity_*` dentro do banco do consumidor. Cada cliente deve ter conexão, credenciais de banco, emissor, destinatário e chave JWT próprios. A separação de bancos é uma responsabilidade de implantação: não existe seleção de tenant por header nem conexão global de identidade.

## Configuração e usuário inicial

Forneça por ambiente ou secret store:

```text
ConnectionStrings__DbContext=<conexão exclusiva do cliente>
Identity__Issuer=<identificador da API do cliente>
Identity__Audience=<identificador do consumidor>
Identity__SigningKey=<chave aleatória exclusiva com pelo menos 32 bytes>
```

O JWT expira em 30 minutos por padrão; `Identity__TokenMinutes` aceita de 1 a 120. A chave é validada na inicialização e não tem valor default.

Após preparar o schema do cliente, habilite o bootstrap uma vez:

```text
Identity__Bootstrap__Enabled=true
Identity__Bootstrap__Email=<email do responsável>
Identity__Bootstrap__Password=<senha exclusiva não vazia, até 128 caracteres>
```

O host chama `await app.InitializeOlivawIdentityAsync()`. O bootstrap cria somente `root`, com senha em hash, e não altera um root existente. Depois, desabilite-o e retire a senha de bootstrap do ambiente. Execute o primeiro bootstrap em uma única instância; inicializações concorrentes podem disputar a chave única. O root não pode ser removido, desativado ou renomeado pelas rotas.

Nenhuma migration antiga foi transportada, e nenhuma nova foi gerada. As migrations e sua aplicação pertencem aos projetos de provedor de cada cliente. `Database:ApplyMigrations` vem desabilitado; habilite somente após criar e revisar as migrations do consumidor. O bootstrap não cria tabelas nem executa `EnsureCreated` ou migrations. SQLite/`EnsureCreated` são usados somente nos testes.

## Rotas

| Recurso | Base |
| --- | --- |
| Usuário | `/identity/usuarios` |
| Perfil | `/identity/perfis-acesso` |
| Permissão | `/identity/autorizacoes` |
| Usuário/perfil | `/identity/usuarios-perfis` |
| Perfil/permissão | `/identity/perfis-autorizacoes` |

Cada recurso oferece `POST`, `PUT /{id}`, `DELETE /{id}`, `GET /{id}` e `GET ?pagina=1&tamanho=20`. O limite é 100 registros por página. Responses individuais usam `{ registro: ... }`; listagens usam `{ registros: [...] }`. IDs HTTP são GUIDs públicos; vínculos recebem esses GUIDs, nunca as chaves numéricas do banco.

Usuário recebe `login`, `nome`, `email`, `ativo` e, na criação, `senha`. Perfil recebe `nome`, `descricao`, `ativo`. Permissão recebe `chave`, `nome`, `descricao`. Vínculos recebem `usuarioId`/`perfilAcessoId` ou `perfilAcessoId`/`autorizacaoId`. Campos de auditoria e privilégio root não são aceitos como comandos de administração.

- `GET /identity/autenticacao/acesso`: dados do usuário atual e permissões efetivas; `root=true` indica acesso integral.
- `POST /identity/autenticacao/login`: `{ login, senha }`; retorna `{ accessToken, expiresAt }`.
- `PUT /identity/autenticacao/senha`: `{ senhaAtual, novaSenha }`; usuário autenticado altera sua própria senha.
- `POST /identity/autenticacao/logout`: `{}`; revoga todas as sessões daquele usuário.

Alterar senha, desativar ou remover usuário invalida seus tokens existentes. Permissões são consultadas no banco a cada autorização, sem esperar o JWT expirar. Login tem limite por IP e bloqueio temporário de conta após cinco falhas. Configure corretamente o proxy confiável e TLS no ambiente de implantação.

Erros esperados usam status 400, 401, 403, 404 ou 409 e Problem Details. Senhas e hashes não são retornados. Remoção é lógica; login, nome de perfil e chave de permissão permanecem reservados, inclusive após remoção. Recriar um vínculo removido restaura o registro anterior. Não há endpoints de recuperação por email, SSO, AD, governo ou integrações corporativas.

## Proteger features do cliente

Registre uma policy no host e use `[Authorize(Policy = "pedidos.cancelar")]` no endpoint correspondente:

```csharp
builder.Services.AddAuthorization(options =>
    options.AddPolicy("pedidos.cancelar", policy =>
        policy.RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement("pedidos.cancelar"))));
```

`PermissionRequirement` pertence a `Olivaw.Identity.Api.Security`. Criar uma autorização no banco não protege automaticamente uma rota: a feature declara a policy exigida. O módulo usa policies de permissão, não claims de roles embutidas no JWT.

## Validação

Na raiz do monorepo:

```bash
dotnet build OlivawHub.sln -c Release
dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release --no-build
```

Alternativa sem comunicação VSTest por socket:

```bash
dotnet run --project olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release --no-build -- -noLogo
```

Testes relacionais usam SQLite; isso não comprova integração específica com PostgreSQL ou SQL Server. Consulte o [registro da implementação](../docs/identidade.md).

A senha é obrigatória, com limite de 128 caracteres e sem tamanho mínimo adicional, tanto no cadastro quanto na troca e no bootstrap. Os caracteres são preservados integralmente. O e-mail tem validação de formato; não há envio ou confirmação por e-mail como requisito para login.

`RootBootstrap.EnsureAsync` sempre garante o root e preserva o cadastro existente. LTMJ e template limpo o executam obrigatoriamente após as migrations no startup. O wrapper `InitializeOlivawIdentityAsync` mantém a chave `Identity:Bootstrap:Enabled` somente para compatibilidade com consumidores que já utilizavam a inicialização opcional.
