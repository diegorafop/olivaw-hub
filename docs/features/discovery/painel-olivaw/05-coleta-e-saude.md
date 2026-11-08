# 05 — Primeira coleta e visão de saúde

## user-history

Como responsável pela operação da Olivaw,
quero contar com primeira coleta e visão de saúde,
para entregar uma primeira verificação confiável de um sistema, com origem e atualização visíveis, e distinguir indisponibilidade de ausência de dados..

## acceptance-criteria

- Falha controlada, recuperação, timeout, ausência de coleta e atraso produzem estados distintos e verificáveis sem usar produção como laboratório.
- Sem histórico suficiente, não mostrar disponibilidade de 100% ou promessa de cobertura.
- Coletas repetidas não duplicam amostras; erro de fonte não aparece como sistema saudável.
- Destinos de coleta são inventariados e autorizados; o coletor não permite acesso arbitrário a redes internas.

## ai-prompt

Incremento local em 30/10/2026: `ScheduledHealthCollector`, desabilitado por padrão, reutiliza a allowlist HTTPS e o bloqueio de endereços privados do PublicHealthProbe. Ciclo sem sobreposição, até 32 recursos explícitos, intervalo 5–60 minutos, timeout de cinco segundos por destino, chave determinística por janela e retenção 7–90 dias (somente amostras automáticas). GET `/health-readings/collector`, root, distingue desabilitado/sem execução de falhas de fonte. Sem destino autorizado não há coleta nem estado saudável inventado. Fonte HTTP não comprova fluxo completo, não abre incidentes nem promete disponibilidade. Falta a política externa de contingência e validação de fonte real; nenhuma coleta ativada em produção.


Primeiro recorte implementado: [05a — Verificações pontuais de saúde](../../done/05a-verificacoes-pontuais-de-saude.md). O restante desta visão permanece no planejamento, conforme as exclusões e pendências do recorte concluído.


Em descoberta; decisões e dependências ainda precisam ser resolvidas antes de mover para ready.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Entregar uma primeira verificação confiável de um sistema, com origem e atualização visíveis, e distinguir indisponibilidade de ausência de dados.

### Dependências

[Inventário](04-inventario-de-recursos.md). Escolher uma fonte limitada e autorizada antes de implementar coleta.

### Escopo da entrega

- Começar por uma única verificação acordada, como disponibilidade HTTP externa ou readiness de uma API inventariada.
- Definir coletor, frequência, timeout, repetição limitada, retenção e histórico suficiente para interpretar o resultado.
- Apresentar Operacional, Atenção, Incidente confirmado, Manutenção, Sem monitoramento e Dados desatualizados apenas conforme critérios definidos. Incidente confirmado depende de ocorrência confirmada; não inferir automaticamente de um único timeout.
- Mostrar fonte, instante, período coberto e limites da verificação. Um HTTP 200 não comprova todos os fluxos de negócio.
- Exibir saúde na ficha e dados resumidos no dashboard; falha do próprio coletor fica visível.
- Planejar verificação externa independente e comunicação de contingência antes de confiar no painel como única fonte operacional.

### Fora desta entrega

- Todas as métricas de infraestrutura, observabilidade completa ou varredura de endereços arbitrários.
- Central de incidentes, notificações externas, autorreparo ou ações remotas.

### Decisões antes de implementar

- Fonte inicial, critérios de saúde, destinos, ambiente de teste, janela de confirmação, retenção e credenciais mínimas.
- Localização técnica do coletor, limites operacionais e contingência; não criar um microservice sem necessidade demonstrada.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
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
```
