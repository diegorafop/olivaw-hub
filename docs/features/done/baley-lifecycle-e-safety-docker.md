# Baley — Projeto Python, lifecycle e skill safety-docker

## user-history

Como responsável pela Olivaw,
quero criar e operar o agente Baley junto dos demais agentes do repositório,
para organizar a segurança com skill própria, lifecycle local e atualização revisável de referências.

## acceptance-criteria

- Projeto `olivaw-agent/ms-baley-py` gerado pelo scaffold de `ms-limpo-py`, com setting real e sem acesso direto ao banco.
- Skill `safety-docker` versionada em `olivaw-agent/skills` e cópia instalada no escopo do usuário do Codex.
- Start/stop/status locais funcionam sem duplicar instâncias, confiar em PID antigo ou iniciar uma auditoria não configurada.
- Agrupador executa as operações para todos os projetos reconhecidos `ms-*-py`, com venv própria quando disponível e resultado por agente.
- Update consulta fontes primárias com limites e propõe evolução sem alterar a fonte ou instalação ativa da skill.
- Falhas, paginação incompleta e ausência de achados não são transformadas em declaração de segurança.

## ai-prompt

### Entrega

Concluída e validada localmente em 24/10/2026. Responsável Codex, worktree atual na branch recebida `feature/issue023-tests-and-adjusts`, sem nova issue criada. Sem commit/push, deployment de agente, SSH ou chamada paga de IA.

Fonte e instruções em [olivaw-agent](../../../olivaw-agent/README.md), projeto [Baley](../../../olivaw-agent/ms-baley-py/README.md) e skill [safety-docker](../../../olivaw-agent/skills/safety-docker/SKILL.md). Instalada em `/home/makina/.agents/skills/safety-docker`, com conteúdo conferido contra a fonte. Invocação no Codex: `$safety-docker`; se não aparecer na sessão atual, reiniciar o Codex.

### Evolução posterior do update

O comportamento de propostas em disco descrito nesta entrega inicial foi substituído por [consulta somente leitura com source/discovery/learn](baley-update-fontes-e-aprendizado.md). O operador decide o que incorporar; update não altera arquivo algum. As seções abaixo preservam o registro da primeira implementação.

### Implementação

- Template recebeu `setting.json`, carregador/validador e ajustes explícitos da whitelist. Baley gerado por `python3 ms-limpo-py/scripts/new_agent.py baley --output olivaw-agent`.
- Derivado sem drivers/DSN/migrations PostgreSQL ou feature demonstrativa. CQS e composição preservados para lifecycle e atualização de skill.
- Supervisor Linux/POSIX com lock, start idempotente e stop cooperativo por identidade da instância. Estado privado e heartbeat limitado; não há log crescente. Não matar PID lido de arquivo.
- `./olivaw-agent/olivaw-agent start|stop|status|update` descobre projetos coerentes, rejeita links/identidades incorretas e agrega resultados. Projeto novo no checkout não precisa estar commitado para testes locais. Outros agentes precisam implementar as operações de runtime antes de serem operados pelo agrupador.
- Skill com instruções concisas e referências por necessidade, frontmatter name/description e UI opcional. Mantém investigação somente leitura por padrão e autorizações específicas para remediação. Não inclui segredos ou dados reais do servidor.
- Update consulta JSON de CISA KEV e NVD com HTTPS, redirects bloqueados, 15 segundos por fonte, 8 MiB por resposta e janela inicial de sete dias. Até 100 candidatos, uma página de até 100 registros NVD e indicação explícita de truncamento; não é catálogo completo.
- Proposta inclui cópia candidata da skill, referência JSON e hashes, em estado ignorado. Até cinco propostas de 1 MiB e quota local de 20 MiB; revisão antes de promover/reinstalar. Limpeza por idade durante update com candidatos, não um serviço de retenção separado. Conteúdo externo não é inserido como instrução na skill.

### Evidências

- 16 testes do template, dez do Baley e três do agrupador/instalador aprovados: 29 no total. Incluem independência do scaffold, configuração inválida, start simultâneo, stop seguro, erro/truncamento de fontes, preservação da skill ativa e substituição explícita da cópia instalada.
- Smoke local real do agrupador: start → status aguardando configuração → stop → status parado. Nenhum worker de teste foi deixado executando.
- Skill validada pelo `quick_validate.py` do skill-creator; cópia de usuário instalada pelo script versionado, sem symlink.
- Consulta pública real: CISA retornou 1.756 registros; NVD retornou 100, com paginação truncada. Na janela/seleção inicial não houve candidatos, e o resultado final foi `partial`, código não zero, sem proposta ou alteração fictícia da skill.
- JSON de settings e links locais verificados. Cache `__pycache__` e estado local ignorados pelo Git.

### Limites e próximos recortes

O supervisor está em `waiting-configuration`, com `analysisEnabled=false`. Ainda não há coleta periódica de SSH, invocação automática de IA ou envio de relatórios à API. O intervalo de seis horas é configuração preparada; não afirmar que a VM está sendo auditada a cada seis horas. Não há modelo escolhido ou credencial inventada. Lifecycle Windows não validado; wheel fora do checkout exige `--settings` explícito.

API administrativa, entidade `SecurityReport`/RawData, autorização de targets e migrations permanecem na [feature principal em andamento](../in-progress/agentes-python-baley-e-relatorios.md). Atualização semântica e política de promoção da skill serão definidas com o mantenedor; não há autoedição ou remediação baseada em CVEs.

## user-feedback

```text
pode criar nossa skill chamada safety-docker e o nome do nosso agent será ms-baley-py pode criar o projeto tbm.
guarde a skill com as diretivas mais modernas e recomendadas pelo codex e tals.
vamos versionar dentro de olivaw-agent/skills e o codigo do agent fica em olivaw-agent/ms-baley-py, o nosso querido agent baley será o responsável pela segurança.

eu queria que ele tivesse, além das funcionalidades padrao de start/stop/status uma quarta operação 'update' a qual vamos definir melhor depois, mas é uma operação que acessa a internet com famosas e confiaveis que registram falhas de segurança e vai 'listar' as novidades e melhorar a própria skill, a nossa skill versionada fica no projeto mas a copia dela para execucao pelo codex com tipo '$safet-docker' voce coloca no local adequado do codex blz?

pode iniciar a skill com diretivas padrao de investigacao que voce ja fez hj no nosso srvidor docker, depois vamos evoluindo ela blz?

dentro da pasta olivaw-agent teremos tbm um script tipo olivaw-agent start, nesse caso essa funcionalidade vai executar todos os ms-agentName-py que tivermos na pasta versionados, dentro do projeto de agents blzzz
```
