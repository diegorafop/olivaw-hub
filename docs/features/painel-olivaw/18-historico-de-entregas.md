# 18 — Histórico de entregas e notas de versão do cliente

## Estado e objetivo

Planejada em 22/10/2026 a pedido do mantenedor; sem implementação. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md).

Registrar as funcionalidades implementadas para cada cliente e apresentar uma linha do tempo de entregas em linguagem compreensível.

## Dependências

- [Clientes](../done/03-cadastro-de-clientes.md) e [serviços vinculados](../done/04a-cadastro-de-servicos.md).
- [Identidade do portal](11-identidade-do-portal.md) para consulta externa.
- Integração opcional com [solicitações](13-solicitacoes-do-cliente.md) e [publicações técnicas](09-publicacoes-e-cuidados.md).

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
