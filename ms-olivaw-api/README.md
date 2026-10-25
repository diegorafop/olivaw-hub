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

Consultas disponíveis com JWT válido da identidade interna e root ou permissão vigente `leads.manage`, pela policy homônima. A chave pública `X-Lead-Key` não concede leitura administrativa. O recorte original de consulta não alterou o schema; o acompanhamento acrescenta a migration `Issue017`, descrita adiante.

| Operação | Contrato |
| --- | --- |
| `GET /leads` | Lista paginada de metadados, negócio, título, origem e contatos; sem snapshot completo |
| `GET /leads/{externalId}` | Ficha com o snapshot público recebido, identificação externa e datas; 404 se ausente ou removido |
| `GET /leads/summary` | Total recebido e total nos últimos sete dias, com início da janela e instante da consulta |

Filtros de lista: `page` (1 a 100000, padrão 1), `pageSize` (1 a 100, padrão 20), `search` (até 100 caracteres), `entry` (`studio` ou `discovery`), `from` (inclusivo), `until` (exclusivo) e `followUpStatus` (uma das etapas do acompanhamento). Datas aceitam ISO 8601 com fuso e são comparadas em UTC. A interface converte os dias escolhidos no fuso local em limites de instantes; horários são apresentados no fuso do navegador. A origem `discovery` inclui descoberta e contato direto, conforme os snapshots existentes.

A busca é insensível a maiúsculas/minúsculas nos campos negócio, título, e-mail e WhatsApp. O telefone é pesquisado pelos dígitos armazenados, sem máscara. Não pesquisa todo o texto dos requisitos. A lista ordena por recebimento decrescente e identificador externo como desempate. Somente registros sem remoção lógica entram na leitura e nos totais. As projeções JSON usam PostgreSQL ou SQL Server e paginação no banco, sem carregar toda a base na API.

Respostas não expõem `WriteKeyHash`, PK interna ou `ActorUserId` da entidade. As rotas desabilitam cache HTTP e limitam logs a método, caminho e status, sem corpo, contatos ou query de busca. Os totais de recebimento não representam não lidos nem pendências. O acompanhamento possui estado persistido próprio e contagens por etapa em `byStatus`, conforme o contrato adiante, preservando snapshot e revisão pública. Não existe responsável atribuído.

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

`sourceLeadExternalId` é opcional e permitido apenas na criação; preserva o vínculo com o lead existente. Origem repetida retorna HTTP 409 e o ID público do cliente vinculado. CNPJ duplicado e revisão desatualizada também retornam 409. Campos inválidos retornam 400 e recursos ausentes 404. Status aceitos: `onboarding`, `active`, `paused`, `closed`, com as transições documentadas na [feature 03](../docs/features/done/03-cadastro-de-clientes.md). Alterar status não suspende sistemas nem afeta cobrança.

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

## Acompanhamento comercial de leads

Implementado em 22/10/2026, ainda sem publicação ou validação contra banco real. Os endpoints administrativos de leads exigem root ou a permissão `leads.manage`, conferida pelo `PermissionHandler` da identidade local a cada autorização. Remover a permissão impede o acesso mesmo quando um token antigo ainda contém a claim. Clientes e seus endpoints continuam exclusivos de root. Não foram criados perfis/usuários ou vínculos de portal.

- `PUT /leads/{externalId}/follow-up`: recebe `{ "revision": 0, "status": "conversation", "note": "Aguardando fotos." }`; retorna a nova `revision`. `revision` é a revisão esperada do acompanhamento e começa em zero. Uma nota é obrigatória quando a etapa não muda. Notas aceitam até 3.000 caracteres; autor e data vêm do servidor, sem edição/exclusão de histórico.
- `GET /leads/{externalId}/history?beforeRevision=21`: histórico em ordem de revisão decrescente, limitado a 20 itens. A primeira página omite `beforeRevision`; `nextBeforeRevision` indica a página anterior ou é nulo quando terminou.
- `GET /leads`: filtro opcional `followUpStatus` e etapa de acompanhamento em cada item.
- `GET /leads/{externalId}`: acrescenta `followUpStatus`, `followUpRevision` e `followUpUpdatedAt`, separados dos dados públicos.
- `GET /leads/summary`: acrescenta `byStatus`, com contagens gerais por etapa, independentes dos filtros da lista; estados ausentes representam zero. `total` e `recent` mantêm seu significado de recebimento.

Etapas persistidas: `new` (Novo contato), `conversation` (Em conversa), `analysis` (Em análise), `development` (Em desenvolvimento), `validation` (Em validação), `delivered` (Entregue) e `discontinued` (Sem continuidade). Transições são livres; não existe responsável atribuído. Nenhuma movimentação publica aplicações, cria clientes ou altera contratos.

`Lead.FollowUpRevision` e a revisão pública são tokens de concorrência no EF. A atualização do lead e a inclusão do histórico são salvas na mesma unidade transacional; conflito retorna HTTP 409. As respostas usam `Cache-Control: no-store`. Movimentações internas preservam `Stage`, `Revision`, `Snapshot` e a data do envio público. Reenvios públicos mantêm a idempotência e preservam o acompanhamento; seu contrato não recebe notas internas.

### Migration Issue017

Uma única migration PostgreSQL, `20261022232507_Issue017`, gerada ao final após estabilizar o modelo. Acrescenta os três campos de acompanhamento em `lead`, índice de etapa e tabela `leadchange` com FK interna restritiva e índices únicos de `externalid` e `(leadid, revision)`. Leads existentes começam em Novo contato, revisão zero, sem criar histórico fictício. Nenhum prazo de expurgo automático foi definido.

Migration, designer, snapshot e SQL entre `Issue013Customers` e `Issue017` foram revisados. Não alteram tabelas de identidade/clientes, nem snapshots existentes. `has-pending-model-changes` confirmou sincronização. **O startup aplica migrations pendentes após publicar a API**; não houve aplicação, conexão a banco real ou redeploy nesta entrega. SQL Server teve mapeamento e tradução SQL verificados sem conexão, mas não recebeu migration; Oracle não foi contemplado.

Comandos reproduzíveis na raiz do monorepo; a geração já foi executada e não deve ser repetida no mesmo checkout:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations add Issue017 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build

dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations script Issue013Customers Issue017 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build \
  --output /tmp/olivaw-issue017.sql

dotnet ef migrations has-pending-model-changes \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build
```

Esses comandos usam a factory de design time, sem iniciar o host. Ferramenta EF 10.0.12 emitiu aviso de patch inferior ao runtime 10.0.13; não houve atualização de dependências nesta tarefa. Consulte também os [comandos do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

Validação: `dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1` (125 testes), build Release, tradução SQL/modelo PostgreSQL e SQL Server e testes HTTP em memória de autenticação, revogação de permissão, validação e conflito. Execução real do SQL e smoke test após publicação continuam pendentes.

## Serviços e vínculos com clientes

Base 04a implementada na issue 018, ainda sem migration nem publicação. Cadastro manual de aplicações lógicas, distinto de recursos e de provisionamento. Root interno pode executar:

- `POST /services`: payload com `technicalId`, `name`, `type`, `description`, `owner`, `scope`, `status` e `customerExternalIds` (até 100 GUIDs públicos). Retorna HTTP 201 com `externalId` e `revision`.
- `PUT /services/{externalId}`: mesmos campos de dados, sem `technicalId`, e revisão esperada `revision`. Vínculos não enviados são encerrados. Conflito retorna 409 e não há edição do identificador técnico.
- `GET /services`: `page`/`pageSize` (máximo 100), `search`, `status`, `type` e `customerExternalId` opcional. Paginação, filtro, contagem e projeção são executados no banco. Contagem de clientes considera vínculos atuais, sem duplicar aplicação compartilhada.
- `GET /services/{externalId}`: dados atuais, vínculos ativos, 20 encerramentos e 20 snapshots mais recentes do cadastro. Não retorna PKs internas ou credenciais.

Tipos: `frontend`, `api`, `worker`, `scheduled`, `other`. Situações: `planned`, `onboarding`, `active`, `closed`, com transições livres. Escopos: `internal`, sem clientes; `customer`, com ao menos um cliente enquanto não encerrado. Um serviço encerrado tem lista vazia de vínculos ativos. O identificador usa até 80 caracteres em minúsculas, números e hífens; nome até 120, descrição até 1.000 e responsável textual até 120.

`managedservice`, `managedservicecustomer` e `managedservicechange` mantêm PK/FK internas, identidade pública, períodos e autoria. Responsável operacional é texto opcional; autor é obtido da sessão no servidor. Reativação cria novo período de vínculo e preserva os anteriores. Novo serviço e seus vínculos/auditoria são salvos juntos; atualizações usam revisão concorrente e índice único para impedir duplicatas. Respostas não são armazenadas em cache; logs dos endpoints omitem corpos e consultas.

**Schema PostgreSQL coberto pela `Issue020`:** a correção descrita abaixo inclui serviços e a base já mapeada do inventário. O startup aplica a migration após publicar a API. Nenhum banco foi alterado durante a geração/revisão; não executar `database update` como validação local.

Validação sem bancos reais:

```bash
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
dotnet build ms-olivaw-api/src/Owh.Olivaw.Api -c Release --no-restore -m:1
dotnet pack ms-olivaw-api/src/Owh.Olivaw.Client -c Release --no-build --no-restore --output /tmp/olivaw-issue018-pack
```

Modelo e tradução de filtros foram verificados para PostgreSQL e SQL Server; Oracle não contemplado. Sem descoberta de nuvem, medições, dados reais de clientes, sementes ou execução de infraestrutura. Veja o [recorte entregue](../docs/features/done/04a-cadastro-de-servicos.md) e o [inventário restante](../docs/features/discovery/painel-olivaw/04-inventario-de-recursos.md).


## Correção de startup — Issue020

O modelo de serviços e da base do inventário havia sido publicado sem migration correspondente. O EF Core interrompia o startup com `PendingModelChangesWarning`. A migration PostgreSQL `20261023010055_Issue020` sincroniza o snapshot e cria 11 tabelas novas, seus índices e FKs: serviços/histórico/clientes, recursos/histórico/revisão da topologia, implantações/histórico e os três vínculos temporais. O SQL de avanço não altera nem remove tabelas/dados existentes. Não foram modificadas migrations anteriores.

O estágio final do Dockerfile instala `libgssapi-krb5-2`, que fornece `libgssapi_krb5.so.2`, necessária ao carregamento do suporte GSSAPI no Linux. O teste `PostgreSqlMigrationTest` usa a factory sem conexão e detecta divergências entre modelo e snapshot; também roda na etapa de testes do Dockerfile, antes de produzir a imagem final. A proteção `PendingModelChangesWarning` permanece habilitada.

Comandos reproduzíveis na raiz (não repetir `migrations add` com os arquivos já presentes):

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations add Issue020 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build

dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

dotnet ef migrations script Issue017 Issue020 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build \
  --output /tmp/olivaw-issue020.sql

dotnet ef migrations has-pending-model-changes \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build

dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
```

Validação local: o teste de regressão falhou antes da migration e passou depois; 162 testes da API passaram; build Release e `has-pending-model-changes` sem diferenças. Migration, designer, snapshot e SQL entre `Issue017` e `Issue020` revisados. Ferramenta EF 10.0.12 emitiu somente aviso de patch inferior ao runtime 10.0.13. SQL Server e Oracle não receberam migrations. O daemon Docker local não está acessível ao usuário e o sudo exige autenticação; build/execução da imagem não foram validados nesta correção.

**Ao publicar uma nova imagem**, o startup aplicará automaticamente a `Issue020` antes de atender requisições. Reiniciar o container com a imagem antiga não inclui a correção. Nenhuma conexão a banco real, aplicação de SQL, publicação ou implantação foi feita nesta tarefa. A interface e os casos de uso da 04b foram concluídos posteriormente nesta branch, conforme a seção abaixo.


## Inventário de recursos e ambientes — 04b

Implementação e validação local concluídas. O [documento da 04b](../docs/features/done/04b-inventario-de-recursos.md) registra campos, regras, limites e evolução restante. Todas as rotas exigem `olivaw.root`, sem cache nem log de body:

- `GET/POST /resources` e `GET/PUT /resources/{externalId}`.
- `GET/POST /deployments` e `GET/PUT /deployments/{externalId}`.
- `POST /resource-customers`, `POST /resource-dependencies` e `POST /deployment-resources`.
- `PUT /resource-customers/{externalId}`, `PUT /resource-dependencies/{externalId}` e `PUT /deployment-resources/{externalId}` encerram períodos com `revision`, sem excluir histórico.
- `GET /inventory-links` pagina vínculos por uma âncora `resourceExternalId` ou `deploymentExternalId`, `kind=customer/dependency/deployment` e `includeEnded`.

Filtros de recursos: `page`, `pageSize`, `search`, `type`, `provider`, `status`, `environment`, `customerExternalId`, `shared`. Implantações: `page`, `pageSize`, `serviceExternalId`, `environment`. Filtros textuais opcionais de serviços/recursos/implantações aceitam ausência/vazio. Payloads planos ficam em `Owh.Olivaw.Client/Features/InventoryFeature`; autoria vem da identidade autenticada, fora do body público.

Vínculos novos recebem `resourceExternalId`, `targetExternalId`, `purpose`; relações acrescentam `kind`, e usos podem declarar `reservedCpu`, `reservedMemoryGiB`, `reservedStorageGiB`. Alterar finalidade/reserva requer encerrar e criar novo período. PUT exige revisão positiva. Desativação, composição e encerramentos são protegidos por revisões; conflitos retornam HTTP 409. Encerrar serviço encerra implantações e usos na mesma transação, preservando recursos compartilhados.

Consultas comuns usam o repositório genérico. `IRepositoryInventoryResource` acrescenta projeções/paginação e alcance deduplicado; `IRepositoryServiceDeployment` pagina implantações com serviço; `IRepositoryInventoryRevision` coordena a versão da topologia e encerramentos. A revisão é carregada antes das leituras que sustentam decisões; as alterações são salvas na mesma unidade de persistência EF.

Comandos na raiz:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Api -c Release --no-restore -m:1
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
dotnet pack ms-olivaw-api/src/Owh.Olivaw.Client -c Release --no-build --no-restore --output /tmp/olivaw-inventory-pack

dotnet ef migrations has-pending-model-changes \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --no-build
```

Resultado: build Release sem warnings, 239 testes aprovados, pack local e modelo PostgreSQL sem diferenças. Autorização HTTP, domínio, CQS, conflitos, filtros vazios, snapshots, períodos e SQL/modelo PostgreSQL e SQL Server foram testados sem conexão. A conclusão **não alterou o schema nem gerou outra migration**: permanece somente a `Issue020`. Oracle não foi contemplado. Não houve conexão a banco real, publicação ou redeploy; os novos fluxos precisam ser publicados e conferidos no ambiente.

## Detalhes de recursos — 04c

O inventário recebeu tecnologia/versão, identificação lógica e campos específicos de renovação de domínio, validade de certificado e intervalo/retenção de backup. Os dados são declarados, opcionais e preservados no histórico; a API valida a compatibilidade com o tipo de recurso. Datas são informadas em UTC à meia-noite.

**Registro anterior à geração de 24/10/2026:** o bloqueio abaixo foi resolvido pela `Issue020Panel`, descrita no fechamento do pacote ao final. Os mappings de `inventoryresource` e `inventoryresourcechange` incluem novas colunas, mas nenhuma migration foi gerada ou aplicada. O teste de correspondência do modelo PostgreSQL permanece como bloqueio; o startup rejeita modelo pendente. A geração depende de pedido explícito e deve preservar a `Issue020` publicada.

Comandos reproduzíveis de build, testes e verificações de interface, resultados e limites dos provedores estão na [feature 04c](../docs/features/done/04c-detalhes-de-recursos.md). PostgreSQL e SQL Server são verificados pelo modelo em memória, sem acessar bancos reais; migrations de ambos permanecem fora desta entrega.

## Operação interna — 05 a 10

Novos contratos internos para saúde pontual, incidentes, consumo informado, estudos de planos, evidências de cuidados e atendimento. Todos os endpoints exigem `olivaw.root`, preservam auditoria e restringem HTTP logging a método/caminho/status. Preços e ofertas não são presumidos; estudos de planos não alteram contratos.

**Registro anterior à geração de 24/10/2026:** migrations PostgreSQL concluídas no fechamento abaixo. Novas tabelas: `healthreading`, `usagereading`, `careevidence`, `incident`, `incidentchange`, `supportrequest`, `supportrequestchange`, `costreference`, `planstudy`, `planstudycost` e `customerplan`, além das colunas documentadas da 04c. O teste de correspondência PostgreSQL permanece ativo e falha até gerar migration mediante solicitação explícita. Nenhum provedor recebeu migration neste lote.

A coleta HTTP pontual utiliza `Monitoring:AllowedUrls`, uma lista de URLs HTTPS exatas previamente autorizadas, e a referência técnica do recurso. Sem configuração, permanece bloqueada. Não habilite alvos apenas para testar: as verificações locais usam transporte simulado. Não há scheduler ativo nem leitura AWS/DevOps.

Escopos, limites, comandos reproduzíveis e resultados no [guia da operação interna](../docs/operacao-interna.md).


## Pacote do painel e portal — Issue020Panel

Em 24/10/2026, mediante autorização do mantenedor, foram geradas no provedor PostgreSQL:

- `20261024142713_Issue020Panel`: 14 colunas adicionais no inventário/histórico e 18 tabelas novas de saúde, consumo, incidentes, custos/estudos, condições por cliente, cuidados, atendimento, acesso do portal e entregas, com seus índices e FKs.
- `20261024142722_Issue020PortalIdentity`: cinco tabelas prefixadas `portal` da identidade compartilhada, no contexto independente `PortalIdentityDbContext`, sem usuários ou root pré-cadastrados. Histórico próprio `portalidentitymigrations`.

A `Issue020` existente e todas as migrations publicadas foram preservadas. Migration, designer, snapshots e SQL entre `Issue020` e `Issue020Panel`, e de zero até `Issue020PortalIdentity`, foram revisados: avanço somente aditivo, sem `DROP`, renomeação, alteração de tipo ou remoção de registros existentes. Campos textuais novos do inventário têm default vazio e datas/números opcionais permanecem nulos. Nenhum SQL foi aplicado nem banco real acessado.

### Comandos reproduzíveis

Executar a partir da raiz. Os arquivos já existem: **não repetir `migrations add`**. As factories usam conexões fictícias e não iniciam o host.

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1

# Geração realizada nesta entrega:
dotnet ef migrations add Issue020Panel --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
dotnet ef migrations add Issue020PortalIdentity --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context PortalIdentityDbContext --output-dir Migrations/PortalIdentity --configuration Release --no-build

# Após gerar, recompilar antes de validar/scriptar:
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1
dotnet ef migrations script Issue020 Issue020Panel --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build --output /tmp/olivaw-issue020-panel.sql
dotnet ef migrations script 0 Issue020PortalIdentity --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context PortalIdentityDbContext --configuration Release --no-build --output /tmp/olivaw-issue020-portal.sql
dotnet ef migrations has-pending-model-changes --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
dotnet ef migrations has-pending-model-changes --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context PortalIdentityDbContext --configuration Release --no-build
dotnet build ms-olivaw-api/src/Owh.Olivaw.Api -c Release --no-restore -m:1
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
```

Validação local: build Release sem erros ou avisos; 359 testes da API passaram; ambos os contextos sem diferenças de modelo. Frontend: build e lint concluídos, 33 testes unitários e 31 E2E do pacote aprovados (29 administrativos e dois do portal). O build Angular mantém avisos preexistentes de orçamento de estilos. EF Tools 10.0.12 emite aviso de patch anterior ao runtime 10.0.13; dependências não foram atualizadas. SQL Server tem modelo/consultas verificados sem conexão, mas **não recebeu migrations**; Oracle não foi contemplado.

### Publicação da base

A base PostgreSQL está pronta para gerar/publicar uma nova imagem a partir deste checkout validado. **O startup aplicará `Issue020Panel` e `Issue020PortalIdentity` automaticamente antes de atender requisições**, sem `database update` manual. A criação de tabelas de identidade do portal ocorre mesmo com login externo desabilitado. Reiniciar uma imagem antiga não inclui o pacote. Execução real do SQL, imagem Docker e smoke test do ambiente ainda não foram verificados nesta entrega. Não houve finish, push ou implantação.

Para habilitar login do cliente, configure `PortalIdentity:Issuer`, `PortalIdentity:Audience`, `PortalIdentity:SigningKey` (segredo próprio com ao menos 32 bytes) e `PortalIdentity:TokenMinutes` (1 a 120). Emissor, audiência e chave devem diferir da administração interna. O portal usa `ConnectionStrings:PortalIdentity` quando informado, ou `ConnectionStrings:DbContext`; o schema deve estar disponível para os dois contextos. Sem configuração válida de identidade, o login retorna 503; os módulos administrativos continuam disponíveis. Convites são entregues manualmente pela operação; não há envio automático de e-mail.

O pacote não inicia outro serviço permanente, agendador ou coletor na VM. Sem `Monitoring:AllowedUrls`, a verificação HTTP permanece bloqueada. Financeiro, IA, automações e aceite comercial formal continuam no planejamento; veja o [índice das entregas](../docs/features/index.md) e o [guia de operação](../docs/operacao-interna.md). Consulte também os [comandos de migrations do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores).

## Relatórios de segurança do Baley

- `POST /security-reports`: conta autenticada com `security.reports.write`, corpo `{ "rawData": "<JSON do relatório>" }`. Retorna 201 na primeira entrega e 200 em reenvio idêntico do mesmo autor. Limite HTTP de 1 MiB e RawData de 262.144 caracteres; envelope JSON até profundidade 32. Limite de 12 escritas por hora por identidade. Corpo e autenticação ficam fora do HTTP logging.
- `GET /security-reports?beforeId=0&hostId=<opcional>`: somente root, blocos de 20 com `nextBeforeId`, mais recentes primeiro.
- `GET /security-reports/{externalId}`: somente root, resposta `{ "item": ... }`, 404 para ID ausente.

`SecurityReport : Entity` fica no domínio. Tabela `securityreport`, GUID público, autor autenticado e datas herdados; colunas estáveis `RunId`, `AgentId`, `HostId`, `SchemaVersion`, `StartedAt`, `CompletedAt`, `ExecutionStatus` e `RawData` string. Índice único por agente/RunId impede duplicação; repetição com outro autor/conteúdo é rejeitada. Campos adicionais do objeto `result` são preservados sem mudar o schema relacional. Não há rota de escrita anônima, exclusão automática ou acesso do agente ao banco.

O envelope 1.0 exige `runId` GUID não vazio, `agentId=baley`, `hostId`, timestamps UTC ordenados, `executionStatus` completed/partial/failed e objeto `result`. Metadados são extraídos do JSON pelo servidor; o cliente não escolhe o autor. A conta `baley-agent` não é root e tem somente `security.reports.write`. Consulte a [operação do Baley](../olivaw-agent/ms-baley-py/README.md) e a [feature concluída localmente](../docs/features/done/agentes-python-baley-e-relatorios.md).

Somente PostgreSQL está autorizado nesta entrega. A migration `20261025121719_Issue025` cria somente `securityreport` e seus índices, preservando tabelas e dados existentes. No deploy, o startup aplica migrations pendentes automaticamente; geração e revisão locais não aplicam alterações em bancos reais. SQL Server fica para uma entrega futura.

Comandos reproduzíveis da issue025 (migration já gerada; não repetir `add`):

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1
dotnet ef migrations add Issue025 --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
dotnet ef migrations script Issue020Panel Issue025 --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build --output /tmp/olivaw-issue025.sql
dotnet ef migrations has-pending-model-changes --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
```

Não executamos `database update`; o deploy do mantenedor aplicará a migration pelo startup.

## Correções aprovadas do Baley

Fluxo implementado com a migration PostgreSQL `20261025220842_Issue025Remediation`. `Issue025` publicada permanece inalterada. A feature de [correção aprovada](../docs/features/done/baley-correcao-aprovada-e-saude.md) registra checks e pendência de publicação.

- `GET /security-remediations?reportExternalId=<guid>`: root consulta estado e plano de cada verificação, sem revelar reserva de execução.
- `POST /security-remediations`: root autoriza `{ "reportExternalId": "...", "findingIndex": 0 }`; índice corresponde ao array original e imutável. Relatório antigo ou procedimento não disponível exige intervenção.
- `POST /security-remediations/batch`: root envia `{ "reportExternalId": "..." }`; autoriza pendentes do relatório inteiro e preserva registros existentes. Salva a coleção em uma unidade de persistência; restrição única impede duplicações concorrentes. Retorna contagens autorizadas, encaminhadas e já tratadas.
- `GET /security-remediations/queue?hostId=<id>`: `security.remediations.execute`, somente tarefas do próprio autor técnico e host solicitado, aprovadas e não expiradas.
- `PUT /security-remediations/{externalId}`: mesma permissão, identidade do executor, reserva e estado corretos. Ações fixas claim/validating/resolved/intervention, nunca comandos. Reserva de dez minutos e autorização de 24 horas. Resolvido exige fase de validação e evidências de saúde antes/depois, duas amostras e achado ausente. Reserva expirada não permite sucesso; intervenção ainda pode registrar a execução interrompida.

`SecurityRemediation : Entity` referencia o relatório sem alterar RawData, registra aprovador, executor, índice do achado, host, plano/resultado JSON, mensagem, prazo e revisão. Índice exclusivo da chave de host ativa impede correções simultâneas por processos diferentes. Execução interrompida pode exigir revisão manual antes de liberar esse host; nunca liberar e repetir a mutação automaticamente.

## Categorias de serviços

O campo continua `type` no contrato para preservar os consumidores; o rótulo administrativo é **Categoria**. Novos valores aceitos: unknown, dotnet, angular, postgres, rabbitmq e redis. Categoria e situação são obrigatórias no cadastro; “Todos” existe apenas nos filtros. A UI inicia categoria unknown. Valores legados não confirmam uma tecnologia e são apresentados/filtrados como unknown, sem reescrever históricos existentes. Situações mantêm códigos planned/onboarding/active/closed, exibidos como Em análise/Em implantação/Em operação/Encerrado.

- `PUT /security-remediations/{externalId}/review`: root registra revisão de uma execução incerta após o prazo expirar, com `confirmedStopped=true` e nota de pelo menos 20 caracteres. Libera somente a reserva do host; o achado continua em intervenção. Não executar enquanto o processo remoto ainda puder estar em andamento.

Migration já gerada, **não repetir add**. Comandos reproduzíveis, sem iniciar o host ou conectar a banco real:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql -c Release --no-restore -m:1
dotnet ef migrations add Issue025Remediation --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
dotnet ef migrations script Issue025 Issue025Remediation --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build --output /tmp/olivaw-issue025-remediation.sql
dotnet ef migrations has-pending-model-changes --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --context WriteDbContext --configuration Release --no-build
```

O startup aplicará a nova tabela durante o deploy; não foi executado database update ou redeploy nesta entrega. O modelo do PortalIdentity foi conferido sem alterações pendentes. Somente PostgreSQL contemplado.
