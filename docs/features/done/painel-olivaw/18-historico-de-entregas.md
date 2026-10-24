# 18 — Histórico de entregas e notas de versão do cliente

## user-history

Como responsável pela operação da Olivaw,
quero contar com histórico de entregas e notas de versão do cliente,
para registrar as funcionalidades implementadas para cada cliente e apresentar uma linha do tempo de entregas em linguagem compreensível..

## acceptance-criteria

- Cliente consulta somente entregas publicadas das empresas às quais possui vínculo autorizado.
- Rascunhos, observações internas e segredos não são expostos ao portal.
- Solicitação em análise não aparece como entrega; publicação da nota não dispara deploy.
- Equipe consegue registrar uma entrega manualmente, sem depender de GitHub ou de integração de deployment.
- Correções em notas publicadas preservam histórico e autoria.

## ai-prompt

### Entrega e validação local

Concluída em 24/10/2026 por Codex, na branch `feature/issue020-inventario-recursos`, worktree atual.

Registro manual interno por empresa, serviço e pedido opcional, categorias, versão, resumo público e nota interna. Rascunho primeiro; publicação exige confirmação e data de entrega passada; pedido vinculado precisa estar concluído. Correções preservam snapshots e autoria; retirada exige motivo. Linha do tempo pública paginada por data/ID, com filtros e somente notas publicadas da empresa autorizada. Testes cobrem confirmação, histórico, privacidade, isolamento, tradução SQL PostgreSQL/SQL Server e consulta no navegador desktop/celular. Registrar ou publicar uma nota não dispara deploy.

### Banco e publicação

Migrations PostgreSQL `Issue020Panel` e `Issue020PortalIdentity` geradas e SQL revisado, sem alterar a `Issue020` publicada. Modelos de ambos os contextos sem diferenças pendentes. Suíte da API: 359 testes aprovados; Angular: 33; E2E do lote administrativo: 29; portal: 2. Build Release da API e build/lint Angular verificados. SQL Server e Oracle não receberam migrations.

Não houve conexão a banco real, execução de SQL, integração Git Flow, push ou publicação. O startup da nova imagem aplicará as duas migrations automaticamente. O acesso externo do portal exige `PortalIdentity:Issuer`, `Audience`, `SigningKey` e `TokenMinutes` independentes da autoridade interna; sem configuração válida, o login retorna indisponibilidade. Conferência no ambiente permanece pendente.

### Referências

Veja [operação interna](../../../operacao-interna.md) e [comandos da API](../../../../ms-olivaw-api/README.md). A numeração organiza funcionalidades, sem criar issues adicionais. O feedback abaixo preserva o planejamento original e não redefine o estado atual.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 18 — Histórico de entregas e notas de versão do cliente

## Estado e objetivo

Planejada em 22/10/2026 a pedido do mantenedor; sem implementação. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md).

Registrar as funcionalidades implementadas para cada cliente e apresentar uma linha do tempo de entregas em linguagem compreensível.

## Dependências

- [Clientes](../03-cadastro-de-clientes.md) e [serviços vinculados](../04a-cadastro-de-servicos.md).
- [Identidade do portal](11-identidade-do-portal.md) para consulta externa.
- Integração opcional com [solicitações](13-solicitacoes-do-cliente.md) e [publicações técnicas](../../discovery/painel-olivaw/09-publicacoes-e-cuidados.md).

## Escopo da entrega

- Registro interno por empresa e serviço, com título, resumo das mudanças, data da entrega e versão quando houver.
- Identificar funcionalidades, melhorias e correções e permitir vínculo com solicitações atendidas.
- Preparar rascunho e publicar explicitamente a nota para o cliente; implementação em branch ou teste em homologação não comprova entrega em produção.
- Linha do tempo paginada, ordenada por data, com filtros por serviço e período.
- Separar conteúdo público de notas internas, referências técnicas e dados de outras empresas.
- Auditar criação, publicação e correção das notas, preservando rastreabilidade.

## Critérios de aceite

- Cliente consulta somente entregas publicadas das empresas às quais possui vínculo autorizado.
- Rascunhos, observações internas e segredos não são expostos ao portal.
- Solicitação em análise não aparece como entrega; publicação da nota não dispara deploy.
- Equipe consegue registrar uma entrega manualmente, sem depender de GitHub ou de integração de deployment.
- Correções em notas publicadas preservam histórico e autoria.

## Fora desta entrega

- Geração automática de notas por IA, publicação automática a partir de commits e execução de deploy.
- Alteração automática de contratos, planos ou cobranças.

## Decisões antes de implementar

- Campos obrigatórios, confirmação da entrega e permissões de publicação/correção.
- Política de retenção e eventual comunicação de novas versões; nenhum expurgo automático foi aprovado.
```
