# Olivaw Identity

Bibliotecas compartilhadas de identidade. A autoridade administrativa interna é a [ms-olivaw-api](../ms-olivaw-api/README.md). Cada cliente mantém sua própria autoridade, usuários, banco, emissor, audiência e secret.

## Composição

- `AddOlivawIdentity<WriteDbContext>(configuration)`: registra login, CRUD, permissões e validação do estado local dos usuários. Usado pelo Olivaw e pelas APIs principais dos clientes.
- `AddOlivawIdentityConsumer(configuration)`: valida JWT localmente com o secret, emissor e audiência da autoridade. Não registra banco de identidade, usuários, bootstrap, endpoints de login ou consultas HTTP ao Olivaw. Usado pela DevOps e pelos outros serviços internos.

Os tokens incluem `sub`, `name`, `stamp`, `root`, `service`, `permission`, emissor, audiência e expiração. Os consumidores usam os claims assinados até expirar; alterações de acesso exigem novo token. A autoridade local continua validando o estado de seus próprios usuários no banco.

## Modelo

| Entidade | Tabela |
| --- | --- |
| `UserProfile` | `userprofile` |
| `Authorization` | `authorization` |
| `AccessProfile` | `accessprofile` |
| `UserProfileAccessProfile` | `userprofileaccessprofile` |
| `AccessProfileAuthorization` | `accessprofileauthorization` |

Entidades usam PK `int`, `ExternalId` GUID público e FKs `int`. O EF configura relacionamentos, índices únicos, remoção lógica e concorrência por `SecurityStamp`. Propriedades e colunas são em inglês; as convenções dos serviços aplicam lowercase às colunas. `IsServiceAccount` distingue contas técnicas de contas humanas. Root pertence somente à operação Olivaw e não pode ser atribuído pelo CRUD.

## API das autoridades

| Recurso | Operações |
| --- | --- |
| `/identity/authentication/login` | POST com `login`, `password`; retorna `accessToken`, `expiresAt` |
| `/identity/authentication/access` | GET com acesso atual da autoridade local |
| `/identity/authentication/logout` | POST |
| `/identity/authentication/password` | PUT com `currentPassword`, `newPassword` |
| `/identity/userprofiles` | POST, GET; PUT/GET/DELETE por GUID |
| `/identity/authorizations` | POST, GET; PUT/GET/DELETE por GUID |
| `/identity/accessprofiles` | POST, GET; PUT/GET/DELETE por GUID |
| `/identity/userprofileaccessprofiles` | POST, GET; PUT/GET/DELETE por GUID |
| `/identity/accessprofileauthorizations` | POST, GET; PUT/GET/DELETE por GUID |

O CRUD exige `identity.manage` ou root. Leituras retornam `item` ou `items` e aceitam `page`/`pageSize`. Senhas preservam espaços e caracteres; nunca retornam hashes. Serviços consumidores não expõem essas rotas.

## Inicialização e validação

As autoridades aplicam migrations e executam bootstrap idempotente de root no startup. Os consumidores aplicam apenas suas próprias migrations. Os bancos precisam existir antes de iniciar os containers. Migrations pertencem aos serviços, não a esta biblioteca.

```bash
dotnet build Olivaw.sln -c Release -m:1
dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release --no-build
```

Veja [arquitetura e publicação](../docs/identity-architecture.md). As configurações `.env` são versionadas no Git privado nesta etapa; não registre seus valores em logs ou documentação.

## Atualização de dependências

As dependências foram conferidas na atualização de 22/10/2026. MassTransit permanece em 8.5.x e MediatR em 12.5.x por decisão explícita do mantenedor, incluindo referências transitivas e serviços gerados. Ver [versões, limites e verificações do monorepo](../docs/dependencias.md). Não houve publicação de pacotes ou alteração de banco como parte desta atualização.
