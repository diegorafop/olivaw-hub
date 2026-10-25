# Baley — Auditoria automática e relatórios pela API Olivaw

## user-history

Como responsável pela Olivaw,
quero organizar agentes Python padronizados e executar um analista de segurança com modelo e skill configuráveis,
para acompanhar a infraestrutura periodicamente e guardar relatórios evolutivos pela API administrativa, sem acesso direto do agente ao banco.

## acceptance-criteria

- Baley acessa os alvos somente pelo escopo autorizado da futura skill; análise não executa correções automaticamente. Listas de verificações e detalhes de coleta aguardam o próximo prompt.
- O agente nunca usa driver, DSN, conexão ou usuário de banco. Persistência, consulta e envio de relatórios passam exclusivamente pela API autenticada da Olivaw.
- Resultado é JSON válido, com envelope versionado e conteúdo flexível. Ausência de campos opcionais e novos campos internos são tolerados; uma falha de coleta não é convertida em evidência de segurança.
- API valida o envelope, preserva `RawData` como string e guarda metadados mínimos em entidade herdada de `Entity`, sem duplicar seus campos de identidade/auditoria.
- Reenvio da mesma execução não duplica relatório; repetição com conteúdo divergente retorna conflito. Conta técnica do Baley tem somente permissões específicas, sem root ou deployment.
- Agendamento evita sobreposição e controla duração, tamanho de saída, tentativas, uso de IA e retenção local; desligado até configuração e autorização de execução.
- Chaves privadas, senhas, cookies, tokens e dados não autorizados não entram no prompt do modelo, no JSON persistido ou nos logs. Chave administrativa existente não é copiada para a VM nem para um container.
- Testes cobrem configurações inválidas, indisponibilidade de SSH/modelo/API, JSON evolutivo, repetição, autorização, isolamento e ausência de acesso a banco pelo agente.
- Base do Baley, skill e lifecycle local aprovados e implementados; coleta automatizada, IA, API, tabelas e migrations continuam pendentes e não são declarados concluídos.

## ai-prompt

### Estado e autorização atual

Em discovery, a pedido explícito do mantenedor. Auditoria automática e persistência de relatórios aguardam planejamento e nova autorização de implementação. O mantenedor aprovou criar Baley em `olivaw-agent/ms-baley-py`, skill `safety-docker` em `olivaw-agent/skills`, cópia de execução do Codex e comandos de lifecycle/update. O agrupador deve operar todos os projetos reconhecidos da pasta. Esse recorte local foi implementado; coleta autônoma de SSH/IA, API administrativa, entidade RawData, migrations e agendamento real da auditoria continuam pendentes do contrato detalhado.

Responsável: Codex desta sessão, worktree `/home/makina/projects/olivaw`, branch recebida `feature/issue023-tests-and-adjusts`, sem nova issue criada. Preservar alterações preexistentes e conferir AGENTS antes de avançar. A criação anterior de documentação foi ampliada por autorização explícita, sem autorização implícita para implantar o agente no servidor ou iniciar chamadas de IA pagas.

### Escopo pendente registrado

Esta feature planeja o fluxo **Baley → API Olivaw → banco**: coleta autorizada por SSH, análise com skill/modelo definidos, agendamento sem sobreposição, produção de JSON e envio autenticado para uma rota administrativa. A API deverá validar e persistir uma entidade herdada de Entity, com RawData string e metadados estáveis, e permitir consulta administrativa dos relatórios.

O supervisor atual não coleta, não chama IA nem envia relatórios. Não existem a rota, a tabela e as migrations desta integração; rodar start/status não gera registros na base. O comando update continua somente consulta de source/discovery/learn e não faz parte da persistência das auditorias.

A base já entregue permanece em [lifecycle e skill](../done/baley-lifecycle-e-safety-docker.md) e [update somente leitura](../done/baley-update-fontes-e-aprendizado.md). Este documento guarda o planejamento restante, sem reabrir essas entregas nem autorizar alterações de código, banco ou ambiente agora.

### Recorte entregue em 24/10/2026

- Template recebeu `setting.json`, carregador e whitelist/testes do scaffold. Baley foi gerado por `python3 ms-limpo-py/scripts/new_agent.py baley --output olivaw-agent`; suporte SQL e demonstração removidos do derivado, preservados no template.
- `start`, `stop` e `status` locais com lock, parada cooperativa e detecção de estado antigo. Supervisor aguarda configuração, sem rede/IA ou coleta periódica; habilitar análise antecipadamente falha.
- `olivaw-agent/olivaw-agent` descobre projetos coerentes `ms-*-py` e delega operações sequenciais, com resultado por projeto. Usa venv própria quando disponível.
- `update` agora é estritamente somente leitura: consulta catálogo, pesquisa fontes candidatas e lista source/discovery/learn. Não grava propostas nem altera agente/skill. Operador decide manualmente o que incorporar.
- Fonte em `olivaw-agent/skills/safety-docker`, cópia instalada em `~/.agents/skills/safety-docker` e invocação `$safety-docker`. A grafia safet-docker no feedback foi tratada como referência à skill chamada safety-docker pelo próprio mantenedor, sem criar alias duplicado.
- Validação: 16 testes do template, dez do Baley, três do agrupador/instalador; ciclo real local start/status/stop; skill validada pelo quick_validate. Consulta pública de smoke test leu CISA/NVD, sem SSH ou IA; NVD estava truncado na primeira página, portanto não é busca completa de vulnerabilidades.

Recorte concluído documentado em [Baley e safety-docker](../done/baley-lifecycle-e-safety-docker.md).

### O que foi encontrado

- [Template Python](../../../ms-limpo-py/README.md): scaffold em `scripts/new_agent.py`, arquitetura `domain/core/features/data`, demonstração em memória e suporte opcional a PostgreSQL/migrations.
- Na leitura inicial faltava `setting.json`; agora foi incluído com carregador, identidade ajustada pelo scaffold e testes. O gerador mantém lista fechada de arquivos.
- O exemplar em `/home/makina/Downloads/ms-giskard-py-develop/ms-giskard-py` contém o esqueleto CQS. Conforme esclarecimento do mantenedor, a skill de code review está no PC de trabalho e seu conteúdo é irrelevante para esta feature. Não solicitar cópia, não procurá-la em outras máquinas e não condicionar Baley a ela.
- A persistência direta e os comandos PostgreSQL descritos no template/Giskard não são o padrão autorizado para Baley. O agente deve usar adaptador HTTP e não receber configuração ou dependência de banco. Não alterar o projeto externo do Giskard.
- A [auditoria anterior](../discovery/seguranca-e-infraestrutura.md) é referência de problemas e limites observados, não uma lista de verificações aprovada para o robô. A [infraestrutura implantada](../done/infraestrutura-compartilhada.md) tem VM pequena; o local de execução do agente ainda precisa ser decidido.

### Organização implementada

```text
olivaw-agent/
  AGENTS.md
  README.md
  olivaw-agent
  hub.py
  scripts/install_skill.py
  skills/safety-docker/
    SKILL.md
    agents/openai.yaml
    references/
  ms-baley-py/
    AGENTS.md
    setting.json
    pyproject.toml
    src/ms_baley/
    tests/
```

Cada projeto é independente. A skill canônica é compartilhada pelo agrupador, sem cópias concorrentes no repositório. A cópia instalada no Codex é sincronizada explicitamente, não é editada por update. Consulte [a operação do agrupador](../../../olivaw-agent/README.md) e [o projeto Baley](../../../olivaw-agent/ms-baley-py/README.md).

### Configuração `setting.json`

O setting real já existe no projeto, sem `example`; o modelo/skill usados no supervisor estão explícitos e a coleta desabilitada. O JSON abaixo continua proposta do contrato futuro de análise/API, não a configuração já executada:

```json
{
  "settingsVersion": 1,
  "agent": { "id": "baley", "version": "0.1.0" },
  "schedule": {
    "enabled": false,
    "intervalHours": 6,
    "timezone": "UTC",
    "misfirePolicy": "skip",
    "overlapPolicy": "skip"
  },
  "execution": {
    "mode": "report-only",
    "timeoutSeconds": 900,
    "maxOutputBytes": 1048576,
    "maxRetries": 3
  },
  "ai": {
    "provider": null,
    "model": null,
    "apiKeyEnv": "SAFETY_AI_API_KEY",
    "maxOutputTokens": 4000,
    "maxCallsPerRun": 1
  },
  "skill": {
    "path": "../skills/safety-docker/SKILL.md"
  },
  "ssh": {
    "targetRef": null,
    "privateKeyPathEnv": "SAFETY_SSH_KEY_PATH",
    "knownHostsPathEnv": "SAFETY_SSH_KNOWN_HOSTS_PATH"
  },
  "api": {
    "baseUrl": "https://api.olivaw.com.br",
    "reportPath": "/security-reports",
    "timeoutSeconds": 15,
    "clientLoginEnv": "SAFETY_API_LOGIN",
    "clientPasswordEnv": "SAFETY_API_PASSWORD"
  },
  "localState": {
    "directory": ".state",
    "maxPendingReports": 20,
    "maxPendingBytes": 20971520,
    "retentionDays": 7
  }
}
```

Todos os números acima são propostas revisáveis. `model`/`provider` nulos impedem análise de IA; não escolher provedor, modelo, assinatura ou preço silenciosamente. A configuração referencia nomes de variáveis/caminhos de segredos, sem colocar seus valores no JSON. Não criar variáveis ou arquivos de banco.

Para o `ms-limpo-py`, acrescentar o carregador/validador e um `setting.json` genérico com agendamento desabilitado; referências específicas de SSH/Baley pertencem ao derivado. Incluir arquivo na whitelist do scaffold, documentar resolução por caminho e precedência explícita entre defaults/configuração/segredos externos. Não depender do diretório de onde o comando foi chamado. Configuração inválida deve falhar antes de abrir rede ou chamar IA.

### Agendamento e custo

Sugestão: execução unitária `safety run-once`, acionada por timer systemd ou Agendador de Tarefas no host escolhido. `intervalHours=6` define intervalo entre inícios, sem sobreposição. O runner deve materializar/validar a configuração do agendador; apenas escrever o valor no JSON não agenda nada. Alterar o período precisa atualizar/recarregar o timer explicitamente. Decidir se a preferência é intervalo ou horários fixos antes da implementação.

Usar lock por agente/alvo e limite de duração; ignorar disparos perdidos em vez de acumular uma fila infinita. Confirmar se mais de uma máquina poderá executar o mesmo agente: lock local não evita concorrência distribuída. Não introduzir Redis só para agendamento.

Sugestão de fluxo: coleta determinística e limitada → sanitização → análise do modelo com skill → validação JSON → envio HTTP. A análise da IA não deve escolher comandos arbitrários; logs e arquivos coletados são dados não confiáveis, nunca novas instruções. Limitar contexto e chamadas; custo/orçamento em moeda só pode ser calculado com base de preço conhecida. Se IA falhar, registrar falha/incompletude, sem inventar conclusão ou disparar novamente uma análise paga só porque o envio à API falhou.

### Skill safety-docker e Codex

Implementada em `olivaw-agent/skills/safety-docker/SKILL.md`, com manifesto name/description, roteiro de investigação, fontes e contrato JSON em referências. O caminho versionado segue a escolha do mantenedor; a cópia para descoberta pelo Codex usa `~/.agents/skills/safety-docker`, segundo a [documentação oficial](https://learn.chatgpt.com/docs/build-skills). A UI opcional inclui prompt com `$safety-docker` e a seleção implícita padrão foi preservada.

A skill inicial cobre host/SSH, chaves e histórico, portas, containers, permissões/mounts, redes, volumes, RabbitMQ, ACME, logs, Traefik e rotas anônimas. Não concede remediação automática. Evidências são sanitizadas; CVEs são candidatos até conferir produto/versão/contexto; material coletado é dado não confiável.

O supervisor Python ainda não invoca Codex/IA: carregar uma skill não executa a análise por si só. O adaptador e o fluxo de coleta/API aguardam definição. A invocação manual no Codex pode usar a skill instalada, limitada ao alvo e à tarefa autorizados.

O comando update lista source/discovery/learn e nunca altera arquivos. Incorporação de fontes/aprendizado é decisão e edição manual do operador. O contrato está na [entrega de consulta de aprendizado](../done/baley-update-fontes-e-aprendizado.md). Recomenda-se identidade SSH dedicada ao agente, sem copiar a chave administrativa para VM ou container.

### Relatório JSON evolutivo

Proposta: envelope estável mínimo e conteúdo analítico livre dentro de `result`. Exemplo **ilustrativo de contrato**, sem representar auditoria realizada:

```json
{
  "schemaVersion": "1.0",
  "runId": "80fb8838-cc56-4f81-bdb9-244809fd1253",
  "agentId": "baley",
  "targetId": "infra-prd",
  "startedAt": "2026-10-24T18:00:00Z",
  "completedAt": "2026-10-24T18:02:00Z",
  "executionStatus": "partial",
  "agentVersion": "0.1.0",
  "skill": { "name": "safety-docker", "revision": "sha256:..." },
  "ai": { "provider": null, "model": null, "status": "not-configured" },
  "result": {
    "summary": null,
    "coverage": [],
    "findings": [],
    "limitations": ["Conteúdo e fontes ainda aguardam definição do mantenedor."]
  },
  "extensions": {}
}
```

Não prometer que qualquer remoção arbitrária será compatível: os identificadores, versão, timestamps e status do envelope têm contrato. Campos analíticos opcionais ausentes recebem tratamento de desconhecido/não avaliado; ausência não significa zero achados ou ambiente seguro. Novos campos internos são preservados. Mudanças incompatíveis do envelope exigem nova versão maior e estratégia explícita de leitura; evitar migration de tabela para cada novo atributo de análise.

Recomendação: diferenciar `executionStatus` (`completed`, `partial`, `failed`) de gravidade dos achados. `failed` não significa que o servidor está vulnerável, e `completed` não significa que está seguro. Skill/modelo/revisão e instante de coleta permitem explicar mudanças entre rodadas. Detalhes do finding, identificador de regra, evidência, confiança e prioridades serão definidos depois.

Produzir JSON estruturado e validar antes de enviar; não depender de extrair JSON de texto Markdown livre. Rejeitar JSON inválido ou com raízes/tipos proibidos, versões maiores não suportadas, chaves duplicadas, timestamps incoerentes, tamanho/profundidade excessivos e dados sensíveis. Não executar nem renderizar HTML/scripts vindos de RawData.

### Persistência administrativa proposta

Rota sugerida, a confirmar: `POST /security-reports` na `ms-olivaw-api`. O agente chama a API, e só a API persiste. Consultas paginadas `GET /security-reports` e `GET /security-reports/{externalId}` ficam restritas à operação, sem exposição pública ou ao portal do cliente.

Entidade proposta `SecurityReport`, em `Owh.Olivaw/Domain`, herdando de `Entity`; tabela `securityreport`, minúscula. Não duplicar `Id`, `ExternalId`, `ActorUserId`, `CreatedAt`, `UpdatedAt` ou `DeletedAt`. `CreatedAt` representa recebimento na API; a coleta usa seus próprios timestamps UTC.

| Campo adicional | Propósito |
| --- | --- |
| `RunId` (Guid) | Identidade imutável da rodada e reenvio idempotente. |
| `AgentId` (string limitada) | Identidade lógica, validada contra a conta técnica autenticada. |
| `TargetId` (string limitada) | Alvo autorizado, sem aceitar qualquer alvo só porque veio no body. |
| `SchemaVersion` (string limitada) | Contrato do JSON aceito, sem coluna para cada detalhe analítico. |
| `ExecutionStatus` (string limitada) | Execução concluída/parcial/falhou, separado de risco. |
| `StartedAt`, `CompletedAt` (UTC) | Janela da coleta; validar ordem e limite de relógio. |
| `ContentHash` (string limitada) | Hash calculado pela API para detectar reenvio alterado. |
| `RawData` (string) | JSON completo e sanitizado do relatório, inclusive campos desconhecidos permitidos. |

Preferir inicialmente somente essas colunas estáveis. `Summary`, gravidade máxima e contagens podem ficar em RawData até o contrato de findings estar definido; materializar depois apenas o que justificar filtro/índice. Não confiar em contagem/gravidade enviada para conceder privilégios ou encerrar incidente automaticamente.

Proposta de request: `{ "rawData": "<JSON serializado>" }`. A API lê o envelope uma vez, valida, extrai colunas e calcula hash sem precisar de um DTO rígido para todo `result`. Não aceitar metadados de coluna contraditórios em campos paralelos. Normalização genérica do pipeline não deve alterar conteúdo do relatório; preservar entrada e aplicar validadores específicos. Validar antes de salvar e armazenar o corpo sanitizado efetivamente aceito; não persistir um original contendo segredos só para chamá-lo de raw.

Propor unicidade por identidade autenticada do agente + RunId. Mesmo id e mesmo conteúdo aceito retornam o relatório existente; mesmo id com conteúdo diferente retorna 409. Calcular hash de conteúdo canônico com regras documentadas, sem descartar campos desconhecidos; rejeitar duplicatas de chave antes de canonicalizar. Em reenvio incerto, reutilizar os mesmos bytes/RunId, sem gerar nova rodada. Garantir esse comportamento por constraint e tratamento de concorrência, não apenas consulta prévia.

Policies sugeridas `security-reports.write` e `security-reports.read`; cliente técnico não root, sem `devops.deploy`, `identity.manage`, conta de cliente ou acesso direto ao banco. Actor vem do JWT, nunca do body. Definir o cadastro confiável que vincula a identidade técnica aos agentIds/targets permitidos. Não usar a conta root de deployment atual para o Baley.

Consultas comuns usam `IRepository<SecurityReport>`; repositório específico só se projeção/filtro/paginação requererem operações ausentes no contrato genérico. Preservar convenções e histórico de migrations do serviço. `RawData` continua propriedade string, com mapping de texto adequado a cada provedor; não presumir jsonb obrigatório ou conversão para objeto fortemente tipado no EF.

### Envio, falhas e armazenamento local

Enviar somente a origem HTTPS configurada; timeout, tentativas limitadas com espera progressiva, sem seguir redirecionamento que vaze token. Autenticação pela autoridade interna e renovação de sessão sem repetir chamadas infinitamente. Tratamento explícito para 401/403, 409, 413, 429 e erros temporários.

Sugestão: API é fonte definitiva; spool local de JSON sanitizado permite reenviar após indisponibilidade. Arquivos privados em `.state/`, fora do Git, com prazo e teto de espaço. Não instalar SQLite ou PostgreSQL como fila oculta. Se o mantenedor preferir zero arquivo local, registrar a consequência: falha prolongada da API pode perder um relatório já produzido. Definir confirmação de recebimento, limpeza, falhas de disco e o que fazer quando o spool lotar; nunca crescer indefinidamente.

Para SSH e API indisponíveis, distinguir o diagnóstico do alvo da falha do próprio monitor. Não publicar diretamente da VM monitorada usando credenciais de banco. Não registrar chave, bearer, cabeçalhos de autenticação, corpos brutos de login ou saídas de comandos com valores secretos. Coleta, logs, spool e tamanho de RawData precisam de limites independentes.

### Etapas restantes da feature ampla

1. Fechar escopo de coleta, alvos, execução, modelo/provedor e política de retenção com o próximo prompt.
2. Base/scaffold/lifecycle/skill concluídos; evoluir o contrato de análise sem recriar ou sobrescrever o projeto gerado.
3. Implementar modo API-only, runner, skill própria, coleta autorizada, sanitização e contrato versionado, sem copiar código de review ou conexão direta do Giskard.
4. Implementar contrato cliente, CQS, policies, entidade, mappings e testes na API. Migration ao final, somente para provedor solicitado e com nome de issue/branch confirmado, preservando todas as migrations publicadas. A intenção de gerar foi registrada, mas **não gerar nesta fase documental**.
5. Validar configuração/scaffold, filtros de segredos, limite de comandos/saída, idempotência concorrente, JSON flexível e autorização. Testes simulam SSH/modelo/API, sem chamadas pagas ou bancos de clientes.
6. Teste integrado controlado e revisão da skill; publicar/habilitar timer somente após autorização específica. Comprovar que a conta do agente não acessa banco nem faz remediação.

### Decisões para a próxima rodada

- Quais alvos, fontes, comandos, permissões e itens entram na análise? O mantenedor já informou que detalhará depois; não antecipar coleta.
- Onde o agente executa: máquina local, PC de trabalho ou runner separado? Considerar acesso SSH e a restrição de memória da VM.
- Qual modelo/provedor e como autenticar sua execução? Codex CLI e chamada direta de API são opções diferentes; selecionar uma antes de implementar adaptador/agendamento.
- Skill e diretórios já definidos: safety-docker em olivaw-agent/skills; não precisa do conteúdo da skill de code review.
- Cadência de seis horas, horário fixo, fuso, lock e comportamento quando a máquina estiver desligada.
- Contrato definitivo dos achados, permissões da API, alvos autorizados, janela de retenção, teto de RawData e necessidade de spool local.
- Provedor de banco e issue para a migration da API; sem banco/migrations do agente.

Não são perguntas que bloqueiam esta especificação; são decisões registradas para a leitura e o próximo prompt. Não acrescentar esta feature ao índice principal de concluídas enquanto estiver em discovery.

## user-feedback

```text
criaremos tbm um novo projeto aqui dentro do hub da olivaw. ms-agent-py, nessa pasta teremos diversos agents de IA escritos em python o qual voce vai usar o scaffold/schematics que temos em ms-limpo-py para gerar o agent no formato padrao de projeto de agents que ficam dentro de ms-agent-py, nosso primeiro agent será um analista de segurança, ms-safety-py . esse cara vai ter uma skill que nós mesmos vamos criar no padrao que voce codex orienta em colocar as skills personalizadas. essa skill nós a evoluiremos para que logue no servidor como fizemos agora e faça uma analise de segurança.

o que será analisado e onde vamos guardar as informacoes voce aguarda o proximo prompt, mas será basicamente acessar alguma rota administrativa do api.olivaw para enviar esses reports. 

o robo 'safety' terá seu arquivo de setting.json (ja ajuste isso no ms-limpo-py tbm caso ele n tenha esse setting) nesse setting teremos configuracoes do robo como periodo de execucao, por exemplo a cada 6 horas, etc. o robo terá um modelo de IA e uma skill para uso.

n terá acesso direto do banco ok, aqui faremos por API sempre...
nessa pasta tem um exemplo de agent /home/makina/Downloads/ms-giskard-py-develop/ms-giskard-py

ele faz code review usando uma skill de code review faremos o nosso analogo mas sem acesso a base blz?

o resultado da analise deve gerar um JSON resultante com todas as infos, se a gente for removendo coisas ou adicionando o json n quebra.
ai voce pode criar em ms-olivaw-api uma rota que receberá algumas informacoes que voce achar ideal para 'guardar' essas informacoes, uma tabela herdada de Entity que vai gerar migrations com entity ja teriamos createdAt, etc etc. voce colocara as colunas que achar necessario para essa feature, vai versionar a feature nos nossos docs e ja vai implementar tudo ok. UM dos campos dessa tabala vai ser rawdata onde terá o JSON com rsultado, porque se a gente for mudando os dados que vem nao precisamos ficar alterando tabelas do banco, e sera no formato string blz, os dados que voce achar interessante que sejam colunas a parte pode criar blz.

bom começa escrevendo a feature coloca na pasta discovery ai espera eu ler para a gente avançar.
se tiver outras ideias consideracoes ou ainda formas melhores de fazer me avise nesse arquivo dessa feature

a skill fica somente no PC do meu trabalho, ter o conteudo da skill code review agora é irrelevante
```


```text
pode criar nossa skill chamada safety-docker e o nome do nosso agent será ms-baley-py pode criar o projeto tbm.
guarde a skill com as diretivas mais modernas e recomendadas pelo codex e tals.
vamos versionar dentro de olivaw-agent/skills e o codigo do agent fica em olivaw-agent/ms-baley-py, o nosso querido agent baley será o responsável pela segurança.

eu queria que ele tivesse, além das funcionalidades padrao de start/stop/status uma quarta operação 'update' a qual vamos definir melhor depois, mas é uma operação que acessa a internet com famosas e confiaveis que registram falhas de segurança e vai 'listar' as novidades e melhorar a própria skill, a nossa skill versionada fica no projeto mas a copia dela para execucao pelo codex com tipo '$safet-docker' voce coloca no local adequado do codex blz?

pode iniciar a skill com diretivas padrao de investigacao que voce ja fez hj no nosso srvidor docker, depois vamos evoluindo ela blz?

dentro da pasta olivaw-agent teremos tbm um script tipo olivaw-agent start, nesse caso essa funcionalidade vai executar todos os ms-agentName-py que tivermos na pasta versionados, dentro do projeto de agents blzzz
```


```text
isso, o update em SI não muda nada no baley nem na skill. Ele apenas lista essa informacoes e o operador (que no caso sou eu) vou definir quais usar na skill
```


```text
boa blz, vou rodar o agent e verificar o status.
a saida dele seria salvar dados no olivaw né?

depois eu confiro na base os reports do nosso agent blz.

otiom, entao deixa isso como feature em discovery
```
