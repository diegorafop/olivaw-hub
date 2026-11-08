# 13 — Problemas, dúvidas e melhorias no portal

## user-history

Como cliente da Olivaw,
quero contar com problemas, dúvidas e melhorias no portal,
para permitir que o cliente abra e acompanhe pedidos com suas palavras, conectando-os à fila interna..

## acceptance-criteria

- Cliente abre e acompanha solicitação do próprio sistema; notas internas nunca entram na resposta pública.
- Trocar identificadores ou vínculos não permite ler/comentar pedido de outra empresa.
- Dúvida pode ser resolvida com resposta simples; melhoria pode aguardar definição e aceite.
- Falha de envio mantém o texto e a repetição não cria pedidos duplicados.

## ai-prompt

### Entrega e validação local

Concluída em 24/10/2026 por Codex, na branch `feature/issue020-inventario-recursos`, worktree atual.

Pedidos por empresa e serviço, protocolo, texto original, capacidades/plano desejados, acompanhamento, mensagens públicas e reabertura auditada. A equipe responde no atendimento interno com separação explícita de mensagens privadas e públicas. Chaves de reenvio com hash do conteúdo evitam duplicação e alteração silenciosa da mesma submissão. Testes do servidor verificam conversas públicas sem notas internas, nome interno de atendente ou dados de outra empresa. E2E desktop/celular verifica pedido com texto original e chave de idempotência. Enviar pedido não muda infraestrutura, contrato, cobrança ou plano; propostas e aceite formal continuam na feature 14.

### Banco e publicação

Migrations PostgreSQL `Issue020Panel` e `Issue020PortalIdentity` geradas e SQL revisado, sem alterar a `Issue020` publicada. Modelos de ambos os contextos sem diferenças pendentes. Suíte da API: 359 testes aprovados; Angular: 33; E2E do lote administrativo: 29; portal: 2. Build Release da API e build/lint Angular verificados. SQL Server e Oracle não receberam migrations.

Não houve conexão a banco real, execução de SQL, integração Git Flow, push ou publicação. O startup da nova imagem aplicará as duas migrations automaticamente. O acesso externo do portal exige `PortalIdentity:Issuer`, `Audience`, `SigningKey` e `TokenMinutes` independentes da autoridade interna; sem configuração válida, o login retorna indisponibilidade. Conferência no ambiente permanece pendente.

### Referências

Veja [operação interna](../../../operacao-interna.md) e [comandos da API](../../../../ms-olivaw-api/README.md). A numeração organiza funcionalidades, sem criar issues adicionais. O feedback abaixo preserva o planejamento original e não redefine o estado atual.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 13 — Problemas, dúvidas e melhorias no portal

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Permitir que o cliente abra e acompanhe pedidos com suas palavras, conectando-os à fila interna.

## Dependências

[Identidade do portal](11-identidade-do-portal.md), [dashboard do cliente](12-dashboard-do-cliente.md) e [atendimento interno](../../discovery/painel-olivaw/10-atendimento-interno.md).

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
```
