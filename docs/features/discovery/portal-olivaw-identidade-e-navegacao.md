# Painéis Olivaw — Identidade, endereço e navegação

## user-history

Como responsável pela Olivaw,
quero estudar o nome, público e navegação dos painéis do hub,
para diferenciar a operação interna, o portal do cliente e as áreas de gestão dos sistemas contratados.

## acceptance-criteria

- Identificar o público do painel antes de escolher o endereço definitivo: equipe Olivaw ou cliente acompanhado pela Olivaw.
- Diferenciar funcionalidades existentes, evolução pendente e propostas deste estudo.
- Definir nome exibido, endereço de entrada, login e organização da navegação antes de implementar mudanças.
- Preservar a separação entre identidade administrativa, identidade do portal e identidades dos sistemas dos clientes.
- Planejar compatibilidade de links e convites existentes, inclusive login, query strings e navegação, se houver renomeação.
- Limitar o trabalho autorizado à prévia visual interna de `/portal`; a migração do portal real, redirects, APIs, migrations, DNS e publicação aguardam definição própria.

## ai-prompt

### Estado

Atualização de implementação local em 30/10/2026: `/portal` e `/portal/entrar` passam a ser os endereços reais do cliente, usando a autoridade PortalIdentity existente. `/meu-negocio` e `/meu-negocio/entrar` redirecionam preservando query strings e convites. Links emitidos pela administração usam o endereço novo. A demonstração estática fica em `/admin/portal-home-preview`, com guard administrativo root e sem gravação real. Não alterar `/admin` ou DNS. Sem commit, push ou deploy. O estudo original abaixo é histórico.


Discovery. Em 25/10/2026, o mantenedor aprovou `/portal` para o cliente e autorizou uma prévia visual da Home com dados ilustrativos. A prévia está temporariamente protegida por acesso administrativo root e não migra o portal real. As atividades e features serão definidas depois da revisão da UI. Sem issue própria; não associar esta evolução à issue025 do Baley. As opções abaixo preservam o estudo inicial, não decisões sobre a operação interna.

A separação obrigatória de roles, audiences e autorização entre Portal e DevOps está registrada em [Segurança JWT](seguranca-jwt-portal-e-devops.md); ainda não implementada por este estudo.

### Qual é a feature existente?

A visão maior é a [épica Administração Olivaw e portal do cliente](painel-administrativo-e-portal-do-cliente.md). Ela agrega entregas com estados distintos; não deve ser apresentada como integralmente publicada apenas porque existem arquivos em done.

| Espaço | Implementação observada no código | Documentação de referência |
| --- | --- | --- |
| Operação interna Olivaw | `/admin`, login `/login`; visão geral, leads, cadastros/configuração e Agents | [01 — Dashboard administrativo](../done/01-dashboard-administrativo.md) |
| Portal do cliente, chamado Meu negócio | `/meu-negocio` e `/meu-negocio/entrar`; empresas autorizadas, serviços publicados, estado da última verificação, solicitações, plano e entregas | [11 — Identidade](../done/painel-olivaw/11-identidade-do-portal.md), [12 — Dashboard](../done/painel-olivaw/12-dashboard-do-cliente.md), [13 — Solicitações](../done/painel-olivaw/13-solicitacoes-do-cliente.md), [18 — Entregas](../done/painel-olivaw/18-historico-de-entregas.md) |
| Evolução comercial do portal | Consulta do plano e pedido de alteração implementados parcialmente; aceite de proposta e evolução comercial pendentes | [14 — Meu plano e evolução](../in-progress/painel-olivaw/14-plano-e-evolucao-no-portal.md) |
| Gestão de um sistema de cliente | Exemplo: `/gestao` do frontend LTMJ; pertence ao sistema do cliente, não ao portal Olivaw | [Documentação LTMJ](../../clients/ltmj/README.md) |

Leitura local de `ms-olivaw-ng/src/app/app.routes.ts` e `features/portal/portal-page.*`. Esta conferência não acessou produção, contas de clientes ou banco. A presença no código não prova publicação ou uso por um cliente real. No portal, ausência de uma fonte ou de plano confirmado não deve virar informação demonstrativa apresentada como real.

### Opções de endereço

| Endereço | Público mais adequado | Avaliação |
| --- | --- | --- |
| `/admin` | Equipe Olivaw | Já existe e já se diferencia de `/gestao`; pode permanecer se o objetivo for apenas evitar coincidência com clientes. |
| `/operacao` | Equipe Olivaw | Recomendado se quisermos um nome próprio para o trabalho interno da Olivaw. Abrange cadastros, infraestrutura, atendimento e agentes. |
| `/portal` | Cliente da Olivaw | Recomendação para um espaço amplo de serviços, atendimento, plano e entregas. Curto e fácil de comunicar. |
| `/meu-negocio` | Cliente da Olivaw | Nome atual, acolhedor e já implementado; exige esclarecer que acompanha a relação com a Olivaw, não substitui a gestão do negócio do cliente. |
| `/suporte` | Atendimento | Natural em português quando o foco é pedir ajuda e acompanhar solicitações. Mais estreito que todo o portal. |
| `/support` | Atendimento | Alternativa em inglês; manter idioma consistente na interface. A grafia usual é support, não suport. |

Recomendação de trabalho: **Operação Olivaw** para a equipe e **Portal Olivaw** para os clientes. Se os dois endereços forem revistos, considerar `/operacao` e `/portal`, respectivamente. O endereço `/portal` foi aprovado para o cliente; `/operacao` continua apenas uma recomendação. Não criar novo subdomínio ou alterar Registro.br para resolver uma decisão de caminho no frontend.

### O que estudar na experiência

Para o cliente, partir do que já existe: visão dos serviços, solicitações, meu plano e entregas. A página inicial deve responder: quais serviços acompanho, há algo exigindo atenção e o que preciso fazer agora? Usar linguagem simples, sem relatórios técnicos de segurança, custos internos, agentes ou topologia da VM.

Para a equipe, preservar o painel administrativo e suas autorizações, com acesso aos cadastros e ferramentas de operação. Atendimento interno não deve dar ao cliente as permissões da equipe. O `/admin/support` atual é uma ferramenta interna, não um endereço de portal público.

Antes de codar, decidir se a intenção é apenas trocar o endereço, dar identidade visual própria ao portal ou redesenhar a navegação. Evitar juntar os três trabalhos implicitamente. Reutilizar componentes úteis e os contratos existentes, sem refazer identidade por consequência da URL.

### Decisões pendentes

- O pedido se refere ao portal do cliente, à operação interna ou a ambos?
- Manter Meu negócio como nome exibido ou adotar Portal Olivaw?
- Suporte será uma seção do portal ou o foco do produto inteiro?
- Se houver troca, manter os endereços atuais como atalhos de compatibilidade? Conferir convites, login, expiração, retorno e links compartilhados antes de decidir redirects.
- Quais entregas são necessárias para o primeiro cliente usar o portal, e quais permanecem planejadas?

### Próximo passo

Revisar a prévia da Home com o mantenedor antes de especificar atividades e implementar integrações reais. As entregas existentes continuam em seus estados; somente esta evolução fica em discovery e não entra no índice de funcionalidades concluídas.

## user-feedback

```text
fizemos algo do painel de gestao do olivaw? eu acho que para n ficar /gestao igual dos clientes poderiamos colocar um /suport ou tem outras ideias?
qual é essa feature mesmo? vamos estudar ela antes de codar
```

```text
acho que /portal é melhor mesmo boa.
após ver a UI a gente definir as atividades e registra em docs as features.
```
