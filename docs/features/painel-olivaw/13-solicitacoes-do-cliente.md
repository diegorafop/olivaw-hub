# 13 — Problemas, dúvidas e melhorias no portal

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Permitir que o cliente abra e acompanhe pedidos com suas palavras, conectando-os à fila interna.

## Dependências

[Identidade do portal](11-identidade-do-portal.md), [dashboard do cliente](12-dashboard-do-cliente.md) e [atendimento interno](10-atendimento-interno.md).

## Escopo da entrega

- Ações Preciso de ajuda, Quero uma melhoria e Tenho uma dúvida, com perguntas textuais curtas.
- Solicitar alterações nos aplicativos, mais armazenamento, aumento de recursos e troca de plano, conforme pedido do mantenedor em 22/10/2026. Registrar serviço afetado, descrição e capacidade/plano desejado quando pertinente.
- Equipe avalia viabilidade, impacto, preço e prazo e registra a proposta antes de executar. Criar pedido não altera infraestrutura, contrato ou cobrança.
- Vincular solicitações atendidas às [entregas e notas de versão](18-historico-de-entregas.md) da própria empresa.
- Contexto de empresa, usuário e sistemas permitido pelo servidor; impacto relatado orienta triagem sem conceder prioridade ilimitada.
- Protocolo, comentários públicos, estados e histórico compreensível. Distinguir resumo de entrega de promessa de prazo.
- Acompanhar solicitação, responder perguntas e relatar persistência de problema com regras de reabertura.
- Incidentes amplos seguem para equipe sem aguardar análise de IA; correlação não duplica implementação.

## Fora desta entrega

- Prints e anexos, aprovação comercial automática e atendimento autônomo por IA.
- Encerrar reclamação simplesmente porque uma resposta foi gerada.

## Critérios de aceite

- Cliente abre e acompanha solicitação do próprio sistema; notas internas nunca entram na resposta pública.
- Trocar identificadores ou vínculos não permite ler/comentar pedido de outra empresa.
- Dúvida pode ser resolvida com resposta simples; melhoria pode aguardar definição e aceite.
- Falha de envio mantém o texto e a repetição não cria pedidos duplicados.

## Decisões antes de implementar

- Campos mínimos, estados públicos, regras de reabertura, destinatários e canais de comunicação.
- Política de anexos fica para uma feature própria antes de habilitar uploads.
