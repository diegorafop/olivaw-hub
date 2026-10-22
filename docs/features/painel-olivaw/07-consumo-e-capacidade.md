# 07 — Consumo técnico e capacidade

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Medir o primeiro consumo útil por cliente e permitir avaliar crescimento, separando medição, estimativa e custo interno.

## Dependências

[Inventário](04-inventario-de-recursos.md) e mecanismo de coleta confiável da [feature de saúde](05-coleta-e-saude.md). Limites contratuais dependem da [feature de planos](08-planos-e-condicoes.md), mas medições podem existir antes deles.

## Escopo da entrega

- Escolher inicialmente uma métrica atribuível, como tamanho do banco ou arquivos, com definição, unidade, origem e instante.
- Distinguir tamanho atual, taxa por intervalo e acumulado no ciclo. Não somar repetidamente contadores cumulativos.
- Histórico, cobertura, atraso e barras discretas; sem limite definido, mostrar valor medido sem percentual fictício.
- Recursos compartilhados têm parcela não atribuída e regra de rateio versionada para custo interno. Uso total da VM não é medição individual.
- Separar consumo técnico, estimativa de custo interno e plano contratado; proteger custos por permissão.
- Ampliar métricas e reconciliar com provedores em incrementos posteriores desta feature, sem exigir todos os conectores na primeira entrega.

## Relação com o inventário por cliente

Usar a identidade dos recursos, implantações e vínculos com vigência definidos na [feature 04](04-inventario-de-recursos.md). Distinguir capacidade física/contratada, reserva de uma aplicação e uso medido. Um volume alcançado por API e banco é o mesmo recurso; suas amostras não são duplicadas porque existem vários caminhos até ele.

Separações lógicas precisam de fonte e qualidade conhecidas. Tamanho do banco não determina sozinho CPU/memória atribuíveis ao cliente; prefixo Redis não comprova isolamento ou quota; fila RabbitMQ pode ocupar memória/disco, mas sua atribuição depende da coleta disponível. Cada medição informa recurso, unidade, período, origem e qualidade, como medida, estimada ou informada manualmente. A capacidade ainda sem atribuição permanece explícita.

## Fora desta entrega

- Cobrança variável automática, suspensão, upgrade e alteração contratual por consumo.
- Previsões categóricas com poucas amostras e rateios arbitrários apresentados como medição.

## Critérios de aceite

- Reprocessar amostra não duplica consumo; valor informa unidade, fonte e período.
- Atraso, ausência e estimativa são explícitos; comparação de planos só usa limites vigentes aprovados.
- Custo interno não vaza na futura visão do cliente.
- Reconciliação identifica diferenças e consumo ainda sem atribuição, sem esconder lacunas.

## Decisões antes de implementar

- Primeira métrica, GB ou GiB, intervalo, ciclo, retenção, fonte autorizada e regra de atribuição.
- Quais custos podem ser estimados e quais precisam permanecer sem rateio nesta etapa.

## Apresentação no dashboard analítico

A aba Dashboard analítica receberá gráficos de crescimento e uso por período, com cliente/sistema, unidade, fonte e cobertura. A Visão geral mantém prioridades e resumos; gráfico sem histórico suficiente mostra estado vazio, não dados simulados.
