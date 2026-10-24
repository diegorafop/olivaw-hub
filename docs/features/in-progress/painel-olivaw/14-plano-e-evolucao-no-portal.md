# 14 — Meu plano e evolução no portal

## user-history

Como cliente da Olivaw,
quero contar com meu plano e evolução no portal,
para mostrar o que foi contratado e o que está sendo planejado ou entregue, sem transformar interesse em aprovação comercial..

## acceptance-criteria

- Plano e histórico respeitam vigência e não expõem custo ou margem interna.
- Pedidos ainda em análise não aparecem como funcionalidades aprovadas.
- Cliente distingue entregue, planejado, em avaliação e aguardando sua decisão.
- Pedido de revisão não muda franquia nem mensalidade.

## ai-prompt

Em desenvolvimento local, a pedido do mantenedor. Decisões em aberto serão apresentadas como sugestões, sem dados de produção inventados. Responsável: Codex desta sessão, execução sequencial no worktree atual, sem integração ou publicação.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Parcialmente implementada: consulta do plano vigente e pedidos de alteração disponíveis; aceite de proposta e evolução comercial ainda pendentes. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Mostrar o que foi contratado e o que está sendo planejado ou entregue, sem transformar interesse em aprovação comercial.

### Dependências

[Identidade](../../done/painel-olivaw/11-identidade-do-portal.md), [dashboard do cliente](../../done/painel-olivaw/12-dashboard-do-cliente.md), [planos internos](../../discovery/painel-olivaw/08-planos-e-condicoes.md) e [atendimento](../../discovery/painel-olivaw/10-atendimento-interno.md). Consumo medido depende da [feature 07](../../discovery/painel-olivaw/07-consumo-e-capacidade.md).

### Escopo da entrega

- Consultar serviços, limites, ciclo e condições vigentes, com linguagem simples.
- Mostrar capacidade medida e franquias disponíveis/reservadas/concluídas somente quando definidas e confiáveis.
- Listar evolução combinada, primeiro MVP, etapas posteriores e entregas verificadas, com vínculos às solicitações do próprio cliente.
- Pedir avaliação de capacidade ou mudança de plano sem alterar automaticamente o contratado.
- Registrar aceite de proposta de evolução somente após definir mecanismo, responsáveis e evidências.

### Fora desta entrega

- Pagamentos, faturas, upgrade instantâneo, cobrança variável e promessas de entrega sem aprovação.

A consulta futura de cobranças e pagamentos anteriores está prevista na [feature 17 — Financeiro](../../discovery/painel-olivaw/17-financeiro.md), com autorização por empresa. Continua fora da implementação desta feature; não inclui processamento automático de pagamentos. Cadastro de cliente permanece sem login até a entrega da identidade do portal.

### Decisões antes de implementar

- Condições comerciais publicáveis, mecanismos de aceite e quais dados históricos o cliente pode consultar.
- Quando uma alteração reserva ou consome franquia, conforme acordo aprovado.



### Decisões sugeridas para a implementação

- Portal usa autoridade própria no mesmo host, tabelas prefixadas e chave/emissor/audiência independentes, reutilizando Olivaw Identity. Não há outro processo permanente na VM.
- Convites têm validade inicial de 48 horas e papéis Consulta, Solicitações e Gestão; nenhum vínculo com empresas é concedido automaticamente pelo cadastro.
- A prévia administrativa é identificada como somente consulta e não gera sessão de cliente.
- Campos ainda sem fonte, preço ou integração ficam pendentes; sugestões de produto não viram medições, contratos ou aprovações reais.
- A base de condições por cliente e a identidade do portal estão cobertas pelas migrations PostgreSQL de 24/10/2026. Esta feature permanece em andamento até concluir propostas, aceite e histórico comercial.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 14 — Meu plano e evolução no portal

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Mostrar o que foi contratado e o que está sendo planejado ou entregue, sem transformar interesse em aprovação comercial.

## Dependências

[Identidade](../../done/painel-olivaw/11-identidade-do-portal.md), [dashboard do cliente](../../done/painel-olivaw/12-dashboard-do-cliente.md), [planos internos](../../discovery/painel-olivaw/08-planos-e-condicoes.md) e [atendimento](../../discovery/painel-olivaw/10-atendimento-interno.md). Consumo medido depende da [feature 07](../../discovery/painel-olivaw/07-consumo-e-capacidade.md).

## Escopo da entrega

- Consultar serviços, limites, ciclo e condições vigentes, com linguagem simples.
- Mostrar capacidade medida e franquias disponíveis/reservadas/concluídas somente quando definidas e confiáveis.
- Listar evolução combinada, primeiro MVP, etapas posteriores e entregas verificadas, com vínculos às solicitações do próprio cliente.
- Pedir avaliação de capacidade ou mudança de plano sem alterar automaticamente o contratado.
- Registrar aceite de proposta de evolução somente após definir mecanismo, responsáveis e evidências.

## Fora desta entrega

- Pagamentos, faturas, upgrade instantâneo, cobrança variável e promessas de entrega sem aprovação.

A consulta futura de cobranças e pagamentos anteriores está prevista na [feature 17 — Financeiro](../../discovery/painel-olivaw/17-financeiro.md), com autorização por empresa. Continua fora da implementação desta feature; não inclui processamento automático de pagamentos. Cadastro de cliente permanece sem login até a entrega da identidade do portal.

## Critérios de aceite

- Plano e histórico respeitam vigência e não expõem custo ou margem interna.
- Pedidos ainda em análise não aparecem como funcionalidades aprovadas.
- Cliente distingue entregue, planejado, em avaliação e aguardando sua decisão.
- Pedido de revisão não muda franquia nem mensalidade.

## Decisões antes de implementar

- Condições comerciais publicáveis, mecanismos de aceite e quais dados históricos o cliente pode consultar.
- Quando uma alteração reserva ou consome franquia, conforme acordo aprovado.
```
