# Infraestrutura compartilhada com volumes preservados

## user-history

Como responsável pela Olivaw,
quero manter e executar a infraestrutura compartilhada pelo repositório,
para migrar de VM sem trocar volumes, credenciais ou identidade dos serviços existentes.

## acceptance-criteria

- Compose reúne PostgreSQL, RabbitMQ e Traefik, com Redis opcional e desligado por padrão.
- Preserva volumes, redes, credenciais, identidade do nó RabbitMQ, ACME e imagens em execução.
- Faz conferência e backup antes da troca, com possibilidade de retorno sem remover volumes.
- Infraestrutura usa logs com rotação limitada; Traefik registra informações de acesso sem cabeçalhos secretos.
- APIs Olivaw, DevOps e LTMJ continuam acessando banco e respondendo por HTTPS depois da migração.

## ai-prompt

### Entrega

Implementada, validada e instalada em 24/10/2026 com autorização explícita do mantenedor. Responsável Codex, worktree atual na branch recebida `feature/issue023-tests-and-adjusts`, sem nova issue criada. Arquivos em [ms-infrastructure-app](../../../ms-infrastructure-app/README.md), incluindo `.env` real, Compose, AGENTS e scripts de preparação/checagem. A integração Git, commit/push e recuperação em VM nova continuam separados desta implantação manual autorizada.

### Preservação e operação

- Projeto ativo `ms-infrastructure-app`, containers `ms-postgres-db`, `ms-rabbit-mq` e `ms-traefik-pr`.
- Volumes externos existentes `ms-fintalk-dat_postgres-data`, `ms-fintalk-dat_rabbitmq-data` e `ms-fintalk-dat_rabbitmq-logs`. Nenhum volume criado, removido ou renomeado. Mesmos diretórios de dados; redes externas originais preservadas.
- RabbitMQ mantém hostname `ce49564db068` e nó `rabbit@ce49564db068`, evitando abrir outro diretório Mnesia no volume antigo. Não trocar hostname ao reutilizar dados.
- ACME conserva `/home/ec2-user/ms-fintalk-dat/acme.json`, modo 0600. Credenciais preservadas dos containers anteriores sem exibi-las; `.env` local/remoto em 0600.
- Imagens PostgreSQL 17, RabbitMQ 3-management e Traefik v3.0 com os mesmos image IDs. `up -d --pull never --no-build`, sem atualização ou início de Redis.
- Backup lógico dos bancos e cópia privada de configuração/ACME em `/var/backups/olivaw-infra/20261024T153252Z`; volume RabbitMQ copiado depois de parar o broker. Backup root-only 0700, arquivos 0600. Retenção desses backups ainda precisa ser definida.
- Containers anteriores parados e renomeados com `-rollback-20261024T153252Z`. Não executar originais e novos juntos nos mesmos volumes; limpeza posterior somente de containers, sem `-v`.

### Verificações

- Compose base e profile Redis válidos; quatro testes locais passaram para scanner e preparação segura, incluindo recusa de criar volumes ausentes no modo de conferência.
- Cluster PostgreSQL preservou system identifier; mesmos bancos e OIDs antes/depois. Não houve comparação exaustiva de todas as tabelas.
- Olivaw, DevOps e LTMJ: `/ready` interno e HTTPS com TLS validado, HTTP 200 e PostgreSQL pass. `/version` sem token continua 401. Mesmos IDs de containers de API, sem reinício desses serviços.
- AMQP TCP alcançável por DNS a partir de cada API. Endpoint administrativo RabbitMQ retornou 200; credenciais configuradas das APIs Olivaw e DevOps também autenticaram com 200 em whoami. LTMJ está com MassTransit desabilitado. Não foram publicados eventos nem validados fluxos de negócio de mensageria.
- Contagem dos logs recentes das APIs Olivaw/DevOps não encontrou Broker unreachable, ACCESS_REFUSED, Connection refused ou Bus start faulted no recorte inspecionado. Não há consumidores/conexões AMQP ativos na leitura; isso fica explícito e não comprova publicação completa pela aplicação.
- Três novos containers rodando, zero reinícios na leitura inicial, mounts idênticos e driver json-file em 3 × 5 MB. Access logs JSON observados no Traefik sem Authorization, Cookie ou X-Lead-Key. Retenção por tamanho, sem promessa de dias mínimos. Volumes de logs RabbitMQ, aplicações e journal do host exigem política própria.

### Pendências de segurança

A [auditoria ampla](../discovery/seguranca-e-infraestrutura.md) permanece em discovery: rotação de chave SSH recuperável do histórico, restrição do socket Docker, readiness público detalhado, hardening do host/proxy e revisão de versões. Esta entrega não declara o servidor integralmente protegido nem comprova recuperação de desastre em outra VM.

## user-feedback

```text
vi que voce criou volumes ou variaveis de volme tbm tem que preservar os mesmos blz ou perdemos dados.

após confirmar isso faz o seguinte, mata os containers de infra e sobe o nosso novo compose blz?
ai testa se as apis tão acessando coretamente
```
