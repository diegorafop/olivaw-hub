# 05 — Primeira coleta e visão de saúde

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Entregar uma primeira verificação confiável de um sistema, com origem e atualização visíveis, e distinguir indisponibilidade de ausência de dados.

## Dependências

[Inventário](04-inventario-de-recursos.md). Escolher uma fonte limitada e autorizada antes de implementar coleta.

## Escopo da entrega

- Começar por uma única verificação acordada, como disponibilidade HTTP externa ou readiness de uma API inventariada.
- Definir coletor, frequência, timeout, repetição limitada, retenção e histórico suficiente para interpretar o resultado.
- Apresentar Operacional, Atenção, Incidente confirmado, Manutenção, Sem monitoramento e Dados desatualizados apenas conforme critérios definidos. Incidente confirmado depende de ocorrência confirmada; não inferir automaticamente de um único timeout.
- Mostrar fonte, instante, período coberto e limites da verificação. Um HTTP 200 não comprova todos os fluxos de negócio.
- Exibir saúde na ficha e dados resumidos no dashboard; falha do próprio coletor fica visível.
- Planejar verificação externa independente e comunicação de contingência antes de confiar no painel como única fonte operacional.

## Fora desta entrega

- Todas as métricas de infraestrutura, observabilidade completa ou varredura de endereços arbitrários.
- Central de incidentes, notificações externas, autorreparo ou ações remotas.

## Critérios de aceite

- Falha controlada, recuperação, timeout, ausência de coleta e atraso produzem estados distintos e verificáveis sem usar produção como laboratório.
- Sem histórico suficiente, não mostrar disponibilidade de 100% ou promessa de cobertura.
- Coletas repetidas não duplicam amostras; erro de fonte não aparece como sistema saudável.
- Destinos de coleta são inventariados e autorizados; o coletor não permite acesso arbitrário a redes internas.

## Decisões antes de implementar

- Fonte inicial, critérios de saúde, destinos, ambiente de teste, janela de confirmação, retenção e credenciais mínimas.
- Localização técnica do coletor, limites operacionais e contingência; não criar um microservice sem necessidade demonstrada.

## Apresentação no dashboard analítico

Gráficos de disponibilidade e latência podem inaugurar uma aba Dashboard analítica, separada da Visão geral de prioridades. Exibir período, fonte, cobertura e lacunas; não preencher falhas de coleta como disponibilidade confirmada.
