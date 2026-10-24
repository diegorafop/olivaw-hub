# 02 — Consulta administrativa de leads

## user-history

Como integrante da equipe comercial,
quero consultar os contatos recebidos,
para entender o interesse de cada pessoa e dar continuidade ao atendimento.

## acceptance-criteria

- Operador root localiza e lê o lead; pessoa sem permissão não acessa consultas administrativas.
- Contato por e-mail, só WhatsApp e snapshots antigos são apresentados corretamente.
- Envio idempotente continua funcionando e registros não são duplicados por uma atualização comercial.
- Paginação, ausência de resultados, falha de leitura e dados ainda não carregados têm estados distintos.

## ai-prompt

Concluída no desenvolvimento; preserve pendências de publicação e ambiente.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Recorte de consulta concluído. O [acompanhamento comercial](02-acompanhamento-comercial-de-leads.md) foi implementado posteriormente em recorte próprio. Parte da [épica de administração e portal](../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Dar à equipe acesso aos JSONs recebidos pela jornada e pelo contato direto. É a primeira fonte comercial útil para o dashboard, sem transformar automaticamente um interessado em cliente.

### Dependências

[Dashboard administrativo](01-dashboard-administrativo.md) e persistência de leads disponível no ambiente. Envio público e consultas administrativas protegidas estão implementados. A evolução comercial é independente deste recorte.

### Escopo da entrega

- Lista paginada de leads, com busca e filtros úteis por origem e data.
- Ficha legível com negócio, ideia, escolhas, plano de interesse, e-mail ou WhatsApp e histórico da jornada. Aceitar snapshots antigos sem WhatsApp.
- Criar consultas administrativas autorizadas, preservando o envio público sem oferecer leitura pública do lead ou revelar sua chave de escrita.
- Conectar ao dashboard o total recebido e a quantidade recebida nos últimos sete dias, com navegação para a lista. Esses números não representam pendências comerciais.

### Fora desta entrega

- Envio automático de WhatsApp ou e-mail, campanhas e CRM completo.
- Conversão automática em cliente ou criação de recursos.
- Exposição de contatos em logs, métricas gerais ou notificações sem necessidade.

### Decisões antes de implementar

- Permissões de leitura e acompanhamento; estados comerciais e responsável.
- Retenção, arquivamento, necessidade de exportação e separação entre snapshot recebido e acompanhamento interno.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 02 — Consulta administrativa de leads

## Estado e objetivo

Recorte de consulta concluído. O [acompanhamento comercial](02-acompanhamento-comercial-de-leads.md) foi implementado posteriormente em recorte próprio. Parte da [épica de administração e portal](../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Dar à equipe acesso aos JSONs recebidos pela jornada e pelo contato direto. É a primeira fonte comercial útil para o dashboard, sem transformar automaticamente um interessado em cliente.

## Dependências

[Dashboard administrativo](01-dashboard-administrativo.md) e persistência de leads disponível no ambiente. Envio público e consultas administrativas protegidas estão implementados. A evolução comercial é independente deste recorte.

## Escopo da entrega

- Lista paginada de leads, com busca e filtros úteis por origem e data.
- Ficha legível com negócio, ideia, escolhas, plano de interesse, e-mail ou WhatsApp e histórico da jornada. Aceitar snapshots antigos sem WhatsApp.
- Criar consultas administrativas autorizadas, preservando o envio público sem oferecer leitura pública do lead ou revelar sua chave de escrita.
- Conectar ao dashboard o total recebido e a quantidade recebida nos últimos sete dias, com navegação para a lista. Esses números não representam pendências comerciais.

## Fora desta entrega

- Envio automático de WhatsApp ou e-mail, campanhas e CRM completo.
- Conversão automática em cliente ou criação de recursos.
- Exposição de contatos em logs, métricas gerais ou notificações sem necessidade.

## Critérios de aceite

- Operador root localiza e lê o lead; pessoa sem permissão não acessa consultas administrativas.
- Contato por e-mail, só WhatsApp e snapshots antigos são apresentados corretamente.
- Envio idempotente continua funcionando e registros não são duplicados por uma atualização comercial.
- Paginação, ausência de resultados, falha de leitura e dados ainda não carregados têm estados distintos.

## Decisões antes de implementar

- Permissões de leitura e acompanhamento; estados comerciais e responsável.
- Retenção, arquivamento, necessidade de exportação e separação entre snapshot recebido e acompanhamento interno.

## Recorte implementado

A página `/admin/leads` oferece lista paginada, busca por negócio/título/e-mail/WhatsApp, filtros de origem e período, atualização manual e ficha em `/admin/leads/:id`. A ficha traduz o snapshot para contato, ideia, escolhas incluídas/excluídas, interesse em plano e eventos. No celular, os detalhes aparecem antes da lista. A identidade do operador e o menu recolhível permanecem compartilhados entre as páginas.

O dashboard consulta contagens reais de leads recebidos e recebidos nos últimos sete dias. Elas não representam leads não lidos nem pendências comerciais, pois esses estados ainda não existem. Erro e ausência de resultados são distintos, sem contagens simuladas.

A API oferece `GET /leads`, `GET /leads/{externalId}` e `GET /leads/summary`, somente para root autenticado da identidade interna. O envio público `PUT` continua separado. Respostas não incluem chave/hash de escrita, PK interna ou auditoria interna da entidade; usam `Cache-Control: no-store`.

A paginação e os filtros são executados no banco, extraindo campos do JSON. A consulta usa `IRepositoryLead` / `RepositoryLead`, herdando `IRepository<Lead>` / `Repository<Lead>`, porque o contrato genérico não fornece paginação/contagem nem projeção JSON. Leitura por ID e filtros comuns continuam usando `IRepository<Lead>` diretamente no handler. PostgreSQL e SQL Server possuem projeções próprias; os valores dos filtros são parametrizados pelo EF Core. A busca cobre negócio, título, e-mail e WhatsApp, não o texto completo dos requisitos.

Não houve alteração de entidade persistida, schema, migration ou dados existentes. O novo código requer publicação da API; atualizar apenas o frontend não disponibiliza as consultas. Na implementação e nos testes locais não houve acesso a banco real. Posteriormente, a consulta foi publicada em 21/10/2026 e `GET /leads` autenticado respondeu HTTP 200, conforme o [registro operacional](../../deploy-executor.md#compose-falha-e-o-disco-está-cheio).

## Evolução separada: acompanhamento comercial

Etapas livres, notas internas, histórico, concorrência e autorização delegada foram entregues posteriormente na [feature 02b](02-acompanhamento-comercial-de-leads.md). A implementação preserva o snapshot e a revisão pública. Não há responsável atribuído. Os contratos e permissões descritos acima registram a entrega original de consulta; a feature 02b amplia o acesso para operadores com `leads.manage` e documenta a migration/publicação pendente.

A conversão explícita de lead foi entregue no [cadastro de clientes](03-cadastro-de-clientes.md). Exportações e notificações automáticas continuam fora dessas entregas.

## Validação deste recorte

- Compilação da API e do frontend, lint e formatação dos arquivos modificados.
- Testes CQS de listagem, detalhes, resumo, filtros, limites e compatibilidade de contatos.
- HTTP em memória verificando 401 sem sessão, 403 sem root, 200 com root, 404 para lead ausente e ausência de campos internos nas respostas.
- Tradução SQL de filtros e paginação para PostgreSQL e SQL Server, sem conexão real. Isso não substitui uma verificação de execução no banco do ambiente após a publicação.
- Testes de navegador com fontes simuladas para lista, detalhes, origem, período, paginação, falhas, contatos e layout desktop/celular.

Comandos e contratos no [README da API](../../../ms-olivaw-api/README.md#consulta-administrativa-de-leads) e no [README do frontend](../../../ms-olivaw-ng/README.md#leads-administrativos).
```
