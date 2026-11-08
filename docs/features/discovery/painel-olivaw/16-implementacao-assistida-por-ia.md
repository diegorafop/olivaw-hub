# 16 — Implementação e revisão assistidas por IA

## user-history

Como responsável pela operação da Olivaw,
quero contar com implementação e revisão assistidas por ia,
para transformar um pedido com escopo aprovado em uma alteração isolada, testável e revisável, com rastreabilidade até a entrega..

## acceptance-criteria

- Uma solicitação aprovada produz artefato revisável sem alterar ambiente real durante preparação.
- Falhas nos testes e limites de execução impedem avanço automático e ficam visíveis à equipe.
- Instruções externas não mudam ferramentas, permissões ou escopo aprovado.
- A relação pedido → issue/alteração → revisão → publicação autorizada → verificação pode ser auditada.

## ai-prompt

Em 30/10/2026 o mantenedor pediu explicitamente não implementar pendências de IA e deixá-las para análise posterior. Nenhum executor novo ficou no repositório e nenhuma chamada de IA foi executada. Não retomar esta feature como consequência do fechamento de Planos.


Em descoberta; decisões e dependências ainda precisam ser resolvidas antes de mover para ready.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Transformar um pedido com escopo aprovado em uma alteração isolada, testável e revisável, com rastreabilidade até a entrega.

### Dependências

[Assistência por IA](15-triagem-e-diagnostico-com-ia.md), [atendimento](10-atendimento-interno.md), política de execução isolada e definição dos repositórios/ambientes permitidos.

### Escopo da entrega

- Equipe define prioridade, escopo e critérios antes da execução. Agente prepara patch em ambiente isolado e executa verificações adequadas.
- Mostrar diff, evidências, testes, falhas, custo estimado, riscos e ação aguardada para revisão humana.
- Aplicar menor acesso necessário por cliente, projeto e repositório; não fornecer credenciais gerais de produção.
- Preservar Git Flow clássico, rastreabilidade da issue quando existente e PR opcional. Revisão não impõe mudança de fluxo.
- Cancelamento, falha e reexecução têm histórico e limites; separar autor da alteração e decisão de aprovação.
- Entrega só é comunicada como disponível depois de publicação autorizada e verificação. Merge, deploy e ações operacionais continuam exigindo autorização explícita.

### Fora desta entrega

- Implementação automática a partir de qualquer ticket, aprovação do próprio trabalho ou autoatendimento irrestrito de produção.
- Fechamento de issues em develop, mudança de contrato por agente e promessa automática de prazo.

### Decisões antes de implementar

- Isolamento do executor, repositórios, credenciais restritas, permissões e quem aprova cada etapa.
- Critérios de aceite técnico/comercial e processo de comunicação e validação com o cliente.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 16 — Implementação e revisão assistidas por IA

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Transformar um pedido com escopo aprovado em uma alteração isolada, testável e revisável, com rastreabilidade até a entrega.

## Dependências

[Assistência por IA](15-triagem-e-diagnostico-com-ia.md), [atendimento](10-atendimento-interno.md), política de execução isolada e definição dos repositórios/ambientes permitidos.

## Escopo da entrega

- Equipe define prioridade, escopo e critérios antes da execução. Agente prepara patch em ambiente isolado e executa verificações adequadas.
- Mostrar diff, evidências, testes, falhas, custo estimado, riscos e ação aguardada para revisão humana.
- Aplicar menor acesso necessário por cliente, projeto e repositório; não fornecer credenciais gerais de produção.
- Preservar Git Flow clássico, rastreabilidade da issue quando existente e PR opcional. Revisão não impõe mudança de fluxo.
- Cancelamento, falha e reexecução têm histórico e limites; separar autor da alteração e decisão de aprovação.
- Entrega só é comunicada como disponível depois de publicação autorizada e verificação. Merge, deploy e ações operacionais continuam exigindo autorização explícita.

## Fora desta entrega

- Implementação automática a partir de qualquer ticket, aprovação do próprio trabalho ou autoatendimento irrestrito de produção.
- Fechamento de issues em develop, mudança de contrato por agente e promessa automática de prazo.

## Critérios de aceite

- Uma solicitação aprovada produz artefato revisável sem alterar ambiente real durante preparação.
- Falhas nos testes e limites de execução impedem avanço automático e ficam visíveis à equipe.
- Instruções externas não mudam ferramentas, permissões ou escopo aprovado.
- A relação pedido → issue/alteração → revisão → publicação autorizada → verificação pode ser auditada.

## Decisões antes de implementar

- Isolamento do executor, repositórios, credenciais restritas, permissões e quem aprova cada etapa.
- Critérios de aceite técnico/comercial e processo de comunicação e validação com o cliente.
```


segura ai, n faz nada de pendencia de IA

deixa para analisae
