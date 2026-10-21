# 06 — Central de alertas e incidentes

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Transformar sinais em uma fila operacional com impacto, responsável e recuperação rastreável.

## Dependências

[Inventário](04-inventario-de-recursos.md) e [coleta de saúde](05-coleta-e-saude.md).

## Escopo da entrega

- Distinguir alerta de incidente; correlacionar sinais de uma causa e os sistemas afetados.
- Fluxo Detectado → Em avaliação → Em atendimento → Em observação → Resolvido, com reconhecimento auditável.
- Registrar impacto, gravidade, responsável, evidências, linha do tempo e próxima atualização.
- Central interna, agrupamento, intervalo entre avisos, manutenção e silenciamento com motivo e expiração.
- Conectar atenção agora e últimos acontecimentos ao dashboard. Notificações externas entram somente após definição de canal e política.
- Separar anotações internas de comunicados que poderão ser exibidos no portal.

## Fora desta entrega

- SLA ou atendimento permanente não contratado, WhatsApp presumido, execução automática em produção e recuperação sem verificação.

## Critérios de aceite

- Falha compartilhada gera uma ocorrência correlacionada, com os clientes afetados, sem explosão de avisos duplicados.
- Reconhecer não resolve; silenciar não desliga a coleta; recuperação segue a janela de confirmação.
- Permissões e visibilidade protegem evidências e notas internas.
- Falhas de notificação e de coleta são identificáveis; aviso enviado não é tratado como lido.

## Decisões antes de implementar

- Gravidades, critérios de abertura/encerramento, responsáveis, escalonamento, canal e contingência.
- Quem pode reconhecer, silenciar, comunicar e resolver cada ocorrência.
