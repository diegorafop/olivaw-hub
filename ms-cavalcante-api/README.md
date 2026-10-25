# ms-cavalcante-api

Template ASP.NET Core/.NET 10 para novos clientes do Olivaw. O host mantém suas features, contexto, repositórios e projetos de provedor. Core e identidade são bibliotecas compartilhadas por `ProjectReference` dentro deste monorepo.

## Etapa Cavalcante — issue #24

Esqueleto gerado por `bash ms-limpo-api/scripts/new-microservice.sh cavalcante`, sem implementação de domínio, endpoints de catálogo/orçamento ou novas migrations nesta tarefa. Os projetos estão em `Olivaw.sln`. Identidade local independente e segredos iniciais foram criados pelo gerador oficial; não reutilizam os do template.

A infraestrutura e a feature de exemplo são herdadas. O frontend funciona sem iniciar este host. Não iniciar para validar o protótipo: o startup aplica migrations pendentes e pode inicializar identidade. Oracle permanece reservado. Consulte o [planejamento](../docs/clients/cavalcante/README.md).

Validação local sem executar o host, a partir da raiz:

```bash
dotnet build ms-cavalcante-api/tests/Owh.Cavalcante.Tests/Owh.Cavalcante.Tests.csproj -c Release -m:1
dotnet test ms-cavalcante-api/tests/Owh.Cavalcante.Tests/Owh.Cavalcante.Tests.csproj -c Release --no-build --no-restore
```

## Estrutura

- `src/Owh.Cavalcante.Api`: host HTTP, composição, readiness e feature de exemplo.
- `src/Owh.Cavalcante`: regras e contratos específicos do futuro cliente, com as pastas permanentes `Repositories`, `Services` e `Requests`.
- `src/Owh.Cavalcante.Dados`: contexto, repositório genérico e mapeamentos locais.
- `src/Owh.Cavalcante.Dados.PostgreSql` e `.SqlServer`: projetos para migrations do cliente.
- `src/Owh.Cavalcante.Client`: biblioteca base para um futuro cliente REST.
- `tests/Owh.Cavalcante.Tests`: testes das funcionalidades do template.
- `../olivaw-identity`: usuários, perfis, permissões, vínculos e autenticação compartilhados.
- `../ms-olivaw-core-api`: contratos e infraestrutura genéricos.

## Repositórios

Use `IRepository<T>` para CRUD e consultas comuns com filtros por expressão. Não crie um repository ou reader específico quando os métodos genéricos atenderem. Para uma consulta realmente especializada, uma classe como `PortfolioRepository : Repository<PortfolioEvent>` pode acrescentar o comportamento necessário e herda o contrato genérico, aproveitando o código base.

## Serviços e requests externos

`Owh.Cavalcante/Services` guarda interfaces pequenas para capacidades externas que não são persistência, como armazenamento de arquivos, APIs de terceiros e mensageria; suas implementações ficam nas camadas concretas. `Owh.Cavalcante/Requests` guarda contratos de Commands/Queries enviados a outros microservices, organizados por serviço e feature. Requests e handlers MediatR das próprias features continuam em `Owh.Cavalcante.Api`.

Oracle permanece reservado; não há integração Oracle validada. Os componentes compartilhados ficam fora da pasta do cliente e devem acompanhar o monorepo no build.

## Gerar e desenvolver

Veja [TEMPLATE.md](TEMPLATE.md). A partir da raiz do monorepo:

```bash
bash ms-cavalcante-api/scripts/new-microservice.sh pastelaria-joao
dotnet build Olivaw.sln -c Release
dotnet test ms-cavalcante-api/tests/Owh.Cavalcante.Tests/Owh.Cavalcante.Tests.csproj -c Release
```

Depois de configurar banco e identidade, execute `dotnet run --project ms-cavalcante-api/src/Owh.Cavalcante.Api`. Variáveis de ambiente ou user secrets fornecem `ConnectionStrings__DbContext`, `Identity__Issuer`, `Identity__Audience` e `Identity__SigningKey`. Os arquivos `appsettings` não contêm senha de banco ou chave JWT. Os dotenv de serviço são versionados no repositório privado conforme a decisão atual do mantenedor. Configure `DatabaseProvider` como `PostgreSql` ou `SqlServer`; `Database:Schema` pode especificar o schema do histórico de migrations.

Cada organização/cliente recebe seu banco por ambiente; cada serviço usa um schema próprio. O `WriteDbContext` aplica os mapeamentos de identidade junto dos locais; o módulo não tem conexão própria. As rotas compartilhadas são importadas por MVC application parts, sem copiar features para o cliente. Consulte [Olivaw Identity](../olivaw-identity/README.md) para contratos, policies e bootstrap de root.

## Schema e root

O startup sempre aplica as migrations pendentes do provedor e depois garante o root antes de atender requisições. Não há flag para habilitar migrations ou bootstrap. Se essa preparação falhar, a API não inicia com o banco incompleto.

O bootstrap é idempotente: se root já existir, preserva sua senha e seu cadastro. Configure `Identity__Bootstrap__Email` e `Identity__Bootstrap__Password` para a primeira inicialização. Root é exclusivo do Olivaw; contas de clientes recebem somente as permissões de negócio.

Os arquivos de migrations precisam acompanhar o projeto. O startup aplica arquivos existentes; geração de novas migrations é uma etapa de desenvolvimento. O contexto de design time PostgreSQL permite gerar arquivos sem inicializar a API nem acessar a AWS.

### Gerar e revisar migrations PostgreSQL

As migrations seguem a issue da branch: `feature/issue013-descricao` gera `Issue013`. Se já existir, acrescente um sufixo descritivo; não reescreva o histórico. Gere somente os provedores solicitados e mediante pedido explícito.

Exemplo a partir da raiz do monorepo; substitua `Issue013` pela issue da entrega:

```bash
dotnet build ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql --no-restore -m:1

dotnet ef migrations add Issue013 \
  --project ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql \
  --startup-project ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql \
  --context WriteDbContext --no-build
```

O `--no-restore` exige dependências restauradas. A factory de design time evita iniciar o host e não aplica alterações no banco. Depois de gerar, compile novamente o provedor e revise migration, designer e snapshot. Gere o SQL substituindo `MIGRATION_ANTERIOR` pelo identificador real da migration anterior, ou `0` se for a primeira:

```bash
dotnet build ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql --no-restore -m:1

dotnet ef migrations script MIGRATION_ANTERIOR Issue013 \
  --project ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql \
  --startup-project ms-cavalcante-api/src/Owh.Cavalcante.Dados.PostgreSql \
  --context WriteDbContext --no-build \
  --output /tmp/cavalcante-issue013.sql
```

Esses comandos não conectam ao banco nem autorizam aplicação manual ou redeploy. A API aplica migrations pendentes ao iniciar após a publicação. Para APIs geradas, ajuste os caminhos para o novo serviço; não reutilize o histórico de outro cliente. Veja a [padronização no Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

## Configuração de nuvem e Docker

Os appsettings por ambiente e overrides Compose seguem os endereços de infraestrutura usados pelo FinTalk. O template usa banco `olivaw_prd`, schema `ms_cavalcante` e domínio de produção `https://api.cavalcante.olivaw.com.br`. Outros ambientes têm nomes próprios, documentados no [guia de nuvem](../docs/cloud.md).

As configurações dos ambientes estão nos dotenv versionados no repositório privado até a futura migração para Key Vault; o arquivo versionado `.env.example` contém placeholders. As variáveis do Compose substituem as connection strings sem senha dos appsettings. Redis tem connection string preparada, mas não possui integração ativa no código. RabbitMQ é habilitado nos dotenv de nuvem, com prefixo exclusivo por serviço/ambiente.

O contexto de build continua sendo a raiz do monorepo:

```bash
docker build -f ms-cavalcante-api/src/Owh.Cavalcante.Api/Dockerfile -t olivaw-cavalcante .
docker compose --env-file ms-cavalcante-api/docker/.env.prd -f ms-cavalcante-api/docker/docker-compose.yml -f ms-cavalcante-api/docker/docker-compose.prd.yml config --quiet
```

O último comando apenas valida. A subida exige bancos/schemas preparados e a infraestrutura disponível; consulte o guia antes de executar `up`. A API recebe HTTP/8080, sem PFX; HTTPS é terminado pelo Traefik com `websecure` e `myresolver`. Não há porta pública da API no Compose de nuvem. O override local expõe apenas `127.0.0.1:8081` e desabilita Traefik.

O Dockerfile executa as suítes de identidade, core e template e roda a aplicação sem root. Os dotenv são versionados no Git privado nesta etapa e ficam excluídos do contexto do build Docker; o Compose os injeta no runtime.

O template inclui a migration inicial de identidade em inglês, sem entidades demonstrativas. Os serviços gerados usam o prefixo `Owh`, pastas `Domain` com instruções e configurações de identidade independentes. Veja [geração de serviços](TEMPLATE.md).

## Atualização de dependências

As dependências foram conferidas na atualização de 22/10/2026. MassTransit permanece em 8.5.x e MediatR em 12.5.x por decisão explícita do mantenedor, incluindo referências transitivas e serviços gerados. Ver [versões, limites e verificações do monorepo](../docs/dependencias.md). Não houve publicação de pacotes ou alteração de banco como parte desta atualização.
