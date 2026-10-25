# Baley — Correções aprovadas e saúde da infraestrutura

## user-history

Como operador do hub,
quero autorizar correções específicas ou todas as verificações de um relatório,
para o Baley corrigir apenas o que consegue executar e validar com segurança.

## acceptance-criteria

- Evidência, Leitura do Baley e Cobertura e limitações ficam visíveis, sem clique para expandir; apenas JSON técnico pode ficar recolhido.
- Estados: Aguardando revisão, Autorizado, Corrigindo, Validando infraestrutura, Resolvido e Requer intervenção.
- Aprovações individuais e em lote exigem root, confirmação e vínculo imutável a relatório/achado/plano. Lote considera todo o relatório, preserva estados existentes e registra intervenção para planos não suportados.
- Conta técnica só consome tarefas próprias e do host configurado; plano e reserva expiram e há uma execução exclusiva por host.
- Procedimentos iniciais restringem permissões do diretório SSH, authorized_keys e socket Docker, com metadados prévios e rollback delimitado. Não executar comandos inventados pela IA.
- Qualquer container parado, pausado, reiniciando, unhealthy, starting, serviço obrigatório ausente ou probe indisponível bloqueia manutenção.
- Resolvido exige duas amostras de saúde, mesmos containers/instantes de início/reinícios e confirmação de ausência do achado; falha implica intervenção, inclusive após rollback.
- Containers parados e de rollback nunca são removidos ou iniciados para liberar o gate. Limpeza de cache sem procedimento catalogado requer intervenção; skill orienta plano separado e não autoriza system prune genérico.

## ai-prompt

Responsável Codex, issue #25, branch `feature/issue025-baley-agent`, worktree do monorepo. Mudança em API, contratos, mapeamento, agente e frontend. Aprovação persistida separadamente de RawData imutável; não converter texto de relatório/IA em comando.

API em implementação: `securityremediation : Entity`, root lê planos e autoriza; conta técnica com `security.remediations.execute` consome fila e informa estados com reserva própria. Índice exclusivo da reserva ativa por host e revisão EF protegem contra concorrência. Jobs interrompidos nunca repetem mutação automaticamente: recibos privados são reenviados, ou exigem revisão quando a execução não pode ser reconstruída com segurança.

O supervisor consulta aprovações a cada 30 segundos sem alterar o intervalo de seis horas das auditorias e sem execução concorrente. O catálogo inicial é limitado a permissões; mudanças de firewall/SSHD/imagens/redes/serviços/cache continuam sem procedimento automático e são encaminhadas à intervenção. O futuro agente de disponibilidade não está implementado por esta entrega.

Não aplicar qualquer correção em produção durante testes. Testes usam API simulada, doubles e arquivos temporários; verificar bloqueio de saúde, pré-condições, rollback, comunicação perdida, idempotência, autorizações e lote sob filtro. A skill safety-docker e sua cópia instalada devem refletir os gates reais.

Migration PostgreSQL nova sugerida: `Issue025Remediation`, preservando `Issue025` publicada. Geração aguardando confirmação explícita do mantenedor; modelo novo não pode ser publicado sem essa migration. Deploy de API/frontend permanece sob responsabilidade do mantenedor. Não marcar esta entrega como publicada ou concluída enquanto faltarem os checks e a migration.

## user-feedback

```text
faremos mais 1 melhoria no baley, porque aquel 'Evidencias" com clique? essa informacao pode vir direto no card talvez ao lado direito n sei pensa ai em algo. Mas 1 clique para mostrar isso ficou ruim.

tbm crie um status para os cards, teremos categorias como  em analise, aprovado, em execucao, corrigido , se o baley n conseguir corrigir com segurança ele marca o status para algo como nao foi possivel corrigir algo assim. ENFIM pense em caterorias status com nomes melhroes e mais intuitivos para a UI.
qualqeur falha de segurança que o Baley corrigir ele deve analisar a saude dos containers e ver se esta ok a nossa infraestrutura antes de marcar como corrigido/sucesso etc.

esses nmes são bem ruins voce vai encontrar melhores, a ideia é que tenha uma acao nesse card onde o operador vai marcar 'aprovado' e então o baley, além da operação de investigar por falhas, vai tentar efetuar a correcao sempre bem safe sem comprometer a infra blz.

isso, melhora a skill dele ai tbm é claro

outra coisa qeu a skill do balley pode fazer é um docker system prune para e limpez de caches do servidor deixando tudo organizado.

isso, caso de cnotainer parado obviamente ele n vai remover né... Depois criaremos um outro agent especialista em manter a infra de pé. SE os containers n estiverem todos de pé bonitihos o baley n faz a limpeza de segurança

box de leitura e analise do baley ficou legal mas n precisa desse negocio de expandir nem a parte de cobertura de limitacoes.

na interface de Detalhes de auditoria, coloque tbm um button que faça a operação de aprovação para todos os itens
```
