# ms-limpo-api

Template ASP.NET Core/.NET 10 para novos clientes do Olivaw Hub. O host mantém suas features, contexto, repositórios e projetos de provedor. Core e identidade são bibliotecas compartilhadas por `ProjectReference` dentro deste monorepo.

## Estrutura

- `src/Mps.Limpo.Api`: host HTTP, composição, readiness e feature de exemplo.
- `src/Mps.Limpo`: regras específicas do futuro cliente.
- `src/Mps.Limpo.Dados`: contexto, repositório genérico e mapeamentos locais.
- `src/Mps.Limpo.Dados.PostgreSql` e `.SqlServer`: projetos para migrations do cliente.
- `src/Mps.Limpo.Client`: biblioteca base para um futuro cliente REST.
- `tests/Mps.Limpo.Tests`: testes das funcionalidades do template.
- `../olivaw-identity`: usuários, perfis, permissões, vínculos e autenticação compartilhados.
- `../ms-olivaw-core-api`: contratos e infraestrutura genéricos.

Oracle permanece reservado; não há integração Oracle validada. Os componentes compartilhados ficam fora da pasta do cliente e devem acompanhar o monorepo no build.

## Gerar e desenvolver

Veja [TEMPLATE.md](TEMPLATE.md). A partir da raiz do monorepo:

```bash
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
dotnet build OlivawHub.sln -c Release
dotnet test ms-limpo-api/tests/Mps.Limpo.Tests/Mps.Limpo.Tests.csproj -c Release
```

Depois de configurar banco e identidade, execute `dotnet run --project ms-limpo-api/src/Mps.Limpo.Api`. Variáveis de ambiente ou user secrets fornecem `ConnectionStrings__DbContext`, `Identity__Issuer`, `Identity__Audience` e `Identity__SigningKey`. Os arquivos `appsettings` não contêm senha de banco ou chave JWT. O `.env` corporativo antigo foi retirado; use o exemplo sem credenciais reais. Configure `DatabaseProvider` como `PostgreSql` ou `SqlServer`; `Database:Schema` pode especificar o schema do histórico de migrations.

Cada cliente recebe seu próprio banco. O `WriteDbContext` aplica os mapeamentos de identidade junto dos locais; o módulo não tem conexão própria. As rotas compartilhadas são importadas por MVC application parts, sem copiar features para o cliente. Consulte [Olivaw Identity](../olivaw-identity/README.md) para contratos, policies e bootstrap de root.

## Schema e root

Não há migrations herdadas de autenticação. Gere e revise a migration inicial no projeto do provedor do cliente quando decidir criar o banco. A inicialização automática de migrations vem desligada (`Database:ApplyMigrations=false`). Só habilite-a após ter migrations válidas para o consumidor.

Com o schema pronto, habilite temporariamente `Identity__Bootstrap__Enabled=true`, informe `Identity__Bootstrap__Email` e `Identity__Bootstrap__Password` e inicialize uma instância da API. Somente root será criado; depois, retire essa configuração. A senha não é redefinida quando o bootstrap encontra root existente.

## Docker

O contexto de build é a raiz do monorepo para incluir todos os `ProjectReference`:

```bash
docker build -f ms-limpo-api/src/Mps.Limpo.Api/Dockerfile -t olivaw-limpo .
cp ms-limpo-api/docker/.env.example ms-limpo-api/docker/.env
```

Substitua os valores de exemplo antes de executar:

```bash
docker compose --env-file ms-limpo-api/docker/.env -f ms-limpo-api/docker/docker-compose.yml up --build
```

O Dockerfile executa as suítes de identidade, core e template, publica o host e roda sem root na porta 8080. TLS pertence ao ingress/reverse proxy; certificados privados não são copiados. Em Docker, use um endereço de banco alcançável pelo container, não `localhost` para um banco externo. Broker e logging externo vêm desabilitados.
