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

O projeto `Mps.Devops.Dados.PostgreSql` contém a migration inicial `DbCreate`, com as tabelas `usuario`, `perfilacesso`, `autorizacao`, `usuarioperfilacesso` e `perfilacessoautorizacao` (nomes em minúsculas das classes de domínio), além da tabela `example`. A conexão define o `SearchPath` para `ms_devops`; a migration não fixa o schema nas operações das tabelas. A database precisa existir antes: em produção, crie `olivaw_prd` no PostgreSQL da VM. A API aplica migrations em todo startup e o `.env.prd` habilita o bootstrap idempotente do usuário `root`; a senha inicial está configurada nesse arquivo.

## Configuração de nuvem e Docker

O ambiente segue a infraestrutura do FinTalk: PostgreSQL em `ms-postgres-db`, RabbitMQ em `ms-rabbit-mq` e roteamento Traefik na rede `traefik-net`. Produção usa database `olivaw_prd`, schema `ms_devops` e URL `https://api.devops.olivaw-hub.duckdns.org`.

Os cinco ambientes possuem appsettings e overrides próprios. Senhas temporárias e chaves JWT ficam nos dotenv versionados no repositório privado, conforme decisão do mantenedor. A migração para Key Vault fica planejada para o futuro. `.env.example` não contém credenciais reais. Redis tem conexão preparada, mas continua sem integração ativa no código.

Valide antes da subida:

```bash
docker compose --env-file ms-devops-api/docker/.env.prd -f ms-devops-api/docker/docker-compose.yml -f ms-devops-api/docker/docker-compose.prd.yml config --quiet
```

O contexto Docker é a raiz do monorepo. A API recebe HTTP/8080 sem PFX e executa sem root; o Traefik termina HTTPS com `websecure` e `myresolver`. Os ambientes de nuvem não publicam portas da API no host. Para acesso local em Docker, o override publica somente `127.0.0.1:8082`.

Consulte o [guia de nuvem](../docs/cloud.md) para usar os dotenv, preparar bancos/schemas e escolher os comandos de subida. Nenhum recurso foi provisionado e nenhuma implantação foi realizada nesta preparação.

O [fluxo de desenvolvimento](../docs/desenvolvimento.md), o [padrão Git Flow](../docs/gitflow.md) e os `AGENTS.md` locais continuam válidos.

## Implantações via SSH

A API envia solicitações ao executor instalado na VM; Git e Docker rodam no host. `POST /implantacoes` exige JWT e permissão `devops.implantar`:

```json
{ "folder": "olivaw-hub/ms-devops-api", "branch": "develop", "environment": "dev" }
```

O retorno é HTTP **202**, com `id`, `status`, `log` e header `Location`. Consulte `GET /implantacoes/{id}` para acompanhar o job. Os ambientes permitidos são `dev`, `qas`, `stg`, `rls` e `prd`. O worker persiste estado e continua independente do container da API, inclusive durante atualização do próprio DevOps.

Na VM, `folder` é relativo a `/home/ec2-user` e limitado à allowlist do executor. O script faz checkout, pull `--ff-only` e Compose com dotenv/override do ambiente e `up -d --build`. Erros de entrada/transporte usam a validação centralizada; falhas operacionais ficam registradas no status `failed` do job.

Veja o [guia do executor](../docs/deploy-executor.md) para entender cada arquivo e instalar script, chave restrita, `known_hosts`, systemd e override Docker. A imagem contém cliente SSH; acesso à VM depende desse provisionamento manual. Nenhum acesso ou instalação na AWS foi realizado nesta implementação.


## Limpeza de disco da VM

`POST /disk-cleanups` agenda limpeza de cache de build e imagens dangling por SSH. `GET /disk-cleanups/{id}` acompanha o job. Acesso exclusivo do root DevOps, sem comandos arbitrários; containers e volumes são preservados. Instalação do executor atualizado e exemplos de curl em [limpeza de disco](../docs/deploy-executor.md#limpeza-de-disco-pela-api-devops).
