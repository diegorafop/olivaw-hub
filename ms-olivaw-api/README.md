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

## Cadastro administrativo de clientes

Disponível no código para avaliação, exclusivo da policy `olivaw.root`. A migration PostgreSQL foi gerada; sua aplicação e o teste no ambiente ainda dependem da publicação pelo mantenedor. Cliente representa a empresa e não cria usuário, `UserProfile`, conta técnica ou credencial.

| Método | Recurso | Resultado |
| --- | --- | --- |
| GET | `/customers?page=1&pageSize=20&search=&status=` | Lista paginada, total e itens resumidos |
| GET | `/customers/summary` | Contagens por situação e momento da consulta |
| GET | `/customers/{externalId}` | Ficha, contatos, origem e até 20 alterações recentes |
| POST | `/customers` | Cadastro em implantação, ID público e revisão; HTTP 201 |
| PUT | `/customers/{externalId}` | Atualização com revisão esperada; HTTP 200 |

Payloads públicos em `Owh.Olivaw.Client/Features/CustomerFeature`. Nome comercial e de um a oito contatos são obrigatórios; cada contato tem nome e pelo menos e-mail ou WhatsApp válido. WhatsApp contém DDD e celular de 11 dígitos, sem máscara. Ambos os canais preenchidos devem ser válidos. Razão social, CNPJ (14 números), responsável interno e observações são opcionais. Não há validação fiscal externa.

`sourceLeadExternalId` é opcional e permitido apenas na criação; preserva o vínculo com o lead existente. Origem repetida retorna HTTP 409 e o ID público do cliente vinculado. CNPJ duplicado e revisão desatualizada também retornam 409. Campos inválidos retornam 400 e recursos ausentes 404. Status aceitos: `onboarding`, `active`, `paused`, `closed`, com as transições documentadas na [feature 03](../docs/features/painel-olivaw/03-cadastro-de-clientes.md). Alterar status não suspende sistemas nem afeta cobrança.

O servidor obtém operador e identidade do JWT, sem aceitar auditoria do body. Respostas não expõem chaves internas ou dados de autenticação. A trilha mínima registra criação/atualização e mudanças de situação; não guarda versões completas dos dados anteriores.

As leituras simples e escritas usam `IRepository<Customer>`. `IRepositoryCustomer` é restrito à paginação/contagem e agregação por situação no banco, operações ausentes no genérico. O modelo inclui `customer`, `customercontact` e `customerchange`, com FK internas, contatos relacionais, índice único opcional de CNPJ e de lead de origem, e revisão para concorrência otimista.

**Migration PostgreSQL pronta:** `20261022010058_Issue013Customers` cria somente `customer`, `customercontact` e `customerchange`, seus vínculos e índices. Migration, designer, snapshot e SQL entre `Issue013` e `Issue013Customers` foram revisados; não há alterações em tabelas de leads ou identidade. `has-pending-model-changes` confirmou modelo sincronizado. Nenhum banco foi acessado para gerar ou revisar estes artefatos. SQL Server e Oracle não receberam migrations nesta entrega.

**Ao publicar:** o startup da API aplica automaticamente as migrations pendentes. Os arquivos foram gerados localmente; a nova feature ainda não foi publicada e não foi testada contra PostgreSQL real. O mantenedor fará o redeploy e a validação na AWS. O deploy de leads concluído anteriormente não contém esta feature.

Comandos usados na raiz do monorepo. O `migrations add` documenta a geração original; não repetir quando a migration já estiver no checkout:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations add Issue013Customers \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build

dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations script Issue013 Issue013Customers \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build \
  --output /tmp/olivaw-issue013-customers.sql

dotnet ef migrations has-pending-model-changes \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build
```

Esses comandos usam a factory de design time, sem iniciar o host ou aplicar alterações. Ver também os [comandos do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

Validação local, sem banco real:

```bash
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -c Release -m:1
dotnet pack ms-olivaw-api/src/Owh.Olivaw.Client/Owh.Olivaw.Client.csproj -c Release --no-build -o /tmp/olivaw-customer-pack
```
