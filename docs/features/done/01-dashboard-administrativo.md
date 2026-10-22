# 01 — Dashboard administrativo da Olivaw

## Estado e objetivo

Implementação inicial disponível no código para avaliação visual pelo mantenedor. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Transformar a tela básica de `/admin` em um ponto de entrada claro para a operação. Esta é a primeira feature a implementar e deve funcionar sem depender da construção dos módulos seguintes.

## Dependências

Login, sessão e saída já existentes no frontend Olivaw. Confirmar o acesso administrativo permitido pela identidade interna. Não depende do cadastro de clientes, de telemetria ou do portal.

## Escopo da entrega

- Criar o layout administrativo com navegação lateral no desktop e navegação adequada ao celular, cabeçalho, conteúdo e identificação do operador.
- Manter a entrada em `/admin`, com título **Olivaw - Operação**, saudação e indicação clara de que esta é a administração interna.
- Compor uma visão geral enxuta: contexto do ambiente quando conhecido, orientação inicial e atalhos somente para funcionalidades realmente disponíveis.
- Mostrar estados vazios específicos: clientes ainda não cadastrados, monitoramento ainda não conectado e solicitações ainda não habilitadas. Esses estados não são contadores nem medições.
- Preparar regiões para prioridades e últimas mudanças sem preencher com eventos, gráficos ou números inventados. Acrescentar os dados quando as features responsáveis forem entregues.
- Usar superfícies claras, azul profundo e verde suave, tipografia forte e um mini Olivaw discreto no acolhimento ou estado vazio. Animações leves e movimento reduzido.
- Preservar o fluxo de logout, o tratamento de sessão expirada e os estados de erro da autenticação. Confirmar autorização interna: estar autenticado não basta se o papel não permitir operar.
- No máximo quatro indicadores quando houver fontes reais. Todo indicador deve abrir a lista que o explica e identificar fonte, período e atualização.

## Fora desta entrega

- CRUD de clientes, leitura administrativa de leads, inventário, coleta, incidentes, consumo, planos e agentes.
- Novo serviço ou tabela apenas para alimentar um dashboard sem fontes.
- Menu cheio de páginas vazias ou botões que aparentem executar funcionalidades futuras.
- Criação de usuários, gestão completa de perfis, provisionamento e operações em produção.

## Critérios de aceite

- Operador autorizado entra em `/admin`, reconhece a operação Olivaw e consegue sair. Visitante sem sessão e usuário sem autorização recebem o tratamento previsto.
- A página é útil e coerente mesmo sem nenhum cliente cadastrado ou fonte conectada; ausência de dados nunca aparece como zero confirmado ou estado saudável.
- Não há requisições para endpoints inexistentes nem dados demonstrativos misturados à operação real.
- Desktop, celular, navegação por teclado, foco visível e movimento reduzido permitem usar o layout sem sobreposição ou rolagem horizontal.
- Os fluxos existentes de login e logout continuam funcionando. Validar rotas, autorização e apresentação nos testes existentes, sem criar testes de detalhes puramente decorativos.

## Decisões adotadas na primeira versão

- Acesso inicial exclusivo do root da identidade interna, obtido no login existente. Conta autenticada sem root é direcionada a acesso negado. A delegação terá uma permissão própria antes da entrada de outros perfis; nenhuma consulta operacional protegida foi criada nesta feature.
- Ambiente exibido como “Ambiente não informado”. Não há configuração pública confirmada e não inferimos ambiente pela URL da API.
- Menu contém somente Visão geral. Acesso ao site público, identificação do operador e saída completam a navegação. Nenhuma página futura foi simulada.
- Dados de clientes, saúde, atendimento e histórico permanecem sem integração; são estados iniciais, não medições ou contagens.

## Implementação e validação

A rota `/admin` recebeu layout, componentes de apresentação e tratamento de acesso root, preservando o contrato de login e logout. O dashboard tem navegação por teclado, atalho de foco para o conteúdo, layout para desktop/celular e animação de entrada dispensada em movimento reduzido.

Referências: [dashboard](../../../ms-olivaw-ng/src/app/features/admin/admin-page.ts), [layout](../../../ms-olivaw-ng/src/app/features/admin/admin-layout.ts), [guard](../../../ms-olivaw-ng/src/app/core/auth/auth.guard.ts) e [testes de navegador](../../../ms-olivaw-ng/e2e/admin-dashboard.spec.ts).

Os testes simulam a identidade e cobrem root, conta sem root, estados iniciais, ausência de consultas operacionais, imagem carregada, saída, falha de revogação, expiração e apresentação em desktop/celular. Comandos reproduzíveis no [README do frontend](../../../ms-olivaw-ng/README.md#dashboard-interno-da-olivaw).

Não houve mudança de API, schema, migration, configuração de infraestrutura ou publicação. Este registro descreve o recorte inicial. Consulta de leads e cadastro de clientes foram entregues depois e têm documentos próprios nesta pasta.

Validação local da primeira versão: build de produção, lint, formatação dos arquivos alterados, um teste do estado de autenticação e oito testes de navegador aprovados. Permanecem avisos de tamanho já existentes nos estilos públicos do Estúdio e da jornada. Três regras redundantes de espaçamento do resumo da jornada foram removidas para corrigir o limite de erro que já bloqueava o build; o dashboard não aumenta esses limites.

## Evolução: visão geral e dashboard analítico

A Visão geral permanece focada em prioridades, contexto e acontecimentos. Uma aba Dashboard será o espaço de análise com gráficos, filtros por período e aprofundamento, após entregar as fontes de dados. Não é uma segunda página vazia nesta primeira versão.

Os primeiros gráficos previstos são disponibilidade e latência com cobertura explícita na [feature 05](../painel-olivaw/05-coleta-e-saude.md), e crescimento de armazenamento/uso por período na [feature 07](../painel-olivaw/07-consumo-e-capacidade.md). A navegação analítica entra com a primeira fonte real; sem histórico suficiente, apresenta ausência de dados, sem curvas demonstrativas na operação. Esta separação não altera o escopo da primeira entrega.
