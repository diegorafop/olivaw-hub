# 02 — Consulta e acompanhamento de leads

## Estado e objetivo

Recorte de consulta implementado no código para avaliação. Acompanhamento comercial ainda planejado. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Dar à equipe acesso aos JSONs recebidos pela jornada e pelo contato direto. É a primeira fonte comercial útil para o dashboard, sem transformar automaticamente um interessado em cliente.

## Dependências

[Dashboard administrativo](01-dashboard-administrativo.md) e persistência de leads disponível no ambiente. O envio público já existe; consultas administrativas protegidas e acompanhamento interno ainda precisam ser implementados.

## Escopo da entrega

- Lista paginada de leads, com busca e filtros úteis por origem, data e situação comercial.
- Ficha legível com negócio, ideia, escolhas, plano de interesse, e-mail ou WhatsApp e histórico da jornada. Aceitar snapshots antigos sem WhatsApp.
- Definir acompanhamento comercial mínimo: novo, em conversa e arquivado, com responsável e notas internas; confirmar transições e persistência antes de codar.
- Criar consultas administrativas autorizadas, preservando o envio público sem oferecer leitura pública do lead ou revelar sua chave de escrita.
- Conectar ao dashboard o número de leads que precisam de atenção, com navegação para a lista correspondente.

## Fora desta entrega

- Envio automático de WhatsApp ou e-mail, campanhas e CRM completo.
- Conversão automática em cliente ou criação de recursos.
- Exposição de contatos em logs, métricas gerais ou notificações sem necessidade.

## Critérios de aceite

- Operador com permissão localiza e lê o lead; pessoa sem permissão não consulta, exporta ou altera seu acompanhamento.
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

Não houve alteração de entidade persistida, schema, migration ou dados existentes. O novo código requer publicação da API; atualizar apenas o frontend não disponibiliza as consultas. Nenhum deploy ou acesso a banco real foi feito durante a implementação.

## Acompanhamento comercial pendente

Status novo/em conversa/arquivado, responsável e notas internas continuam fora deste recorte de consulta. Precisam de um modelo separado do snapshot enviado pelo visitante, com auditoria, concorrência, permissões e retenção definidos. Não usar `Stage` da jornada como status comercial nem alterar o snapshot para registrar notas: isso confundiria o contrato público e a idempotência.

Não há botão de acompanhamento fictício, filtro comercial sem persistência, exportação, notificação automática ou conversão em cliente. Ao implementar a persistência comercial, gerar migrations somente após solicitação explícita do mantenedor.

## Validação deste recorte

- Compilação da API e do frontend, lint e formatação dos arquivos modificados.
- Testes CQS de listagem, detalhes, resumo, filtros, limites e compatibilidade de contatos.
- HTTP em memória verificando 401 sem sessão, 403 sem root, 200 com root, 404 para lead ausente e ausência de campos internos nas respostas.
- Tradução SQL de filtros e paginação para PostgreSQL e SQL Server, sem conexão real. Isso não substitui uma verificação de execução no banco do ambiente após a publicação.
- Testes de navegador com fontes simuladas para lista, detalhes, origem, período, paginação, falhas, contatos e layout desktop/celular.

Comandos e contratos no [README da API](../../../ms-olivaw-api/README.md#consulta-administrativa-de-leads) e no [README do frontend](../../../ms-olivaw-ng/README.md#leads-administrativos).
