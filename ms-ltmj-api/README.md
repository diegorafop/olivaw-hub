# ms-ltmj-api

Template ASP.NET Core/.NET 10 para novos clientes do Olivaw Hub. O host mantém suas features, contexto, repositórios e projetos de provedor. Core e identidade são bibliotecas compartilhadas por `ProjectReference` dentro deste monorepo.

## Estrutura

- `src/Owh.Ltmj.Api`: host HTTP, composição, readiness e feature de exemplo.
- `src/Owh.Ltmj`: regras específicas do futuro cliente.
- `src/Owh.Ltmj.Dados`: contexto, repositório genérico e mapeamentos locais.
- `src/Owh.Ltmj.Dados.PostgreSql` e `.SqlServer`: projetos para migrations do cliente.
- `src/Owh.Ltmj.Client`: biblioteca base para um futuro cliente REST.
- `tests/Owh.Ltmj.Tests`: testes das funcionalidades do template.
- `../olivaw-identity`: usuários, perfis, permissões, vínculos e autenticação compartilhados.
- `../ms-olivaw-core-api`: contratos e infraestrutura genéricos.

Oracle permanece reservado; não há integração Oracle validada. Os componentes compartilhados ficam fora da pasta do cliente e devem acompanhar o monorepo no build.

## Gerar e desenvolver

Veja [TEMPLATE.md](TEMPLATE.md). A partir da raiz do monorepo:

```bash
bash ms-ltmj-api/scripts/new-microservice.sh pastelaria-joao
dotnet build OlivawHub.sln -c Release
dotnet test ms-ltmj-api/tests/Owh.Ltmj.Tests/Owh.Ltmj.Tests.csproj -c Release
```

Depois de configurar banco e identidade, execute `dotnet run --project ms-ltmj-api/src/Owh.Ltmj.Api`. Variáveis de ambiente ou user secrets fornecem `ConnectionStrings__DbContext`, `Identity__Issuer`, `Identity__Audience` e `Identity__SigningKey`. Os arquivos `appsettings` não contêm senha de banco ou chave JWT. Os dotenv de serviço são versionados no repositório privado conforme a decisão atual do mantenedor. Configure `DatabaseProvider` como `PostgreSql` ou `SqlServer`; `Database:Schema` pode especificar o schema do histórico de migrations.

Cada organização/cliente recebe seu banco por ambiente; cada serviço usa um schema próprio. O `WriteDbContext` aplica os mapeamentos de identidade junto dos locais; o módulo não tem conexão própria. As rotas compartilhadas são importadas por MVC application parts, sem copiar features para o cliente. Consulte [Olivaw Identity](../olivaw-identity/README.md) para contratos, policies e bootstrap de root.

## Schema e root

O startup sempre aplica as migrations pendentes do provedor e depois garante o root antes de atender requisições. Não há flag para habilitar migrations ou bootstrap. Se essa preparação falhar, a API não inicia com o banco incompleto.

O bootstrap é idempotente: se root já existir, preserva sua senha e seu cadastro. Configure `Identity__Bootstrap__Email` e `Identity__Bootstrap__Password` para a primeira inicialização. Root é exclusivo do Olivaw Hub; contas de clientes recebem somente as permissões de negócio.

Os arquivos de migrations precisam acompanhar o projeto. O startup aplica arquivos existentes; geração de novas migrations é uma etapa de desenvolvimento. O contexto de design time PostgreSQL permite gerar arquivos sem inicializar a API nem acessar a AWS.

## Configuração de nuvem e Docker

Os appsettings por ambiente e overrides Compose seguem os endereços de infraestrutura usados pelo FinTalk. O template usa banco `olivaw_prd`, schema `ms_ltmj` e domínio de produção `https://api.ltmj.olivaw-hub.duckdns.org`. Outros ambientes têm nomes próprios, documentados no [guia de nuvem](../docs/cloud.md).

As configurações dos ambientes estão nos dotenv versionados no repositório privado até a futura migração para Key Vault; o arquivo versionado `.env.example` contém placeholders. As variáveis do Compose substituem as connection strings sem senha dos appsettings. Redis tem connection string preparada, mas não possui integração ativa no código. RabbitMQ é habilitado nos dotenv de nuvem, com prefixo exclusivo por serviço/ambiente.

O contexto de build continua sendo a raiz do monorepo:

```bash
docker build -f ms-ltmj-api/src/Owh.Ltmj.Api/Dockerfile -t olivaw-ltmj .
docker compose --env-file ms-ltmj-api/docker/.env.prd -f ms-ltmj-api/docker/docker-compose.yml -f ms-ltmj-api/docker/docker-compose.prd.yml config --quiet
```

O último comando apenas valida. A subida exige bancos/schemas preparados e a infraestrutura disponível; consulte o guia antes de executar `up`. A API recebe HTTP/8080, sem PFX; HTTPS é terminado pelo Traefik com `websecure` e `myresolver`. Não há porta pública da API no Compose de nuvem. O override local expõe apenas `127.0.0.1:8081` e desabilita Traefik.

O Dockerfile executa as suítes de identidade, core e template e roda a aplicação sem root. Não publique dotenv reais pelo Git nem os copie no build Docker; eles precisam ser fornecidos no ambiente da VM.

## Cliente LTMJ Eventos

Gerado por `bash ms-limpo-api/scripts/new-microservice.sh ltmj ./ms-ltmj-api ltmj` para ser o backend independente de `ms-ltmj-ng`. Nesta entrega há a fundação do template e identidade; não há endpoints de eventos, galeria, comentários ou orçamento implementados. O protótipo público funciona sem API e não persiste pedidos.

Banco por ambiente: `ltmj_dev/qas/rls/stg/prd`; schema `ms_ltmj`. Os dotenv reais estão versionados conforme o padrão atual do Olivaw Hub. As chaves JWT foram geradas exclusivamente para este cliente e ambiente. Migrations pendentes e bootstrap do root rodam automaticamente no startup; RabbitMQ permanece desabilitado por enquanto. As credenciais de infraestrutura do template são mantidas para o servidor compartilhado; bancos, permissões e usuário exclusivo precisam ser preparados antes da publicação.

O gerador não cria bancos. A migration inicial foi recriada para uma base vazia; veja [migrations](docs/migrations.md). Não foi aplicada alteração em dados. O plano de evolução está em [briefing e MVP](../ms-ltmj-ng/docs/briefing-e-mvp.md); a preparação da API está em [infraestrutura](docs/preparacao.md).

A identidade deste cliente é isolada do Hub: tabelas e contratos em inglês, secret e emissor próprios. A conta técnica `ltmj` no Hub é independente da conta da equipe.
