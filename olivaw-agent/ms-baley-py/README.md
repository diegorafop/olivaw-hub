# Baley — Segurança Docker

Agente Python do hub Olivaw. Executa coleta SSH somente leitura, regras de análise, interpretação por Codex e envio pela API; nunca acessa banco ou aplica correções automaticamente.

## Comandos

Da raiz do monorepo:

```bash
./olivaw-agent/olivaw-agent status
./olivaw-agent/olivaw-agent start --agent baley
./olivaw-agent/olivaw-agent stop --agent baley
./olivaw-agent/olivaw-agent audit --agent baley
./olivaw-agent/olivaw-agent update --agent baley --format text
```

`start` inicia um supervisor local e executa o primeiro ciclo imediatamente. O timer espera `schedule.intervalHours` após terminar o ciclo anterior; padrão seis horas, configurável para três. Sem sobreposição entre worker e `audit`. `stop` é cooperativo: a coleta/interpretação em andamento pode levar até seus timeouts para terminar. Nenhum PID arbitrário é encerrado. O agendamento depende desta máquina permanecer ligada e do supervisor ativo; não instala serviço de boot automaticamente.

`status` distingue `auditing`, `scheduled`, `waiting-configuration` e `stopped`, mostrando última execução, próxima execução, entrega e tamanho da fila em quantidade. `update` continua somente leitura e não altera agente, skill ou resultados.

## Configuração

O arquivo real é `setting.json`. Caminhos são relativos a esta pasta; a chave SSH permanece em `../../_secrets/olivaw-admin-2026.pem`, somente local e modo 0600. SSH usa host verificado, sem senha nem execução de comandos de configuração. O host atual é `98.95.250.122`.

- `schedule.enabled` e `schedule.intervalHours`: repetição e intervalo do supervisor.
- `execution.enabled`: habilita auditoria; nunca habilita remediação.
- `audit.logWindowHours`: janela de leitura de logs entre uma e 24 horas, independente do intervalo; manter pelo menos o intervalo escolhido para reduzir lacunas. Há limite de linhas, então janela não garante cobertura integral.
- `ai.enabled`, `ai.provider=codex-cli`, `ai.model` e `ai.timeoutSeconds`: usa Codex instalado e autenticado nesta máquina. Modelo nulo preserva o padrão do CLI; consumo e limites pertencem à conta local. Sem shell, apps, plugins, web ou configuração pessoal de ferramentas; JSON estruturado e timeout. Falha de IA preserva a análise determinística e é registrada.
- `reportApi.baseUrl`: destino HTTPS fixo `https://api.olivaw.com.br`; arquivo privado `../../_secrets/admin.json`, seção `baley` com `login`/`password`. JWT somente em memória. Não usar root no runtime.
- `localState`: fila `.state/outbox`, máximo 20 MiB e sete dias, arquivos 0600 e diretórios 0700. Limites por tamanho e idade; descarte aparece em `droppedCount`. Relatórios entregues são removidos da fila local. A retenção do banco é independente e não apaga relatórios automaticamente.

## Contrato e cobertura

`POST /security-reports` exige `security.reports.write`. `GET /security-reports` e `GET /security-reports/{externalId}` exigem root. O envelope JSON 1.0 contém GUID `runId`, `agentId`, `hostId`, timestamps UTC, status e `result` flexível. A API armazena `RawData` como string mais metadados estáveis; campos novos no resultado são preservados. Reenvios reutilizam o GUID e conteúdo originais; confirmação da API precede exclusão local. Até quatro entregas por ciclo, com fila persistente limitada; falhas 401/403/429/5xx não viram sucesso.

Coleta até 40 containers, com campos selecionados de inspect, sem `Env`, labels ou argumentos. Consulta pressão de recursos, listeners, política SSH, SELinux, privilégios/capabilities, publicação de portas sensíveis, socket, reinícios e rotação Docker. Access logs Traefik e journal SSH são limitados a 1.200 linhas por janela. Contabiliza códigos HTTP, padrões de scanner, IPs observados e falhas de autenticação sem guardar URLs arbitrárias, query strings, headers ou linhas brutas. IP não identifica autoria. Sem logs/coletores suficientes, o resultado fica parcial, nunca verde por falta de evidência.

Não inclui automaticamente exploração, CVE scan, revisão de código, Security Groups, restauração de backups ou confirmação de invasão. Campo ausente significa não avaliado. A IA apresenta resumo e recomendações; não altera a classificação determinística, a skill ou o servidor.

## Provisionamento e implantação

Provisionamento explícito pela API com conta administrativa local, separado do runtime:

```bash
python3 olivaw-agent/scripts/provision_baley.py --admin-env ms-olivaw-api/deployment/olivaw-credentials.env
```

O script cria/reutiliza `baley-agent`, perfil exclusivo e permissão de envio; verifica que não é root e não recebe permissões extras. A senha gerada fica somente na seção `baley` de `_secrets/admin.json`, preservando os demais acessos administrativos. Não redefine senha de conta existente sem a cópia local correta.

A tabela depende da migration PostgreSQL e do deploy do mantenedor. Até a API publicada confirmar recebimento, a fila continua pendente; não afirmar persistência em produção só porque a coleta funcionou. Frontend administrativo: `/admin/agents` → Baley → relatórios → detalhes, protegido por root e com breadcrumb clicável.

## Verificação

```bash
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m unittest discover -s olivaw-agent/ms-baley-py/tests -v
python3 -m unittest discover -s olivaw-agent/tests -v
```

Os testes de coleta/IA/API usam doubles, sem servidor, modelo pago ou banco real. A investigação real é uma operação distinta e explicitamente autorizada. Primeira plataforma comprovada: Linux/POSIX.
