# Infraestrutura compartilhada da Olivaw

Compose do PostgreSQL, RabbitMQ e Traefik usados na VM; Redis opcional. Este módulo permite reconstruir a infraestrutura em outra VM, mas **não substitui backup dos dados**, configuração AWS, aplicações, DNS ou instalação do executor de deploy. Compose instalado no servidor em 24/10/2026, preservando imagens, volumes, credenciais, hostname RabbitMQ e ACME; veja o registro operacional ao final.

## Configuração

O arquivo real é [`.env`](.env), conforme preferência do mantenedor; não há arquivo `example`. Nomes de volumes refletem os volumes detectados na auditoria. Na VM atual, a configuração foi preservada dos containers anteriores sem troca de credenciais. Para outra VM, conferir os valores e o plano de restauração antes de operar. Não imprimir `docker compose config` sem `--quiet`: ele interpola segredos.

As imagens PostgreSQL 17, RabbitMQ 3 e Traefik 3.0 preservam as linhas existentes. Isso não atesta ausência de vulnerabilidades; atualização de versões/digests depende da revisão de advisories e de testes próprios, sem atualizar a VM como efeito deste módulo. A identidade persistida do RabbitMQ usa `RABBITMQ_HOSTNAME`; na VM atual é `ce49564db068`, preservando o nó `rabbit@ce49564db068`. Não trocar esse valor em uma migração dos dados existentes. Redis 7.4 é uma sugestão inicial; não existe Redis ativo na VM auditada.

Somente Traefik publica 80/443. Bancos, broker e cache usam redes próprias sem portas de host. O socket Docker continua acessível ao Traefik: montar `:ro` não transforma a API do daemon em somente leitura. Isolar essa API com proxy de permissões mínimas permanece na [feature de segurança](../docs/features/discovery/seguranca-e-infraestrutura.md).

## Validar sem iniciar serviços

Na raiz do monorepo, após preencher o `.env`:

```bash
docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml config --quiet
docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml --profile redis config --quiet
python3 ms-infrastructure-app/scripts/check-private-keys.py
python3 -m unittest discover -s ms-infrastructure-app/tests -v
```

`check-private-keys.py` verifica o conteúdo dos arquivos rastreados no checkout sem imprimir chaves. Não varre nem reescreve o histórico Git. `.gitignore` exclui chaves privadas, estado ACME e backups; a cópia SSH administrativa deve existir somente na máquina do operador com 0600.

## VM existente

Antes de qualquer recriação, confirme com `docker inspect` os labels `com.docker.compose.project`, os volumes e a origem do ACME. Na VM auditada, os recursos são:

- Projeto legado: `ms-fintalk-dat`.
- PostgreSQL: `ms-fintalk-dat_postgres-data`.
- RabbitMQ: `ms-fintalk-dat_rabbitmq-data` e `ms-fintalk-dat_rabbitmq-logs`.
- Certificados: `/home/ec2-user/ms-fintalk-dat/acme.json`, permissão 0600. Contém chaves TLS; não copiar para o Git.
- Redes externas: `postgres-net`, `rabbit-net` e `traefik-net`.

Configure `TRAEFIK_ACME_FILE` com o caminho absoluto atual. Preserve credenciais, banco inicial e volumes reais; variáveis de inicialização não trocam senhas de um banco já preenchido. Não criar outro volume vazio quando um volume esperado não for encontrado.

```bash
python3 ms-infrastructure-app/scripts/prepare.py
```

Sem `--new-host`, o script só confere recursos e recusa ausências. Antes da migração de 24/10/2026, uma opção era adotar o projeto legado. A instalação efetiva parou/renomeou os containers anteriores e usou o projeto novo, preservando todos os volumes externos. O comando abaixo é somente referência de adoção do projeto legado; **não executar na VM já migrada**:

```bash
docker compose --project-name ms-fintalk-dat --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml up -d --no-deps ms-traefik-pr
```

Esse comando **não foi executado**. Recriar Traefik pode interromper brevemente HTTPS. Não executar `up` de toda a stack apenas para habilitar logs e não executar `down -v`. Trocar o nome do projeto sem coordenar os containers existentes causa conflitos pelos nomes fixos.

## VM nova

1. Instale Docker/Compose e configure acesso administrativo, atualizações, Security Groups e DNS. Deixe somente portas necessárias acessíveis; SSH restrito às origens administrativas.
2. Obtenha o checkout e preencha `.env`. Ajuste nomes se os backups forem restaurados em outros volumes.
3. Execute a preparação explícita de uma VM nova:

   ```bash
   python3 ms-infrastructure-app/scripts/prepare.py --new-host
   ```

4. Restaure os bancos, RabbitMQ quando necessário e o ACME por canal seguro, validando integridade e compatibilidade. Volume criado vazio não contém dados recuperados. O script não faz backup/restauração. Para PostgreSQL, prefira procedimento de dump/restore ou backup físico consistente compatível; não copiar dados de um banco em execução sem ferramenta adequada.
5. Depois de restaurar e autorizar a subida, execute na raiz:

   ```bash
   docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml up -d
   ```

6. Reinstale explicitamente o [executor](../docs/deploy-executor.md), com nova chave restrita gerada no destino e host SSH verificado. A chave administrativa privada nunca é copiada para a VM.
7. Publique aplicações separadamente e valide health, autenticação, certificados e acesso aos dados restaurados antes da troca de DNS.

Redis é opt-in: preparar com `--redis` e subir com `--profile redis` somente após avaliar memória. O limite inicial de dados é 64 MB, container 128 MB, persistência AOF e `noeviction`; operações podem falhar ao atingir o limite. Escolha TTLs/ACLs e retenção conforme uso. A VM auditada tem cerca de 913 MiB de RAM e já utiliza swap; não adicionar Redis por padrão.

## Logs e identificação de chamadas

O Compose habilita access logs JSON com horário UTC, IP do cliente, host, método, caminho, status e identificação do roteamento. Cabeçalhos são descartados, exceto User-Agent. Authorization, Cookie, Referer e X-Lead-Key não são registrados. Não transportar segredos em URLs. O IP pode ser de um proxy intermediário e User-Agent pode ser falsificado; isso não identifica sozinho uma pessoa.

O driver `json-file` limita a três arquivos de 5 MB por container deste módulo, cerca de 15 MB cada. A retenção é por tamanho, não por dias; o histórico mais antigo é descartado quando gira. Logs em volumes (como `/var/log/rabbitmq`) e journals do host exigem política própria e não são limitados por esse driver. Essa mudança vale após recriação e não altera logs de aplicações já existentes. Na leitura inicial, os logs estavam sem rotação e o access log desabilitado. A instalação de 24/10/2026 aplicou a rotação nos três containers de infraestrutura e habilitou o access log do Traefik; os demais containers continuam exigindo retenção própria.

```bash
docker logs --since 10m ms-traefik-pr
```

O projeto pode ser validado sem conexão ao daemon. A substituição operacional foi autorizada e realizada em 24/10/2026; restauração integral em outra VM ainda não foi ensaiada. Veja a [auditoria e plano de correções](../docs/features/discovery/seguranca-e-infraestrutura.md).


## Instalação de 24/10/2026

O módulo foi instalado em `/home/ec2-user/olivaw/ms-infrastructure-app`, com `.env` em 0600, usando as credenciais extraídas dos containers existentes sem exibi-las. O projeto ativo passou a `ms-infrastructure-app`; o Compose foi iniciado com `--pull never --no-build`, preservando os mesmos image IDs.

Volumes reaproveitados como externos: `ms-fintalk-dat_postgres-data`, `ms-fintalk-dat_rabbitmq-data` e `ms-fintalk-dat_rabbitmq-logs`. Redes e `/home/ec2-user/ms-fintalk-dat/acme.json` foram mantidos. Nenhum volume foi criado, removido ou renomeado. Redis permaneceu desligado.

Antes da janela, foram salvos inspect privado, configuração, ACME e dump lógico dos bancos em `/var/backups/olivaw-infra/20261024T153252Z`, diretório root-only 0700 e arquivos privados 0600. Depois de parar RabbitMQ, seu volume de dados recebeu cópia física para recuperação. Esses arquivos contêm segredos/dados e não devem entrar no Git nem ser disponibilizados via HTTP. Confirmar retenção e destinação do backup após a aceitação da migração; não deixar backups acumularem sem política.

Containers anteriores foram parados e renomeados com sufixo `-rollback-20261024T153252Z`; eles não estão executando e não são os serviços ativos. Não iniciar esses containers enquanto os novos estiverem usando os mesmos volumes. São retenção temporária para retorno; remover somente os containers após aceitar a migração, sem `-v`. Não executar comandos Compose do diretório legado para operar a stack nova.

Para mudanças futuras usar o arquivo deste módulo, por exemplo:

```bash
docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml ps
docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml up -d --pull never --no-deps ms-traefik-pr
```


Validação pós-troca: Olivaw, DevOps e LTMJ responderam `200/pass` em `/ready`, pela rede interna e HTTPS com certificado validado. Os IDs dos containers de API permaneceram iguais. O PostgreSQL conservou identificador de cluster e bancos/OIDs; RabbitMQ conservou o nó. Cada API alcançou AMQP por TCP e o painel interno autenticado do broker respondeu 200; não foi publicado evento de negócio. Access logs foram observados e os três containers ativos exibem rotação 3 × 5 MB e zero reinícios na leitura inicial.

Credenciais de mensageria das APIs Olivaw/DevOps autenticaram no broker com HTTP 200; LTMJ está com mensageria desabilitada. Na leitura não havia consumidores/conexões AMQP ativos, e os padrões de erro inspecionados não apareceram nos logs recentes; não foi testado fluxo completo de publicação/consumo de negócio. Veja a [entrega concluída](../docs/features/done/infraestrutura-compartilhada.md).

### Descoberta Docker com proxy restrito (pendente de publicação)

A proposta opcional em `docker-compose.socket-proxy.yml` faz o Traefik deixar de montar diretamente o socket. O `docker-compose.yml` padrão permanece preservado; não habilitar a proposta antes da validação isolada. `ms-docker-socket-proxy` usa HAProxy 3.2.25, rede interna exclusiva sem porta publicada, 64 MiB de teto, 0,10 vCPU e logs limitados. O proxy ainda possui acesso ao daemon: seu comprometimento continua sendo risco elevado. A montagem `:ro` não restringe verbos da API.

A allowlist de `socket-proxy.cfg` foi derivada do provider Docker do Traefik v3.0.0: versão, lista de containers, inspect por ID e eventos. Ping GET/HEAD permite negociação do cliente. Requisições de escrita, logs, exec, arquivos, imagens, volumes, build e demais endpoints são negados. Inspect necessário à descoberta ainda expõe metadados sensíveis ao Traefik. O executor de deploy mantém seu acesso independente.

Fontes: [provider Traefik v3.0.0](https://github.com/traefik/traefik/blob/v3.0.0/pkg/provider/docker/pdocker.go), [acesso ao daemon](https://doc.traefik.io/traefik/v3.0/providers/docker/#docker-api-access), [manutenção HAProxy](https://www.haproxy.org/bugs/bugs-3.2.12.html). Imagem fixada para revisão; validar disponibilidade/digest e advisories antes de publicar. Não atualizar Traefik ou demais imagens como efeito desta alteração.

Antes de publicar, validar em Docker isolado com as versões reais: descoberta inicial, eventos, recriação de serviço, nova rota, negação de POST e endpoints não autorizados, memória e reconexão do stream. Não houve teste de runtime do proxy nem avaliação de capacidade na VM nesta entrega. Validar sintaxe HAProxy com `docker run --rm -v "$PWD/ms-infrastructure-app/socket-proxy.cfg:/usr/local/etc/haproxy/haproxy.cfg:ro" haproxy:3.2.25-alpine haproxy -c -f /usr/local/etc/haproxy/haproxy.cfg` apenas em ambiente isolado autorizado.

Publicar somente proxy/Traefik após revisão, sem reiniciar bancos ou broker, sem `down` e sem remover volumes. Recuperação: restaurar a configuração anterior do Traefik com socket direto e recriar somente o Traefik. ACME, redes externas, volumes e hostname RabbitMQ permanecem idênticos.
