# Executor de deploy na VM AWS

## Quem faz o quê

```text
POST /implantacoes → CQS → SshDeploymentService → ProcessExecutionService
                                                     │ JSON via SSH
                                                     ▼
                           /opt/olivaw/deploy.py (comando fixo da chave)
                                                     │ systemd-run --user
                                                     ▼
                           worker independente: Git + Docker Compose
```

SSH é o canal autenticado entre o container e a VM. A API usa o executável `ssh` e envia JSON pela entrada padrão, sem concatenar campos em um comando remoto. A chave autoriza somente o script instalado. Não é um terminal aberto nem um novo microservice HTTP.

O script usa apenas a biblioteca padrão Python 3, já comum em Linux. O systemd da VM mantém o worker vivo após fechar a conexão SSH ou reiniciar o container da API. Os jobs são arquivos privados no host; não dependem do banco da API. Não há shell, `eval`, socket Docker montado na API ou acesso SSH implantado automaticamente.

## Contrato HTTP

Autenticação JWT e permissão `devops.implantar` continuam obrigatórias para ambas as rotas.

```http
POST /implantacoes
Content-Type: application/json

{"folder":"olivaw-hub/ms-devops-api","branch":"develop","environment":"dev"}
```

Retorna **202 Accepted**, `Location: /implantacoes/<id>` e corpo `{"id":"<uuid>","status":"queued","log":"Deployment queued."}`. Consulte `GET /implantacoes/<id>` para acompanhar `queued`, `running`, `succeeded` ou `failed`.

Erro de entrada/conexão/protocolo/aceite é `ValidationProblemDetails` HTTP 400; job inexistente retorna 404. Falha de checkout, pull, build ou Compose é um **job `failed`**, consultável com HTTP 200 e log seguro. A saída bruta dos comandos é descartada; nenhum segredo ou stack trace é enviado ao cliente.

A consulta não modifica o registro: se o systemd indicar que um worker pendente foi interrompido/expirou, o status retornado informa falha. O envio considera esse estado ao limitar a fila a 20 jobs pendentes. Uma desconexão no instante do aceite pode deixar o resultado do envio incerto; ainda não há chave de idempotência HTTP nem recuperação automática desse ID.

## Preparação manual da VM

Esta documentação não foi executada na AWS. Revise cada etapa antes de instalar. Os exemplos usam `ec2-user`, que já acessa os repositórios em `/home/ec2-user` e o Docker. Esse usuário e o executor são privilegiados: acesso ao Docker permite controlar o host. Restrinja quem pode alterar os repositórios/Compose e quem possui a permissão de deploy.

Na raiz do clone `/home/ec2-user/olivaw-hub`:

```bash
sudo install -d -m 0755 /opt/olivaw /etc/olivaw /etc/olivaw/ssh
sudo install -o root -g root -m 0644 scripts/deployment/deploy.py /opt/olivaw/deploy.py
sudo install -o root -g root -m 0644 scripts/deployment/deployment.example.json /etc/olivaw/deployment.json
sudo loginctl enable-linger ec2-user
```

O script/configuração ficam fora do clone, administrados por root: um checkout não substitui o próprio executor. Atualize essa cópia explicitamente quando mudar o protocolo. Confira `systemctl --user status` em uma sessão de `ec2-user`; o gerenciador de usuário deve estar disponível. O script define o endereço do barramento em `/run/user/<uid>/bus`.

Edite `/etc/olivaw/deployment.json`: `rootDirectory=/home/ec2-user`; `allowedFolders` contém apenas os caminhos relativos autorizados (inicialmente `olivaw-hub/ms-devops-api` e `lisierp`). Configure `stateDirectory` em diretório privado do usuário. Git e Docker devem existir em `/usr/bin/git` e `/usr/bin/docker`, com Compose disponível. Git precisa de upstream e credenciais não interativas para `ec2-user`. Nenhuma credencial atual é copiada pelo código.

Gere uma chave **exclusiva** para a API, uma única vez:

```bash
sudo ssh-keygen -t ed25519 -f /etc/olivaw/ssh/deployment_key -N ''
```

Acrescente a chave **pública** ao `~ec2-user/.ssh/authorized_keys` mantendo as chaves administrativas existentes. A linha deve ter este prefixo:

```text
restrict,command="/usr/bin/python3 /opt/olivaw/deploy.py" ssh-ed25519 CHAVE_PUBLICA_DA_API
```

O gateway exige o comando original fixo `olivaw-deploy` e lê somente `submit`/`status` do JSON. Não use essa chave para login geral. Preserve as permissões de `.ssh` (0700) e `authorized_keys` (0600).

Crie `/etc/olivaw/ssh/known_hosts` com a chave pública **do servidor**, verificada diretamente na VM (por exemplo `/etc/ssh/ssh_host_ed25519_key.pub`), usando o alias da conexão:

```text
host.docker.internal ssh-ed25519 CHAVE_PUBLICA_VERIFICADA_DA_VM
```

A API usa `StrictHostKeyChecking=yes`; não aceite automaticamente o resultado de `ssh-keyscan` sem conferir a identidade do host. A chave privada pertence ao usuário `app` dentro da imagem; confira seu UID/GID na imagem construída antes de ajustar o arquivo no host (nas imagens habituais é 1654). Dê a esse UID/GID a propriedade do arquivo privado com modo 0400. O `known_hosts` não é secreto e pode usar modo 0444. Não coloque esses arquivos no Git ou no contexto de build.

## Container e comandos executados

O Dockerfile da API instala somente o cliente OpenSSH na imagem final e mantém execução como `app`. O override opcional `docker-compose.ssh.yml` monta chave e `known_hosts` em leitura, configura SSH e resolve `host.docker.internal` para o gateway do host. Não monta projetos ou `/var/run/docker.sock`. Confira acesso do bridge Docker ao SSH da VM; não é necessário abrir uma nova porta pública para o executor.

Para conferir a composição, na raiz do monorepo:

```bash
docker compose --env-file ms-devops-api/docker/.env.dev \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.dev.yml \
  -f ms-devops-api/docker/docker-compose.ssh.yml config --quiet
```

Antes de subir, provisione os arquivos; o override recusa criar diretórios vazios no lugar deles. Inclua esse mesmo override nos deploys futuros do próprio DevOps: `COMPOSE_FILE` não é suficiente, pois o executor usa `-f` explícito. A lista `sshOverrideFolders` do exemplo faz o executor incluir o override SSH para `olivaw-hub/ms-devops-api`, mantendo seus mounts/configuração ao atualizar a API.

Na raiz do serviço selecionado, o worker roda checkout, pull `--ff-only` e:

```bash
docker compose --env-file docker/.env.dev \
  -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up -d --build
```

O ambiente validado seleciona os arquivos `dev/qas/stg/rls/prd`. Para o primeiro projeto externo na VM, use `folder=lisierp` (nome exibido no console); não use o nome local `lisifin` sem conferir a pasta remota.

`--build` é necessário para publicar as alterações de código obtidas pelo Git; o override SSH também é acrescentado para o próprio DevOps. A fila usa um lock único **na VM**, incluindo serviços do mesmo monorepo, evitando checkout concorrente. A API não mantém mais `DeploymentGate` local.

Cada etapa tem timeout (300s padrão); o worker inteiro expira após 1800s, incluindo espera pelo lock. O timeout encerra o grupo de processos da etapa; o systemd controla o restante do serviço. Falhas interrompem as próximas etapas, sem rollback. Preserve e monitore a pasta de estado; retenção/limpeza de jobs finalizados, health checks pós-deploy e rollback ficam para evoluções futuras.

## Arquivos e testes

- `scripts/deployment/deploy.py`: gateway, validação no host, fila, worker e estado persistido.
- `scripts/deployment/deployment.example.json`: caminhos e allowlist da VM.
- `ProcessExecutionService.cs`: cliente SSH; `SshDeploymentService.cs`: protocolo JSON.
- `DeploymentSettings.cs`: host/porta/usuário/chave/known_hosts e timeout SSH (30s padrão, máximo 120s).

```bash
python3 -m unittest discover -s scripts/deployment/tests -v
dotnet test ms-devops-api/tests/Mps.Devops.Tests/Mps.Devops.Tests.csproj
```

Os testes simulam SSH, systemd, Git e Docker; usam apenas arquivos temporários locais. A instalação real, conectividade, permissões, imagem e sobrevivência a reinício precisam de validação posterior na VM.

Referências: [restrições de chaves SSH](https://man.openbsd.org/sshd.8), [verificação de host](https://man.openbsd.org/ssh_config.5), [systemd-run](https://www.freedesktop.org/software/systemd/man/latest/systemd-run.html) e [privilégios Docker](https://docs.docker.com/engine/security/).
