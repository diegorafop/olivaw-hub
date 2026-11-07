# Dashboards gerenciais de Trevize e Baley

## user-history

Como operador da Olivaw,
quero uma visão gerencial por agente antes das listas,
para acompanhar atividade, resultados e prioridades sem confundir captação com o trabalho comercial.

## acceptance-criteria

- Trevize possui `/admin/agents/trevize`, independente da tela de Leads, com breadcrumb e links para fichas.
- Baley mantém `/admin/agents/baley`, com indicadores e gráficos antes das auditorias.
- Ambas as rotas exigem root; os endpoints existentes preservam suas autorizações.
- Filtros de 7, 30 e 90 dias, estados de carregamento, erro, vazio e paginação.
- Indicadores e gráficos usam registros reais carregados da API, com tamanho da amostra explícito.
- A captação apresenta entradas diárias, etapas atuais, novos contatos, avanço comercial e projetos entregues.
- Segurança apresenta auditorias, hosts, coletas incompletas, relatórios com risco elevado e distribuição das ocorrências por gravidade.
- Datas inválidas, falhas e conteúdo incompatível não indicam segurança ou atividade fictícia.
- Interface responsiva, gráficos com rótulos acessíveis e navegação sem modais.

## ai-prompt

Implementação em `ms-olivaw-ng/src/app/features/admin/agents`, compartilhando gráficos e funções puras de métricas. O card de Trevize abre o dashboard próprio. Links levam à ficha em `/admin/leads/:id/details` e à operação de Leads filtrada por prospecção.

A visão de captação usa `GET /leads` com `entry=prospecting`. Esse canal também aceita cadastro manual: **não há atribuição exclusiva ao Trevize**, e o dashboard declara esse limite. Avanço comercial significa etapa atual em conversa, análise, desenvolvimento, validação ou entregue; cadastro novo e desistência não contam. Isso não representa qualificação automática, venda fechada ou conversão histórica. Projetos entregues não significam receita comprovada.

Os gráficos usam somente os registros carregados, filtrados pela data de criação do Lead ou conclusão da auditoria. A paginação amplia a amostra, sem carregar todo o histórico automaticamente. No Baley, os achados são ocorrências por relatório: problemas repetidos não são deduplicados nem considerados corrigidos por desaparecerem. Não inferir disponibilidade do processo local a partir do último relatório.

Não houve alteração de API, schema, migrations ou execução dos agentes. **Evolução ainda necessária:** telemetria de ciclos do Trevize na API, identificação explícita do agente em cada captação, agregação no servidor e histórico de mudanças para taxas reais de qualificação e conversão. Essas métricas não são simuladas nesta entrega.

Validação local: build de produção, lint, testes unitários e E2E de Agents. Publicação, commit e push ficam com o mantenedor. O build mantém avisos de orçamento de CSS em telas preexistentes.

Comandos reproduzíveis, a partir da raiz:

```bash
npm --prefix ms-olivaw-ng run build
npm --prefix ms-olivaw-ng run lint
npm --prefix ms-olivaw-ng test
cd ms-olivaw-ng
npm run e2e -- e2e/admin-agents.spec.ts
```

## user-feedback

acho que a tela dashboard de acompanhamento do trevize n pode ser a tela de leads, ele precisa de uma dashboard própria de captacao e tals.
pode ate ter algum link com os leads mas n pode ser a mesma tela, penso em uma interface mais com dashboards e graficos, uma visao mais gerencial com graficos e tals, a listagem de itens processados em ambos pode ficar mais abaixo, mas antes precisamos de algo mais visaõ de 'floresta' sacou?

pense em algo ultimo para gente mostrar em graficos, para ambos os agents e pode ate vincular com leads os graficos, para saber quais dos itens descobertos por esse agents se transformaram em leads uteis.

bom pense em algo ai usando todo seu conhecimento de agents, leads e UI e UX blz.

toca o barco

## Integração com a issue031

Na finalização local das features 030/031, os gráficos de captação e segurança, filtros, listagem e navegação foram preservados. A página Trevize incorpora também o componente de indicadores de comunicação do CRM, com limites de amostra e sem iniciar coleta/campanha. Testes da integração: 14 unitários dos dashboards e 15 E2E de Agents/fichas aprovados. Nenhum push ou deploy foi executado nesta finalização.
