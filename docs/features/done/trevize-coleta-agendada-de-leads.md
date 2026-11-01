# Trevize — Coleta agendada e qualificação de Leads

## user-history

Como operador, quero iniciar manualmente um agente que encontre empresas de eventos em Curitiba e prepare fichas e abordagens, para revisar oportunidades sem contatos automáticos.

## acceptance-criteria

- Projeto `olivaw-agent/ms-trevize-py` gerado por `ms-limpo-py`, com pacote próprio, CQS, configuração e testes, sem dependência de banco.
- `setting.json` com Curitiba, eventos, até 20 candidatos novos por ciclo e intervalo de 24 horas após término do ciclo anterior.
- Primeiro ciclo ao start, sem sobreposição ou retomada no boot. Status e importação não iniciam processo. Desligamento do PC encerra o worker; novo start é necessário.
- Busca e rolagem limitada no Maps, filtragem e identidade determinística; sem contornar bloqueios ou garantir cobertura exaustiva.
- Leitura passiva e limitada de sites, com proteção contra destinos privados e respeito a robots; resultados inconclusivos não viram falhas confirmadas.
- Qualificação por Codex CLI sem ferramentas, com skill versionada e saída estruturada; rascunho e confiança pendente para revisão humana.
- API com conta técnica sem root, confirmação de entrega, revisão e proteção do trabalho humano já existente. Nenhuma chamada direta a banco.
- Pré-verificação bloqueia coleta e IA se a API de qualificação não estiver disponível. Erro de entrega interrompe lote sem criar uma sequência de registros incompletos.
- Estado local privado e limitado; status JSON indentado. Sem envio de mensagens, chamadas ou formulários.

## ai-prompt

Recorte implementado pelo Codex na branch `feature/issue027-agent-prospector`, issue 027 inferida do branch, worktree raiz. Sem migration, commit/push pelo agente ou deploy. O mantenedor pode ter registrado commits durante o trabalho; não reescrever o histórico.

### Operação

- [Projeto e comandos](../../../olivaw-agent/ms-trevize-py/README.md).
- [Configuração](../../../olivaw-agent/ms-trevize-py/setting.json).
- [Skill](../../../olivaw-agent/skills/trevize-prospect/SKILL.md), criada com orientação de skill-creator e validada; consumida do caminho versionado no prompt do agente, sem instalar ou sobrescrever skills pessoais do Codex.
- [Ficha de Leads](leads-ficha-qualificacao-e-abordagem.md).

`olivaw-agent start --agent trevize` inicia somente Trevize; `olivaw-agent start` inclui todos os projetos válidos. Nenhum start real do Trevize foi feito nesta entrega. `collect` é um comando manual de ciclo único; audit/update do agrupador não disparam prospecção. Stop é cooperativo e pode aguardar operação limitada em andamento.

Conta técnica `agent-trevize` configurada via API em 01/11/2026, com `leads.manage` somente, sem root. Credenciais estão apenas em `_secrets/admin.json`, seção trevize, arquivo 0600 fora do Git. Segredos nunca entram na skill, relatório ou prompt. A permissão já existente no código foi cadastrada no catálogo da API para esse perfil.

A API publicada ainda não tinha `PUT /leads/{id}/qualification` na pré-verificação de 01/11/2026. Publicar o backend atualizado antes de start. O ambiente virtual e Chromium foram preparados, sem alterar pacotes globais do sistema. Codex deve estar autenticado localmente; modelo null usa o padrão do CLI, sem afirmar modelo/preço específico.

### Lifecycle

Baley e Trevize declaram/validam `lifecycle.autoStart=false`. Não foram instalados cron, systemd ou inicialização de shell. Arquivos de shell e diretórios de inicialização do usuário não apresentaram referência de start; consultas de systemd/crontab não estavam disponíveis nesse ambiente. Não afirmar auditoria global de todos os serviços do PC.

Consulta de status observou Baley ativo em scheduled, sessão anterior preservada, e Trevize parado. Supervisor contínuo do Baley não significa auditoria contínua: auditorias a cada seis horas e consulta de aprovações em intervalo próprio. Não reiniciar o Baley apenas por mudar documentação/configuração.

### Limites e evoluções

Até 20 é meta e limite por ciclo, não promessa de 20 entregas: fonte, duplicidade, tempo e erros podem reduzir o lote. Repetições usam UUID determinístico; qualificação já existente não é sobrescrita. Nome igual em outro Lead exige revisão, sem fusão automática. Um índice local limitado evita revisitar os mesmos candidatos; não há garantia de novos resultados após esgotar a busca.

Website é uma leitura HTML, sem medição de velocidade ou teste visual. WhatsApp só é preenchido a partir de link encontrado; telefone do Maps permanece telefone. Formulário só é indicado se detectado na página lida, sem submissão. Região configurada é recorte de busca, não endereço comprovado.

A POC anterior validou navegação real; esta implementação foi validada com fixtures/mocks e Chromium em HTML local com rede bloqueada, coletando 20 identidades únicas após rolagem. Não executou lote real, IA paga ou envio de propostas. Limites de tempo, URLs, contratos e falhas são verificáveis; qualidade comercial depende da revisão dos primeiros 20.

Fontes alternativas, análise aprofundada, métricas de revisão, evidências por campo e licenças permanecem na [discovery](../discovery/trevize-prospeccao-e-qualificacao-de-leads.md). Regras de extração e armazenamento da fonte devem ser avaliadas para uso recorrente; acesso técnico ao Maps não significa licença irrestrita. Não contornar CAPTCHA ou bloqueios.

### Validação

18 testes Trevize de configuração, ciclo, preflight, rede, IA, entrega e lifecycle; 34 testes Baley e 3 do agrupador; smoke Chromium local com rede bloqueada e validação da skill. Comandos reproduzíveis no README. Testes encerram apenas seus workers temporários. Nenhum serviço da AWS reiniciado.

## user-feedback

```text
após terminar isso, ja pode criar noss agent de IA que vai coletar os leads no maps. regiao curitiba e, 20 unidades coletadas a cada 24 horas. empresas de eventos.

detalhe o robo precisa rodar somente quando faço 'start' neles, confere isso tbm parece que o baley ficou rodando diretaooo.
n tem que ser 'auto start' qdo ligo o PC blz?

somente quanto faço start, se desligar o PC ele 'morre'

ms-trevize-py
```
