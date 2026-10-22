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

`PUT /leads/{externalId}` recebe o snapshot versionado da pasta de projeto. O navegador reúne os dados desde a entrada na jornada e só envia ao confirmar a análise, com ao menos um contato válido (e-mail ou WhatsApp). O corpo contém `schemaVersion`, `revision`, `startedAt`, `entry`, `stage`, `project`, `excluded`, `requirements`, `business`, `email`, `whatsapp`, `plan` e `events`. Os contratos públicos ficam em `Owh.Olivaw.Client/Features/LeadFeature/Commands`.

- `email` é opcional quando há `whatsapp` válido. O WhatsApp usa DDD e celular brasileiro: 11 dígitos sem máscara, como `11987654321`. Se os dois contatos forem preenchidos, ambos devem ser válidos. O contato direto e a última etapa da jornada do Estúdio oferecem as duas opções, exigindo ao menos uma válida. O novo campo fica no snapshot JSON e não exige migration adicional.
- O header `X-Lead-Key` leva uma chave aleatória de 32 bytes, exclusiva da jornada; o banco guarda apenas SHA-256. Não confundir com credenciais administrativas.
- A rota é deliberadamente pública para envio, sem leitura pública. Não fornece acesso a outros leads nem retorna o conteúdo enviado.
- Repetir o mesmo snapshot e revisão confirma o registro existente. Revisões antigas, mudanças na mesma revisão e concorrência retornam `409`; chave diferente retorna `403`.
- Limites: 64 KiB por requisição, 32 mil caracteres no snapshot, 3 mil no requisito escrito, 40 eventos e 20 requisições por minuto por IP observado pelo servidor.
- O endpoint limita logs HTTP a método, caminho e status; não registra corpo ou chave. Não persiste IP ou user agent.
- A entidade `Lead` usa tabela `lead`, PK interna `int`, `ExternalId` único e revisão como token de concorrência. Não altera a identidade corporativa.
- Esta entrega prepara gravação e atualização. A leitura administrativa está descrita abaixo; acompanhamento comercial, notificações e políticas operacionais de retenção permanecem pendentes.

A migration PostgreSQL `Issue013` cria a tabela `lead` e o índice único de `externalid`. Sua aplicação ocorre automaticamente no startup após publicar a API atualizada. Não houve aplicação em bancos locais ou remotos durante a geração. Migrations de outros provedores não foram geradas. Os comandos de geração e revisão do SQL estão no [README do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

Validação, a partir da raiz do monorepo:

```bash
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -c Release -m:1
npm --prefix ms-olivaw-ng test
npm --prefix ms-olivaw-ng run e2e -- project-folder.spec.ts site-studio.spec.ts landing.spec.ts
```

Os testes HTTP usam servidor em memória e repositório simulado. Os mapeamentos PostgreSQL e SQL Server são validados sem conexão a bancos reais.

A organização dos projetos de testes e a expansão para integração e E2E estão no [README de testes](tests/README.md).

## Consulta administrativa de leads

Consultas disponíveis somente com JWT válido da identidade interna e claim `root=true`, pela policy `olivaw.root`. O painel usa a identidade atual; contas comuns, técnicas e a chave pública `X-Lead-Key` não concedem leitura administrativa. Não há nova tabela ou migration nesta entrega.

| Operação | Contrato |
| --- | --- |
| `GET /leads` | Lista paginada de metadados, negócio, título, origem e contatos; sem snapshot completo |
| `GET /leads/{externalId}` | Ficha com o snapshot público recebido, identificação externa e datas; 404 se ausente ou removido |
| `GET /leads/summary` | Total recebido e total nos últimos sete dias, com início da janela e instante da consulta |

Filtros de lista: `page` (1 a 100000, padrão 1), `pageSize` (1 a 100, padrão 20), `search` (até 100 caracteres), `entry` (`studio` ou `discovery`), `from` (inclusivo) e `until` (exclusivo). Datas aceitam ISO 8601 com fuso e são comparadas em UTC. A interface converte os dias escolhidos no fuso local em limites de instantes; horários são apresentados no fuso do navegador. A origem `discovery` inclui descoberta e contato direto, conforme os snapshots existentes.

A busca é insensível a maiúsculas/minúsculas nos campos negócio, título, e-mail e WhatsApp. O telefone é pesquisado pelos dígitos armazenados, sem máscara. Não pesquisa todo o texto dos requisitos. A lista ordena por recebimento decrescente e identificador externo como desempate. Somente registros sem remoção lógica entram na leitura e nos totais. As projeções JSON usam PostgreSQL ou SQL Server e paginação no banco, sem carregar toda a base na API.

Respostas não expõem `WriteKeyHash`, PK interna ou `ActorUserId` da entidade. As rotas desabilitam cache HTTP e limitam logs a método, caminho e status, sem corpo, contatos ou query de busca. Resumos não representam leads novos/não lidos: não existe estado comercial persistido nesta entrega. Status, responsável e notas deverão ter persistência própria, sem modificar o snapshot ou sua revisão pública.

Os snapshots gravados pelo endpoint público são JSON válido. Registros antigos sem WhatsApp são aceitos; dados externos que alterem manualmente o formato do JSON estão fora do contrato de persistência. Detalhes com JSON ilegível informam `contentUnavailable`, sem expor o texto bruto.

Validação local, a partir da raiz:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Api --no-restore -m:1
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
dotnet pack ms-olivaw-api/src/Owh.Olivaw.Client/Owh.Olivaw.Client.csproj --no-build --no-restore -c Debug -o /tmp/olivaw-lead-read-pack
```

O comando de pack usa a compilação Debug criada pelos comandos anteriores; não publica o pacote. Testes de HTTP e handlers usam repositórios simulados. Tradução das consultas é verificada para os dois provedores sem banco real; execução integrada no banco do ambiente ainda deve ser conferida após publicar a API. Esta tarefa não aplica migrations nem executa redeploy.
