# ms-devops-api

API .NET 10 do Olivaw Hub, gerada a partir de `ms-limpo-api`. A estrutura inicial está pronta; as funcionalidades específicas de DevOps ainda serão definidas.

## Estrutura

- `Devops.sln`: solução local do serviço.
- `src/Mps.Devops.Api`: host HTTP, composição, readiness, versão e feature de exemplo herdada do template.
- `src/Mps.Devops`: domínio e contratos específicos do serviço.
- `src/Mps.Devops.Dados`: contexto, repositórios e mapeamentos locais.
- `src/Mps.Devops.Dados.PostgreSql` e `.SqlServer`: projetos de provedor para as migrations do serviço.
- `src/Mps.Devops.Client`: biblioteca base para futuro cliente REST.
- `tests/Mps.Devops.Tests`: testes herdados do template, incluindo integração de identidade.

Oracle permanece reservado, sem integração validada. Core e identidade são importados por `ProjectReference` das pastas irmãs do monorepo. As funcionalidades de autenticação não são copiadas para o serviço; seu `WriteDbContext` aplica os mapeamentos compartilhados no schema `ms_devops` do banco Olivaw do ambiente.

## Desenvolvimento

Na raiz do monorepo:

```bash
dotnet build ms-devops-api/Devops.sln -c Release
dotnet test ms-devops-api/tests/Mps.Devops.Tests/Mps.Devops.Tests.csproj -c Release --no-build
```

Os projetos também estão em `OlivawHub.sln`. Antes de executar a API, configure por ambiente ou secret store:

```text
ConnectionStrings__DbContext=<conexão do banco Olivaw com SearchPath=ms_devops>
Identity__Issuer=ms-devops-api
Identity__Audience=ms-devops-api
Identity__SigningKey=<chave aleatória exclusiva com pelo menos 32 bytes>
```

Depois de preparar o schema:

```bash
dotnet run --project ms-devops-api/src/Mps.Devops.Api/Mps.Devops.Api.csproj -c Release --no-launch-profile
```

Nenhuma migration ou banco foi criado na geração. Aplicação de migrations e bootstrap de root vêm desabilitados. O [guia de identidade](../olivaw-identity/README.md) explica como preparar o usuário inicial com senha externa; não há senha padrão.

## Configuração de nuvem e Docker

O ambiente segue a infraestrutura do FinTalk: PostgreSQL em `ms-postgres-db`, RabbitMQ em `ms-rabbit-mq` e roteamento Traefik na rede `traefik-net`. Produção usa banco `olivaw_prd`, schema `ms_devops` e URL `https://ms-devops-api.fintalk-server.duckdns.org`.

Os cinco ambientes possuem appsettings e overrides próprios. Senhas temporárias e chaves JWT ficam nos dotenv locais ignorados pelo Git. `.env.example` não contém credenciais reais. Redis tem conexão preparada, mas continua sem integração ativa no código.

Valide antes da subida:

```bash
docker compose --env-file ms-devops-api/docker/.env.prd -f ms-devops-api/docker/docker-compose.yml -f ms-devops-api/docker/docker-compose.prd.yml config --quiet
```

O contexto Docker é a raiz do monorepo. A API recebe HTTP/8080 sem PFX e executa sem root; o Traefik termina HTTPS com `websecure` e `myresolver`. Os ambientes de nuvem não publicam portas da API no host. Para acesso local em Docker, o override publica somente `127.0.0.1:8082`.

Consulte o [guia de nuvem](../docs/cloud.md) para usar os dotenv, preparar bancos/schemas e escolher os comandos de subida. Nenhum recurso foi provisionado e nenhuma implantação foi realizada nesta preparação.

O [fluxo de desenvolvimento](../docs/desenvolvimento.md), o [padrão Git Flow](../docs/gitflow.md) e os `AGENTS.md` locais continuam válidos.

## Implantações

`POST /implantacoes` recebe um comando CQS:

```json
{ "folder": "olivaw-hub/ms-devops-api", "branch": "develop", "environment": "dev" }
```

Configure `Deployment__RootDirectory` com uma raiz absoluta e confiável no servidor (na VM informada: `/home/ec2-user`) e, opcionalmente, `Deployment__TimeoutSeconds` (padrão 300, máximo 3600, por comando). `folder` aponta para a raiz do microserviço e é relativo à raiz permitida; aceita segmentos com letras, números, `_` e `-`, separados por `/`. Caminhos absolutos, `..`, metacaracteres e links simbólicos são rejeitados. Deve existir um repositório Git na pasta ou em um ancestral dentro da raiz autorizada.

O JWT deve pertencer a um usuário root ou ter a autorização `devops.implantar`, vinculada ao perfil do usuário. O serviço executa `git checkout <branch>`, `git pull --ff-only` e o Compose do ambiente solicitado, nessa ordem, usando `WorkingDirectory` e argumentos separados, sem shell. `environment` é obrigatório e aceita somente `dev`, `qas`, `stg`, `rls` ou `prd`. O comando executado na raiz do microserviço é:

```bash
docker compose --env-file docker/.env.dev -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up -d
```

O sufixo `dev` é substituído pelo ambiente validado, inclusive no dotenv. Os arquivos existentes usam `.yml` e `docker-compose.<ambiente>.yml`. O `--env-file` garante a interpolação das variáveis do ambiente correto. O branch precisa existir localmente para checkout ou ser resolvido pelo Git; o pull usa seu upstream configurado.

Sucesso retorna HTTP 200 com `log` das etapas/códigos de saída. Entrada inválida, pasta indisponível, Git/Docker ausente, saída não zero ou timeout passam pelo filtro centralizado do core, com HTTP 400 e `ValidationProblemDetails` (`title`, `status`, `errors`). A saída bruta dos processos é descartada para evitar expor credenciais; falhas interrompem a sequência, sem rollback automático. O cancelamento da requisição encerra o processo e seus descendentes. Implantações são serializadas por instância; execute uma única réplica enquanto não houver coordenação distribuída.

A execução ocorre na máquina/container da API; não há SSH para a AWS nesta feature. O ambiente executor precisa de Git, Docker Compose, credenciais Git não interativas, acesso aos projetos e permissão para usar o daemon Docker. O Dockerfile atual não fornece essa infraestrutura. Não foi montado socket Docker nem habilitado acesso ao host automaticamente. A raiz e os repositórios devem ser administrados somente por operadores confiáveis, sem alterações concorrentes de filesystem durante a implantação.

Os testes cobrem validator, handler, controller, argumentos dos processos, proteção de caminhos e HTTP real em TestServer com JWT e `IIdentityStore`/`IProcessExecutionService` mockados. Não executam Git/Docker nem dependem da VM.

### Caminhos na VM AWS

Na estrutura informada, configure `Deployment__RootDirectory=/home/ec2-user`. O payload com `folder=olivaw-hub/ms-devops-api` executa os comandos em `/home/ec2-user/olivaw-hub/ms-devops-api`. Exemplo equivalente no host, para `dev`:

```bash
cd /home/ec2-user/olivaw-hub/ms-devops-api
git checkout develop
git pull --ff-only
docker compose --env-file docker/.env.dev -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up -d
```

A raiz é configuração do executor, não um campo do payload. Se a API estiver em container, caminhos, ferramentas e acesso ao daemon precisam ser disponibilizados explicitamente ao executor; a presença desses recursos no host não os torna disponíveis dentro do container. O acesso Git também precisa funcionar com o usuário efetivo do executor, sem prompts. Não há conexão SSH automática nem alteração da VM nesta feature.
