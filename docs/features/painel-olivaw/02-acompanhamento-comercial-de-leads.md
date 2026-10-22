# 02b — Acompanhamento comercial de leads

## Estado e objetivo

Planejada; sem implementação. Evolução separada da [consulta administrativa concluída](../done/02-leads-administrativos.md), vinculada à [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md).

Acompanhar a conversa comercial sem modificar o snapshot original enviado pelo visitante.

## Dependências

- Consulta e persistência de leads existentes.
- Definição das permissões de leitura e manutenção do acompanhamento.

## Escopo proposto

- Situações comerciais como novo, em conversa e arquivado, com transições a confirmar.
- Responsável interno e notas da equipe, separados do conteúdo da jornada.
- Filtro por situação e indicadores de acompanhamento com regras explícitas. Total de leads recebidos não é total de pendências.
- Auditoria, controle de concorrência e política de retenção para as alterações.

## Critérios de aceite

- Atualizar o acompanhamento não altera `Stage`, revisão pública ou snapshot do visitante.
- Reenvio idempotente da jornada preserva as informações comerciais da equipe.
- Somente operadores autorizados consultam e alteram responsável, notas e situação.
- Arquivamento preserva histórico e tem regra explícita de reabertura.
- Filtros e contadores usam estados persistidos, sem indicadores fictícios.

## Fora desta entrega

- Campanhas, mensagens automáticas, exportações e CRM completo.
- Criação automática de clientes, contas ou recursos.

## Decisões antes de implementar

Confirmar situações, transições, responsáveis, modelo de notas, permissões e retenção. Não gerar migrations sem solicitação explícita. A conversão manual de lead em empresa já pertence ao [cadastro de clientes concluído](../done/03-cadastro-de-clientes.md).
