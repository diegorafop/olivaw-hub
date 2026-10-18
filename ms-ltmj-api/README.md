# ms-ltmj-api

Template ASP.NET Core/.NET 10 para novos clientes do Olivaw Hub. O host mantém suas features, contexto, repositórios e projetos de provedor. Core e identidade são bibliotecas compartilhadas por `ProjectReference` dentro deste monorepo.

## Estrutura

- `src/Mps.Ltmj.Api`: host HTTP, composição, readiness e feature de exemplo.
- `src/Mps.Ltmj`: regras específicas do futuro cliente.
- `src/Mps.Ltmj.Dados`: contexto, repositório genérico e mapeamentos locais.
- `src/Mps.Ltmj.Dados.PostgreSql` e `.SqlServer`: projetos para migrations do cliente.
- `src/Mps.Ltmj.Client`: biblioteca base para um futuro cliente REST.
- `tests/Mps.Ltmj.Tests`: testes das funcionalidades do template.
- `../olivaw-identity`: usuários, perfis, permissões, vínculos e autenticação compartilhados.
- `../ms-olivaw-core-api`: contratos e infraestrutura genéricos.

Oracle permanece reservado; não há integração Oracle validada. Os componentes compartilhados ficam fora da pasta do cliente e devem acompanhar o monorepo no build.

## Gerar e desenvolver

Veja [TEMPLATE.md](TEMPLATE.md). A partir da raiz do monorepo:

```bash
bash ms-ltmj-api/scripts/new-microservice.sh pastelaria-joao
dotnet build OlivawHub.sln -c Release
dotnet test ms-ltmj-api/tests/Mps.Ltmj.Tests/Mps.Ltmj.Tests.csproj -c Release
```

Depois de configurar banco e identidade, execute `dotnet run --project ms-ltmj-api/src/Mps.Ltmj.Api`. Variáveis de ambiente ou user secrets fornecem `ConnectionStrings__DbContext`, `Identity__Issuer`, `Identity__Audience` e `Identity__SigningKey`. Os arquivos `appsettings` não contêm senha de banco ou chave JWT. O `.env` corporativo antigo foi retirado; use o exemplo sem credenciais reais. Configure `DatabaseProvider` como `PostgreSql` ou `SqlServer`; `Database:Schema` pode especificar o schema do histórico de migrations.

Cada organização/cliente recebe seu banco por ambiente; cada serviço usa um schema próprio. O `WriteDbContext` aplica os mapeamentos de identidade junto dos locais; o módulo não tem conexão própria. As rotas compartilhadas são importadas por MVC application parts, sem copiar features para o cliente. Consulte [Olivaw Identity](../olivaw-identity/README.md) para contratos, policies e bootstrap de root.

## Schema e root

Não há migrations herdadas de autenticação. Gere e revise a migration inicial no projeto do provedor do cliente quando decidir criar o banco. A inicialização automática de migrations vem desligada (`Database:ApplyMigrations=false`). Só habilite-a após ter migrations válidas para o consumidor.

Com o schema pronto, habilite temporariamente `Identity__Bootstrap__Enabled=true`, informe `Identity__Bootstrap__Email` e `Identity__Bootstrap__Password` e inicialize uma instância da API. Somente root será criado; depois, retire essa configuração. A senha não é redefinida quando o bootstrap encontra root existente.

## Configuração de nuvem e Docker

Os appsettings por ambiente e overrides Compose seguem os endereços de infraestrutura usados pelo FinTalk. O template usa banco `olivaw_prd`, schema `ms_ltmj` e domínio de produção `https://api.ltmj.olivaw-hub.duckdns.org`. Outros ambientes têm nomes próprios, documentados no [guia de nuvem](../docs/cloud.md).

As configurações dos ambientes estão nos dotenv versionados no repositório privado até a futura migração para Key Vault; o arquivo versionado `.env.example` contém placeholders. As variáveis do Compose substituem as connection strings sem senha dos appsettings. Redis tem connection string preparada, mas não possui integração ativa no código. RabbitMQ é habilitado nos dotenv de nuvem, com prefixo exclusivo por serviço/ambiente.

O contexto de build continua sendo a raiz do monorepo:

```bash
docker build -f ms-ltmj-api/src/Mps.Ltmj.Api/Dockerfile -t olivaw-ltmj .
docker compose --env-file ms-ltmj-api/docker/.env.prd -f ms-ltmj-api/docker/docker-compose.yml -f ms-ltmj-api/docker/docker-compose.prd.yml config --quiet
```

O último comando apenas valida. A subida exige bancos/schemas preparados e a infraestrutura disponível; consulte o guia antes de executar `up`. A API recebe HTTP/8080, sem PFX; HTTPS é terminado pelo Traefik com `websecure` e `myresolver`. Não há porta pública da API no Compose de nuvem. O override local expõe apenas `127.0.0.1:8081` e desabilita Traefik.

O Dockerfile executa as suítes de identidade, core e template e roda a aplicação sem root. Não publique dotenv reais pelo Git nem os copie no build Docker; eles precisam ser fornecidos no ambiente da VM.

## Cliente LTMJ Eventos

Gerado por `bash ms-limpo-api/scripts/new-microservice.sh ltmj ./ms-ltmj-api ltmj` para ser o backend independente de `ms-ltmj-ng`. Nesta entrega há a fundação do template e identidade; não há endpoints de eventos, galeria, comentários ou orçamento implementados. O protótipo público funciona sem API e não persiste pedidos.

Banco por ambiente: `ltmj_dev/qas/rls/stg/prd`; schema `ms_ltmj`. Os dotenv reais estão versionados conforme o padrão atual do Olivaw Hub. As chaves JWT foram geradas exclusivamente para este cliente e ambiente. Migrations, criação de banco e bootstrap estão desabilitados, e RabbitMQ está desabilitado por enquanto. As credenciais de infraestrutura do template são mantidas para o servidor compartilhado; bancos, permissões e usuário exclusivo precisam ser preparados antes da publicação.

O gerador não cria banco nem migrations. Não foi aplicada alteração em dados. O plano de evolução está em [briefing e MVP](../ms-ltmj-ng/docs/briefing-e-mvp.md); a preparação da API está em [infraestrutura](docs/preparacao.md).
