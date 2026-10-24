# 10 — Atendimento interno e acompanhamento de entregas

## user-history

Como responsável pela operação da Olivaw,
quero contar com atendimento interno e acompanhamento de entregas,
para organizar problemas, dúvidas e melhorias em uma fila interna antes de abrir o autoatendimento aos clientes..

## acceptance-criteria

- Pedido textual pode ser acompanhado de ponta a ponta com responsável e histórico.
- Permissões de leitura/escrita e notas internas são aplicadas pelo servidor.
- Tarefa compartilhada não expõe mensagens de outro cliente.
- Classificação de melhoria não muda contrato nem promete prazo por si só.

## ai-prompt

Primeiro recorte implementado: [10a — Atendimento interno por cliente](../../done/10a-atendimento-por-cliente.md). O restante desta visão permanece no planejamento, conforme as exclusões e pendências do recorte concluído.


Em descoberta; decisões e dependências ainda precisam ser resolvidas antes de mover para ready.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Organizar problemas, dúvidas e melhorias em uma fila interna antes de abrir o autoatendimento aos clientes.

### Dependências

[Clientes](../../done/03-cadastro-de-clientes.md) e [inventário](04-inventario-de-recursos.md). Correlação com incidentes usa a [central de alertas](06-alertas-e-incidentes.md) quando disponível, sem bloquear o atendimento manual.

### Escopo da entrega

- Criar solicitações internamente, vinculando cliente, sistema, solicitante, tipo, impacto, responsável e próxima ação.
- Protocolo, conversa, notas internas e histórico, com visibilidade pública/interna definida desde o modelo.
- Fluxo Recebido, Em análise, Aguardando você, Planejado, Em execução, Em validação, Entregue e Encerrado; cancelamento ou recusa têm motivo.
- Permitir caminhos mais curtos para dúvidas e regras de reabertura, sem exigir todos os estados.
- Relacionar uma tarefa a vários pedidos sem misturar conversas privadas; conectar ocorrências e entregas quando houver fontes.
- Priorizar impacto, responsável e próximo passo no atendimento e no dashboard.

### Fora desta entrega

- Portal, anexos, respostas automáticas, agentes executando trabalho e aprovação automática de escopo ou franquia.

### Decisões antes de implementar

- Tipos, transições, prioridade, reabertura, papéis e regra de comunicação.
- Como vincular solicitações a issues e entregas preservando o Git Flow e a privacidade.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 10 — Atendimento interno e acompanhamento de entregas

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Organizar problemas, dúvidas e melhorias em uma fila interna antes de abrir o autoatendimento aos clientes.

## Dependências

[Clientes](../../done/03-cadastro-de-clientes.md) e [inventário](04-inventario-de-recursos.md). Correlação com incidentes usa a [central de alertas](06-alertas-e-incidentes.md) quando disponível, sem bloquear o atendimento manual.

## Escopo da entrega

- Criar solicitações internamente, vinculando cliente, sistema, solicitante, tipo, impacto, responsável e próxima ação.
- Protocolo, conversa, notas internas e histórico, com visibilidade pública/interna definida desde o modelo.
- Fluxo Recebido, Em análise, Aguardando você, Planejado, Em execução, Em validação, Entregue e Encerrado; cancelamento ou recusa têm motivo.
- Permitir caminhos mais curtos para dúvidas e regras de reabertura, sem exigir todos os estados.
- Relacionar uma tarefa a vários pedidos sem misturar conversas privadas; conectar ocorrências e entregas quando houver fontes.
- Priorizar impacto, responsável e próximo passo no atendimento e no dashboard.

## Fora desta entrega

- Portal, anexos, respostas automáticas, agentes executando trabalho e aprovação automática de escopo ou franquia.

## Critérios de aceite

- Pedido textual pode ser acompanhado de ponta a ponta com responsável e histórico.
- Permissões de leitura/escrita e notas internas são aplicadas pelo servidor.
- Tarefa compartilhada não expõe mensagens de outro cliente.
- Classificação de melhoria não muda contrato nem promete prazo por si só.

## Decisões antes de implementar

- Tipos, transições, prioridade, reabertura, papéis e regra de comunicação.
- Como vincular solicitações a issues e entregas preservando o Git Flow e a privacidade.
```
