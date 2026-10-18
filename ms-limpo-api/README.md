# ms-limpo-api

Template ASP.NET Core/.NET 10 para novos clientes do Olivaw. O host mantém suas features, contexto, repositórios e projetos de provedor. Core e identidade são bibliotecas compartilhadas por `ProjectReference` dentro deste monorepo.

## Estrutura

- `src/Owh.Limpo.Api`: host HTTP, composição, readiness e feature de exemplo.
- `src/Owh.Limpo`: regras e contratos específicos do futuro cliente, com as pastas permanentes `Repositories`, `Services` e `Requests`.
- `src/Owh.Limpo.Dados`: contexto, repositório genérico e mapeamentos locais.
- `src/Owh.Limpo.Dados.PostgreSql` e `.SqlServer`: projetos para migrations do cliente.
- `src/Owh.Limpo.Client`: biblioteca base para um futuro cliente REST.
- `tests/Owh.Limpo.Tests`: testes das funcionalidades do template.
- `../olivaw-identity`: usuários, perfis, permissões, vínculos e autenticação compartilhados.
- `../ms-olivaw-core-api`: contratos e infraestrutura genéricos.

## Repositórios

Use `IRepository<T>` para CRUD e consultas comuns com filtros por expressão. Não crie um repository ou reader específico quando os métodos genéricos atenderem. Para uma consulta realmente especializada, uma classe como `PortfolioRepository : Repository<PortfolioEvent>` pode acrescentar o comportamento necessário e herda o contrato genérico, aproveitando o código base.

## Serviços e requests externos

`Owh.Limpo/Services` guarda interfaces pequenas para capacidades externas que não são persistência, como armazenamento de arquivos, APIs de terceiros e mensageria; suas implementações ficam nas camadas concretas. `Owh.Limpo/Requests` guarda contratos de Commands/Queries enviados a outros microservices, organizados por serviço e feature. Requests e handlers MediatR das próprias features continuam em `Owh.Limpo.Api`.

Oracle permanece reservado; não há integração Oracle validada. Os componentes compartilhados ficam fora da pasta do cliente e devem acompanhar o monorepo no build.

## Gerar e desenvolver

Veja [TEMPLATE.md](TEMPLATE.md). A partir da raiz do monorepo:

```bash
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
dotnet build Olivaw.sln -c Release
dotnet test ms-limpo-api/tests/Owh.Limpo.Tests/Owh.Limpo.Tests.csproj -c Release
```

Depois de configurar banco e identidade, execute `dotnet run --project ms-limpo-api/src/Owh.Limpo.Api`. Variáveis de ambiente ou user secrets fornecem `ConnectionStrings__DbContext`, `Identity__Issuer`, `Identity__Audience` e `Identity__SigningKey`. Os arquivos `appsettings` não contêm senha de banco ou chave JWT. Os dotenv de serviço são versionados no repositório privado conforme a decisão atual do mantenedor. Configure `DatabaseProvider` como `PostgreSql` ou `SqlServer`; `Database:Schema` pode especificar o schema do histórico de migrations.

Cada organização/cliente recebe seu banco por ambiente; cada serviço usa um schema próprio. O `WriteDbContext` aplica os mapeamentos de identidade junto dos locais; o módulo não tem conexão própria. As rotas compartilhadas são importadas por MVC application parts, sem copiar features para o cliente. Consulte [Olivaw Identity](../olivaw-identity/README.md) para contratos, policies e bootstrap de root.

## Schema e root

O startup sempre aplica as migrations pendentes do provedor e depois garante o root antes de atender requisições. Não há flag para habilitar migrations ou bootstrap. Se essa preparação falhar, a API não inicia com o banco incompleto.

O bootstrap é idempotente: se root já existir, preserva sua senha e seu cadastro. Configure `Identity__Bootstrap__Email` e `Identity__Bootstrap__Password` para a primeira inicialização. Root é exclusivo do Olivaw; contas de clientes recebem somente as permissões de negócio.

Os arquivos de migrations precisam acompanhar o projeto. O startup aplica arquivos existentes; geração de novas migrations é uma etapa de desenvolvimento. O contexto de design time PostgreSQL permite gerar arquivos sem inicializar a API nem acessar a AWS.

## Configuração de nuvem e Docker

Os appsettings por ambiente e overrides Compose seguem os endereços de infraestrutura usados pelo FinTalk. O template usa banco `olivaw_prd`, schema `ms_limpo` e domínio de produção `https://api.limpo.olivaw-hub.duckdns.org`. Outros ambientes têm nomes próprios, documentados no [guia de nuvem](../docs/cloud.md).

As configurações dos ambientes estão nos dotenv versionados no repositório privado até a futura migração para Key Vault; o arquivo versionado `.env.example` contém placeholders. As variáveis do Compose substituem as connection strings sem senha dos appsettings. Redis tem connection string preparada, mas não possui integração ativa no código. RabbitMQ é habilitado nos dotenv de nuvem, com prefixo exclusivo por serviço/ambiente.

O contexto de build continua sendo a raiz do monorepo:

```bash
docker build -f ms-limpo-api/src/Owh.Limpo.Api/Dockerfile -t olivaw-limpo .
docker compose --env-file ms-limpo-api/docker/.env.prd -f ms-limpo-api/docker/docker-compose.yml -f ms-limpo-api/docker/docker-compose.prd.yml config --quiet
```

O último comando apenas valida. A subida exige bancos/schemas preparados e a infraestrutura disponível; consulte o guia antes de executar `up`. A API recebe HTTP/8080, sem PFX; HTTPS é terminado pelo Traefik com `websecure` e `myresolver`. Não há porta pública da API no Compose de nuvem. O override local expõe apenas `127.0.0.1:8081` e desabilita Traefik.

O Dockerfile executa as suítes de identidade, core e template e roda a aplicação sem root. Os dotenv são versionados no Git privado nesta etapa e ficam excluídos do contexto do build Docker; o Compose os injeta no runtime.

O template inclui a migration inicial de identidade em inglês, sem entidades demonstrativas. Os serviços gerados usam o prefixo `Owh`, pastas `Domain` com instruções e configurações de identidade independentes. Veja [geração de serviços](TEMPLATE.md).
