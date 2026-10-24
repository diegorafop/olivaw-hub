# 12 — Dashboard Meu negócio

## user-history

Como cliente da Olivaw,
quero contar com dashboard meu negócio,
para apresentar ao cliente uma visão simples e autorizada dos sistemas que a Olivaw acompanha..

## acceptance-criteria

- Cada cliente vê exclusivamente seus sistemas e dados úteis.
- Falha de fonte nunca é traduzida como tudo funcionando; sem histórico não há percentual inventado.
- Desktop, celular, teclado e movimento reduzido permitem consultar e navegar.
- Ações e links correspondem a funcionalidades reais, sem atalhos falsos para módulos futuros.

## ai-prompt

### Entrega e validação local

Concluída em 24/10/2026 por Codex, na branch `feature/issue020-inventario-recursos`, worktree atual.

Meu negócio apresenta empresas autorizadas, serviços explicitamente publicados pela equipe, links públicos HTTPS e estado da última verificação. Sem fonte recente, mostra ausência de confirmação ou desatualização; não inventa disponibilidade histórica. A prévia administrativa é somente consulta e não gera sessão de cliente. Desktop e celular foram verificados no navegador; respostas públicas são testadas contra exposição de topologia, custos e notas internas. Consumo agregado e franquias permanecem nas evoluções 07/14, sem atalhos para telas inexistentes.

### Banco e publicação

Migrations PostgreSQL `Issue020Panel` e `Issue020PortalIdentity` geradas e SQL revisado, sem alterar a `Issue020` publicada. Modelos de ambos os contextos sem diferenças pendentes. Suíte da API: 359 testes aprovados; Angular: 33; E2E do lote administrativo: 29; portal: 2. Build Release da API e build/lint Angular verificados. SQL Server e Oracle não receberam migrations.

Não houve conexão a banco real, execução de SQL, integração Git Flow, push ou publicação. O startup da nova imagem aplicará as duas migrations automaticamente. O acesso externo do portal exige `PortalIdentity:Issuer`, `Audience`, `SigningKey` e `TokenMinutes` independentes da autoridade interna; sem configuração válida, o login retorna indisponibilidade. Conferência no ambiente permanece pendente.

### Referências

Veja [operação interna](../../../operacao-interna.md) e [comandos da API](../../../../ms-olivaw-api/README.md). A numeração organiza funcionalidades, sem criar issues adicionais. O feedback abaixo preserva o planejamento original e não redefine o estado atual.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 12 — Dashboard Meu negócio

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Apresentar ao cliente uma visão simples e autorizada dos sistemas que a Olivaw acompanha.

## Dependências

[Identidade do portal](11-identidade-do-portal.md), [inventário](../../discovery/painel-olivaw/04-inventario-de-recursos.md) e [saúde](../../discovery/painel-olivaw/05-coleta-e-saude.md). Consumo e atendimento entram somente quando suas fontes estiverem entregues.

## Escopo da entrega

- Título Olivaw - Meu negócio, empresa autorizada, sistema selecionado e acesso ao site/aplicativo.
- Estado em linguagem cotidiana, última verificação, cobertura e informação desatualizada quando necessário.
- Mostrar “Seu site está disponível. Verificado às [hora]” somente com evidência; “Estamos atualizando a verificação” quando não houver confirmação.
- Resumir mudanças e capacidade apenas com fontes autorizadas, sem expor custo interno.
- Interface clara, mini Olivaws pontuais, estados vazios úteis e navegação acessível. Solicitações e Meu plano só aparecem como destinos quando implementados.

## Fora desta entrega

- Stack traces, topologia privada, dados de outros clientes e diagnósticos ainda não confirmados.
- Gestão do negócio dentro do aplicativo do cliente; o portal não substitui sua administração.

## Critérios de aceite

- Cada cliente vê exclusivamente seus sistemas e dados úteis.
- Falha de fonte nunca é traduzida como tudo funcionando; sem histórico não há percentual inventado.
- Desktop, celular, teclado e movimento reduzido permitem consultar e navegar.
- Ações e links correspondem a funcionalidades reais, sem atalhos falsos para módulos futuros.

## Decisões antes de implementar

- Quais sinais podem ser traduzidos com segurança e quais ficam restritos à operação.
- Conteúdo mínimo da primeira tela e política de atualização/comunicação ao cliente.
```
