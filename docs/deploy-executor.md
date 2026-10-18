# Executor de deploy na VM AWS

## Quem faz o quê

```text
POST /implantacoes → CQS → SshDeploymentService → ProcessExecutionService
                                                     │ JSON via SSH
                                                     ▼
                           /opt/olivaw/deploy.py (comando fixo da chave)
                                                     │ systemd-run --user
                                                     ▼
                           worker independente: Git na raiz do repo + Compose no serviço
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

O Dockerfile da API instala somente o cliente OpenSSH na imagem final e mantém execução como `app`. O Compose base monta a chave e o `known_hosts` em leitura e configura o acesso SSH a `host.docker.internal`; os caminhos do host ficam em `DEPLOYMENT_KEY_SOURCE` e `DEPLOYMENT_KNOWN_HOSTS_SOURCE` no dotenv. Os arquivos precisam existir na VM antes de subir, e o Compose falha se algum estiver ausente. Não monta projetos ou `/var/run/docker.sock`. Confira acesso do bridge Docker ao SSH da VM; não é necessário abrir uma nova porta pública para o executor. `docker-compose.ssh.yml` permanece como override de compatibilidade para o worker já instalado.

Para conferir a composição, na raiz do monorepo:

```bash
docker compose --env-file ms-devops-api/docker/.env.dev \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.dev.yml config --quiet
```

Antes de subir, provisione os arquivos; o Compose base recusa criar diretórios vazios no lugar deles. Os deploys futuros do próprio DevOps mantêm SSH porque os mounts e variáveis agora fazem parte do Compose base.

O worker encontra a raiz Git existente acima da pasta autorizada e executa `git fetch origin`, checkout e `git pull --ff-only origin <branch>` nessa raiz. Só depois verifica a pasta do serviço e seus arquivos de ambiente/Compose, e executa nela:

```bash
docker compose --env-file docker/.env.dev \
  -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up -d --build
```

O ambiente validado seleciona os arquivos `dev/qas/stg/rls/prd`. Para o primeiro projeto externo na VM, use `folder=lisierp` (nome exibido no console); não use o nome local `lisifin` sem conferir a pasta remota.

`--build` é necessário para publicar as alterações de código obtidas pelo Git. A fila usa um lock único **na VM**, incluindo serviços do mesmo monorepo, evitando checkout concorrente. A API não mantém mais `DeploymentGate` local.

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

## Deploy de serviços pelo DevOps

O cliente genérico `scripts/deployment/deploy.sh` autentica em `https://api.devops.olivaw-hub.duckdns.org` com o login `root`, solicita a implantação do serviço informado e acompanha o job até sucesso ou falha. A senha fica no script por autorização explícita do mantenedor, até Key Vault; nunca imprima senha ou token.

Na raiz do clone, os três argumentos são obrigatórios:

```bash
bash scripts/deployment/deploy.sh <servico> <ambiente> <branch>
```

Exemplos:

```bash
bash scripts/deployment/deploy.sh ms-olivaw-hub-ng prd master
bash scripts/deployment/deploy.sh ms-devops-api prd master
bash scripts/deployment/deploy.sh ms-novo-servico-api stg develop
bash scripts/deployment/deploy.sh lisierp prd master
```

Para serviços com prefixo `ms-`, o cliente monta `folder=olivaw-hub/<servico>`. Para repositórios independentes, como `lisierp`, usa `folder=<servico>`. Em ambos os casos, usa o ambiente `dev/qas/rls/stg/prd` e envia a branch informada. Exibe o destino antes de autenticar; não há serviço, ambiente ou branch implícitos. Novos serviços não exigem editar esse script.

Cada serviço deve ter `docker/.env.<ambiente>`, `docker/docker-compose.yml` e `docker/docker-compose.<ambiente>.yml` na branch publicada. Seu caminho completo precisa estar em `allowedFolders` de `/etc/olivaw/deployment.json` na VM. O exemplo versionado inclui o frontend institucional e DevOps; não modifica a configuração já instalada. Não adicione frontends a `sshOverrideFolders`; essa opção preserva a integração SSH específica do DevOps.

Para solicitar sem acompanhar, acrescente `--no-wait`. O script usa Bash e Python 3, não exige curl/jq e nunca imprime senha ou token. `DEVOPS_API_URL` e `DEVOPS_ROOT_PASSWORD` substituem os valores fixos sem editar o arquivo.

A branch deve estar publicada com todos os arquivos de implantação. Se a senha root já tiver sido alterada no banco, atualize a credencial do script; o bootstrap não redefine usuários existentes. O cliente não faz logout automaticamente porque o logout atual revoga todas as sessões root. Ao implantar o próprio DevOps, o worker continua na VM; a consulta de status pode ser interrompida temporariamente pelo reinício da API. Nesse caso, consulte o ID do job antes de enviar outro deploy.

## Primeiro deploy de um serviço e compatibilidade com lisierp

`folder` identifica onde executar o Compose, não obriga essa pasta a existir antes da atualização Git. No monorepo, `olivaw-hub/ms-olivaw-hub-ng` usa Git em `/home/ec2-user/olivaw-hub` e Compose em sua subpasta após atualizar a branch. O clone raiz precisa existir, e o serviço precisa estar na allowlist. Se a branch não contiver a pasta ou os arquivos de implantação, o job falha sem executar Docker.

Para `folder=lisierp`, Git e Compose continuam em `/home/ec2-user/lisierp`, seu repositório independente. Não há redirecionamento desse projeto para o monorepo. O lock único da VM, a allowlist, a recusa de links simbólicos e o override SSH específico do DevOps são preservados.

Após publicar esta correção e atualizar o clone na VM, instale a nova versão do executor:

```bash
cd /home/ec2-user/olivaw-hub
sudo install -o root -g root -m 0644 scripts/deployment/deploy.py /opt/olivaw/deploy.py
```

Não é necessário reconstruir a API DevOps para essa alteração de script. A cópia em `/opt/olivaw` é usada pelos novos workers; o pull do monorepo sozinho não substitui o executor instalado. Atualize quando não houver implantação em andamento.

## Permissões dos arquivos da API

Os Dockerfiles do DevOps, template e API LTMJ copiam o publish com `--chown=app:app` e executam como `USER app`. Isso permite ler appsettings e assemblies mesmo quando o checkout do executor herda `umask 077`. O usuário Linux do container é `app`; ele não é o usuário root da aplicação nem o usuário SSH da VM. Se a API estiver fora do ar, reconstruir seu container diretamente pelo Compose na VM; o cliente HTTP de deploy depende de a API responder.


## Limpeza de disco pela API DevOps

`POST /disk-cleanups` cria um job assíncrono, sem body, e retorna HTTP 202 com `id`, `status`, `log` e `Location: /disk-cleanups/{id}`. `GET /disk-cleanups/{id}` acompanha o resultado. As duas rotas exigem root ativo da própria identidade DevOps e versão de sessão vigente; uma conta comum com permissão de deploy não recebe acesso à limpeza.

O container da API usa o mesmo executor SSH restrito dos deploys. O Docker CLI e a limpeza rodam na VM, sob `ec2-user`, que já possui acesso ao Docker. Não há montagem do socket Docker, execução privilegiada nem comandos shell enviados pelo cliente HTTP. O protocolo SSH aceita apenas a ação fixa `cleanup`; campos adicionais são rejeitados no executor.

Comandos fixos, nesta ordem:

```bash
docker builder prune --all --force
docker image prune --force
```

O primeiro remove cache de build sem uso, podendo exigir recompilação/download em builds futuros. O segundo remove apenas imagens dangling (sem tag e sem referência de container). Imagens antigas com tag são preservadas nesta versão, mesmo se o Docker as contabilizar como recuperáveis. Nenhum container, volume, banco, upload ou imagem usada por container é removido.

Limpeza e deploy usam o mesmo lock na VM: a limpeza aguarda o deploy terminar e impede novo build enquanto executa. O job registra bytes livres na partição `/` antes/depois e o código de saída de cada etapa. Uma etapa com erro interrompe as seguintes. O timeout por etapa usa `stepTimeoutSeconds`; o limite global do worker continua em 30 minutos, incluindo espera pelo lock. Cancelar o curl não cancela o job agendado.

### Instalar a nova capacidade

Após commit/push, atualizar o clone na VM para a branch da entrega. Quando não houver job em execução, atualizar o executor instalado:

```bash
cd /home/ec2-user/olivaw-hub
sudo install -o root -g root -m 0644 scripts/deployment/deploy.py /opt/olivaw/deploy.py
```

Depois, publicar `ms-devops-api`. Esta funcionalidade requer tanto o executor novo quanto a API nova; o pull sozinho não atualiza `/opt/olivaw/deploy.py`. Não precisa alterar a allowlist de pastas nem instalar novas chaves SSH. A implementação não cria migrations ou tabelas; reutiliza os registros privados de jobs existentes.

### Solicitar e acompanhar

Usar token root emitido pela API DevOps, diferente do token da API LTMJ:

```bash
read -r -s -p 'Token root DevOps: ' DEVOPS_ROOT_TOKEN
printf '\n'
curl --fail-with-body --request POST \
  'https://api.devops.olivaw-hub.duckdns.org/disk-cleanups' \
  --header "Authorization: Bearer $DEVOPS_ROOT_TOKEN"
```

Para consultar, preencher `CLEANUP_ID` com o `id` retornado:

```bash
curl --fail-with-body \
  "https://api.devops.olivaw-hub.duckdns.org/disk-cleanups/$CLEANUP_ID" \
  --header "Authorization: Bearer $DEVOPS_ROOT_TOKEN"
unset DEVOPS_ROOT_TOKEN
```

Se o disco já estiver completamente cheio, pode faltar espaço até para gravar o registro do job. Nessa situação, a primeira liberação de espaço continua sendo manual na VM. A rota limpa somente os recursos listados; não substitui diagnóstico de consumo fora do Docker nem garante recuperar todos os bytes que `docker system df` aponta como recuperáveis.
