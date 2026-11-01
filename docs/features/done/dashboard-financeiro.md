# Financeiro — dashboard e navegação de despesas

## user-history

Como operador da Olivaw,
quero uma área financeira com indicadores e gráficos filtrados por período,
para entender os gastos e navegar até seus lançamentos sem procurar a tela na Configuração.

## acceptance-criteria

- Financeiro é uma entrada lateral exclusiva de root; Despesas sai dos cards de Configuração.
- Dashboard e Despesas mantêm Financeiro ativo e breadcrumb clicável, sem perder a rota existente de despesas.
- Filtros simples de dia, semana, mês e ano, data de referência e cliente atualizam indicadores e gráficos.
- Links para Despesas preservam o recorte de datas/cliente; URL do dashboard permite compartilhar o mesmo filtro e usar voltar/avançar.
- Separar custos brutos, créditos e líquido; não chamar custo líquido de saldo bancário, lucro ou pagamento confirmado.
- Gráficos mostram histórico, distribuição por categoria e por cliente a partir de todos os registros do período, não apenas de uma página da listagem.
- Custos compartilhados sem rateio ficam identificados, sem duplicar o valor integral para cada cliente.
- Ausência de lançamentos, falha de consulta e dados disponíveis têm estados distintos; nenhum valor ilustrativo é apresentado como despesa real.
- Usar o endpoint root existente com ampliação aditiva dos agregados, sem nova tabela ou migration.

## ai-prompt

Desenvolvimento concluído e validado localmente por Codex, branch feature/issue026-admin-client, worktree atual, sem commit, push ou deploy. Autorização específica do mantenedor para retirar Despesas de Configuração e criar Financeiro na navegação lateral prevalece sobre a convenção anterior de manter todos os cadastros na Configuração. Atualizar as diretrizes locais para registrar essa exceção.

Referências: [Power BI — filtros de data](https://learn.microsoft.com/en-us/power-bi/visuals/desktop-slicer-filter-date-range), [QuickBooks — dashboard e despesas por categoria](https://quickbooks.intuit.com/learn-support/en-us/help-article/product-setup/business-dashboard-details-quickbooks-online-app/L46DORrt9_US_en_US) e [FreshBooks — dashboard](https://support.freshbooks.com/hc/en-us/articles/115015407988-How-do-I-use-my-dashboard). Adotar hierarquia de indicadores, filtro temporal único e detalhamento por categoria/cliente, sem copiar a interface ou introduzir dados que o hub ainda não possui. Consulta em 27/10/2026.

Escopo: dashboard de gastos reais, não contabilidade, conciliação bancária, receitas, faturamento ou lucro. Períodos de calendário UTC e semana de segunda a domingo, com rótulos claros. A data do lançamento financeiro não é automaticamente data de pagamento. API atual limitada a 366 dias e 5.000 registros por consulta; manter erro explícito em vez de somar só parte dos dados. Taxa de conversão dos lançamentos permanece a taxa registrada, sem reprecificação retroativa.

A estrutura de fatura AWS é um discovery separado: [Faturas AWS e dados originais](../discovery/faturas-aws-detalhes-e-dados-originais.md). Nesta etapa ela não ganha entidade ou endpoint. Os quatro lançamentos de setembro solicitados usam a API de despesas existente, com reconciliação e evidência JSON.

### Entrega e validação

Dashboard `/admin/finance` com acesso root, filtros na URL, períodos de calendário UTC, custos brutos, créditos e líquido, histórico, categorias, clientes e lançamentos recentes. Despesas continua em `/admin/expenses`, fora de Configuração, com breadcrumb e menu Financeiro ativos e recorte preservado. O filtro `customer=shared` usa `unallocatedOnly=true`; não inclui custos diretamente atribuídos a clientes. Não há dados fictícios no dashboard.

A API ganhou somente campos aditivos `byCategory/byCustomer` e o filtro de compartilhadas em `GET /expenses`, sem alteração do modelo ou migration. Agregados usam todo o recorte antes de paginar os lançamentos. Limites de 366 dias e 5.000 lançamentos preservados. Estados vazio, falha e carregamento são distintos. Nenhum coletor de AWS, banco ou pagamento automático foi introduzido.

Referência do cadastro AWS: setembro/2026, quatro lançamentos consultados e confirmados, USD 4,49 em custos e USD 4,49 em créditos, líquido zero. Câmbio gerencial estimado de R$5,50/USD identificado nas fontes; não representa pagamento real. A evidência e IDs estão no JSON da feature de faturas. Nenhum custo adicional aproximado de USD 5 foi criado, pois duplicaria os lançamentos existentes.

Validações: build e lint do frontend passaram, 3 testes unitários de calendário (inclusive semana entre anos, ano bissexto e data inválida), 17 E2E em desktop/celular e 416 testes da API passaram. Cinco testes de despesas foram repetidos após ajustes nas asserções. SQL/modelo sem alteração; migration nova desnecessária. Somente avisos SCSS preexistentes. Nenhuma credencial foi escrita nos documentos ou nas respostas.

Publicação de backend e frontend fica com o mantenedor. Atualizar a API antes do frontend para disponibilizar os novos agregados/filtro; não houve commit, push ou deploy nesta entrega. A nova estrutura de fatura AWS permanece em discovery e não foi implementada.

## user-feedback

```text
criaremos tbm uma dashboard Financeiro, na verdade é ela que será exibida em menu lateral esquerdo.
Essa feature é um dashboard com graficos de gastos e tals, voce vai pensar em como fazer baseado em dashboards financeiros e BI famosos.

tem que ter filtros simples de dia/semana/mes/ano.
e dessa dashboard será possivel navegar até a tela despesas atual que vai sair do menu configuracoes.
enfim teremos uma area financeira de verdade.

faça POR favor.

OS DADOS SAO SETEMBRO...
pode continuar
```
