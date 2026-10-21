# ms-olivaw-api

Autoridade administrativa do Olivaw em `https://api.olivaw.com.br`. Gerada pelo template `ms-limpo-api`, com projetos e namespaces `Owh.Olivaw` e as bibliotecas compartilhadas de core e identidade.

O Olivaw mantém usuários humanos da operação e contas técnicas dos clientes, como `ltmj` e `lisierp`. Essas contas técnicas acessam os serviços internos pelos perfis e permissões do Olivaw. Os usuários finais da LTMJ ficam exclusivamente na API da LTMJ.

## Identidade

A biblioteca [Olivaw Identity](../olivaw-identity/README.md) fornece login e CRUD. As tabelas são `authorization`, `accessprofile`, `accessprofileauthorization`, `userprofile` e `userprofileaccessprofile`. Contas técnicas usam `isServiceAccount: true`; somente o bootstrap cria root.

O Olivaw emite tokens com permissões e flags assinadas. DevOps e demais consumidores validam esses tokens localmente com o mesmo secret, emissor e audiência internos, sem consultar esta API. Cada ambiente possui seu conjunto de configurações. Clientes possuem secrets e emissores próprios.

Para deploy delegado, o perfil precisa de `devops.deploy` e de uma permissão por pasta, como `devops.deploy:olivaw/ms-ltmj-api`. A conta root da operação tem acesso administrativo. Os usuários comuns e as concessões serão definidos após a publicação; não existem seeds de contas de clientes.

## Banco e ambientes

Os bancos são `olivaw_dev`, `olivaw_qas`, `olivaw_rls`, `olivaw_stg` e `olivaw_prd`, com schema `ms_olivaw`. O mantenedor precisa criar os bancos vazios antes de iniciar os containers. O startup sempre aplica a migration inicial e garante o root configurado, sem flags de habilitação.

As configurações estão nos dotenv versionados em `docker/`. O e-mail do root é `diegorafop@gmail.com`; senha e secret ficam nesses arquivos e não devem ser impressos. Os overrides Compose mantêm nomes de projeto e container exclusivos por ambiente.

```bash
dotnet build ms-olivaw-api/Olivaw.sln -c Release -m:1
docker compose --project-name ms-olivaw-api-prd \
  --env-file ms-olivaw-api/docker/.env.prd \
  -f ms-olivaw-api/docker/docker-compose.yml \
  -f ms-olivaw-api/docker/docker-compose.prd.yml up -d --build
```

O Compose usa a raiz do monorepo como contexto Docker. Para a primeira publicação e a atualização da DevOps, siga [o roteiro de publicação](../docs/identity-architecture.md).


## Leads da jornada pública

`PUT /leads/{externalId}` recebe o snapshot versionado da pasta de projeto. O navegador reúne os dados desde a entrada na jornada e só envia ao confirmar a análise, com e-mail de contato. O corpo contém `schemaVersion`, `revision`, `startedAt`, `entry`, `stage`, `project`, `excluded`, `requirements`, `business`, `email`, `plan` e `events`. Os contratos públicos ficam em `Owh.Olivaw.Client/Features/LeadFeature/Commands`.

- O header `X-Lead-Key` leva uma chave aleatória de 32 bytes, exclusiva da jornada; o banco guarda apenas SHA-256. Não confundir com credenciais administrativas.
- A rota é deliberadamente pública para envio, sem leitura pública. Não fornece acesso a outros leads nem retorna o conteúdo enviado.
- Repetir o mesmo snapshot e revisão confirma o registro existente. Revisões antigas, mudanças na mesma revisão e concorrência retornam `409`; chave diferente retorna `403`.
- Limites: 64 KiB por requisição, 32 mil caracteres no snapshot, 3 mil no requisito escrito, 40 eventos e 20 requisições por minuto por IP observado pelo servidor.
- O endpoint limita logs HTTP a método, caminho e status; não registra corpo ou chave. Não persiste IP ou user agent.
- A entidade `Lead` usa tabela `lead`, PK interna `int`, `ExternalId` único e revisão como token de concorrência. Não altera a identidade corporativa.
- Esta entrega prepara gravação e atualização. Painel administrativo de leads, notificações e políticas operacionais de retenção ficam para uma próxima etapa.

A migration PostgreSQL `Issue013` cria a tabela `lead` e o índice único de `externalid`. Sua aplicação ocorre automaticamente no startup após publicar a API atualizada. Não houve aplicação em bancos locais ou remotos durante a geração. Migrations de outros provedores não foram geradas. Os comandos de geração e revisão do SQL estão no [README do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

Validação, a partir da raiz do monorepo:

```bash
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -c Release -m:1
npm --prefix ms-olivaw-ng test
npm --prefix ms-olivaw-ng run e2e -- project-folder.spec.ts site-studio.spec.ts landing.spec.ts
```

Os testes HTTP usam servidor em memória e repositório simulado. Os mapeamentos PostgreSQL e SQL Server são validados sem conexão a bancos reais.

A organização dos projetos de testes e a expansão para integração e E2E estão no [README de testes](tests/README.md).
