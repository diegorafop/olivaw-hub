# 02 — Consulta e acompanhamento de leads

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Dar à equipe acesso aos JSONs recebidos pela jornada e pelo contato direto. É a primeira fonte comercial útil para o dashboard, sem transformar automaticamente um interessado em cliente.

## Dependências

[Dashboard administrativo](01-dashboard-administrativo.md) e persistência de leads disponível no ambiente. O envio público já existe; consultas administrativas protegidas e acompanhamento interno ainda precisam ser implementados.

## Escopo da entrega

- Lista paginada de leads, com busca e filtros úteis por origem, data e situação comercial.
- Ficha legível com negócio, ideia, escolhas, plano de interesse, e-mail ou WhatsApp e histórico da jornada. Aceitar snapshots antigos sem WhatsApp.
- Definir acompanhamento comercial mínimo: novo, em conversa e arquivado, com responsável e notas internas; confirmar transições e persistência antes de codar.
- Criar consultas administrativas autorizadas, preservando o envio público sem oferecer leitura pública do lead ou revelar sua chave de escrita.
- Conectar ao dashboard o número de leads que precisam de atenção, com navegação para a lista correspondente.

## Fora desta entrega

- Envio automático de WhatsApp ou e-mail, campanhas e CRM completo.
- Conversão automática em cliente ou criação de recursos.
- Exposição de contatos em logs, métricas gerais ou notificações sem necessidade.

## Critérios de aceite

- Operador com permissão localiza e lê o lead; pessoa sem permissão não consulta, exporta ou altera seu acompanhamento.
- Contato por e-mail, só WhatsApp e snapshots antigos são apresentados corretamente.
- Envio idempotente continua funcionando e registros não são duplicados por uma atualização comercial.
- Paginação, ausência de resultados, falha de leitura e dados ainda não carregados têm estados distintos.

## Decisões antes de implementar

- Permissões de leitura e acompanhamento; estados comerciais e responsável.
- Retenção, arquivamento, necessidade de exportação e separação entre snapshot recebido e acompanhamento interno.
