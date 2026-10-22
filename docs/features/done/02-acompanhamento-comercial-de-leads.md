# 02b — Acompanhamento comercial de leads

## Estado e objetivo

Desenvolvimento concluído e validado localmente em 22/10/2026 na branch `feature/issue017-followup-comercial`. Evolução da [consulta administrativa](02-leads-administrativos.md), vinculada à [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md).

A equipe acompanha contatos e etapas do projeto sem modificar o snapshot original enviado pelo visitante. Publicação e validação no PostgreSQL do ambiente continuam pendentes.

## Entrega implementada

- Etapas Novo contato, Em conversa, Em análise, Em desenvolvimento, Em validação, Entregue e Sem continuidade, com transições livres entre todas elas.
- Sem responsável atribuído; root ou operador com a permissão interna `leads.manage` consulta e movimenta o acompanhamento.
- Notas internas de até 3.000 caracteres, preservadas no histórico com autor e data. Não há edição ou exclusão de notas pela API/interface.
- Histórico paginado em blocos de 20 alterações, com etapa anterior/nova, nota e identificação do operador. Autoria e data são obtidas no servidor.
- Filtro por etapa na lista e contagens gerais por etapa persistida. Total recebido e recebido nos últimos sete dias continuam sendo indicadores de recebimento, não de pendências.
- Revisão própria e resposta HTTP 409 para conflitos; o formulário conserva o texto quando o salvamento falha.
- Lista compacta e painel de detalhes maior no desktop; uma coluna com detalhes antes da lista no celular.

`Stage`, `Revision`, `Snapshot` e data do envio público não são alterados por movimentações internas. Reenvios idempotentes e novas revisões públicas preservam o acompanhamento. Registro da alteração e atualização do lead são salvos na mesma unidade transacional.

## Acesso e retenção

A autorização usa o `PermissionHandler` existente da identidade interna: root tem acesso global e os demais precisam de `leads.manage` vigente. Uma permissão revogada não continua autorizando pelo token antigo. Usuários externos sem essa permissão não consultam leads ou notas. Clientes e Visão geral continuam exclusivos de root nesta entrega; operador delegado entra diretamente em Leads.

Não foram criados access profiles, usuários de empresa ou vínculos de portal. A hierarquia proposta permanece no planejamento da [feature 11](../painel-olivaw/11-identidade-do-portal.md). Notas internas não são notas de versão; a linha do tempo pública está na [feature 18](../painel-olivaw/18-historico-de-entregas.md).

O histórico é preservado sem expurgo automático. Sem continuidade substitui a proposta anterior de arquivamento; é possível voltar de qualquer etapa, inclusive após entrega. Nenhuma mudança dispara publicação, criação de cliente, contratação ou cobrança.

## Correção de sessão incluída

A sessão administrativa passa a guardar somente token e expiração no `sessionStorage`, com chave por URL de API. O bootstrap consulta o acesso atual na API antes de restaurar a sessão. Recarregar com cache desabilitado preserva o login enquanto o token for válido; logout, expiração, 401 autenticado ou falha na revalidação removem a sessão. Sem refresh token ou renovação automática. Prerender não acessa o armazenamento do navegador.

## Migration e publicação pendente

Migration única PostgreSQL `20261022232507_Issue017`, gerada somente ao final após estabilizar o modelo e validar a feature, conforme solicitado pelo mantenedor. Acrescenta os campos de acompanhamento em `lead` e a tabela `leadchange`, com índices e FK interna restritiva. Leads existentes começam em Novo contato e revisão zero, sem histórico inventado.

Migration, designer, snapshot e SQL entre `Issue013Customers` e `Issue017` foram revisados. Não alteram tabelas de identidade/clientes nem migrations anteriores. Modelo sincronizado segundo `has-pending-model-changes`. SQL Server teve mapeamento/tradução verificados, sem migration solicitada; Oracle não contemplado.

**O startup da API aplica migrations pendentes após publicar.** Não houve aplicação em banco, conexão a dados reais, push, finish ou implantação nesta entrega. Execução real no PostgreSQL e smoke test após publicação permanecem com o mantenedor. Atualizar somente o frontend não disponibiliza os endpoints novos.

## Validações

- Build completo de `Olivaw.sln` em Release, sem erros ou warnings.
- Suíte da API: 125 testes, cobrindo domínio, CQS, HTTP em memória, concorrência, permissões e revogação, filtros, paginação e preservação do envio público.
- Frontend: 28 testes unitários; lint e build concluídos. O build mantém os três avisos preexistentes de orçamento de estilos em páginas públicas.
- 18 testes de navegador administrativos passaram, incluindo desktop/celular, salvamento, conflito, acesso delegado, logout, expiração e reload com cache desabilitado. Após ajustar a proporção das colunas, os oito testes de leads passaram novamente e as capturas foram conferidas.
- SQL e modelo verificados sem conexão a banco real; pacote cliente gerado localmente, sem publicação.

Contratos e comandos reproduzíveis no [README da API](../../../ms-olivaw-api/README.md#acompanhamento-comercial-de-leads) e no [README do frontend](../../../ms-olivaw-ng/README.md#acompanhamento-de-leads).

## Fora desta entrega

- Campanhas, mensagens automáticas, exportações e CRM completo.
- Login de empresa, solicitações de alteração, linha do tempo pública de entregas e gestão de recursos/planos, que permanecem nas features próprias do portal.
- Criação automática de clientes, contas ou recursos. A conversão manual pertence ao [cadastro de clientes](03-cadastro-de-clientes.md).
