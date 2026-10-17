# Docker e ambientes do frontend

O padrão acompanha as APIs do Olivaw Hub: Compose base, override local e cinco overrides de nuvem. Cada frontend mantém imagem, build e publicação independentes da API.

## Arquivos

- `docker-compose.yml`: build, porta interna 8080, configuração pública e rede Traefik.
- `docker-compose.override.yml`: execução local em `127.0.0.1:8081`, sem rede externa obrigatória e sem descoberta Traefik.
- `docker-compose.dev.yml`, `.qas.yml`, `.rls.yml`, `.stg.yml`, `.prd.yml`: imagem, container, projeto Compose e routers próprios por ambiente.
- `.env`: configuração local preenchida.
- `.env.dev`, `.env.qas`, `.env.rls`, `.env.stg`, `.env.prd`: configurações preenchidas dos cinco ambientes.
- `30-runtime-config.sh`: prepara o JSON público antes de iniciar o Nginx, usando `API_URL`.

Os overrides de nuvem usam `traefik-net`, `websecure` e o resolver `myresolver`, ajustáveis no dotenv. Não publicam portas no host. A produção usa `https://olivaw-hub.duckdns.org`. Consulte cada `.env.<ambiente>` para os hosts adicionais; eles precisam resolver para a VM e receber certificados por hostname.

## Produção na AWS

Os arquivos `.env` acompanham o repositório. Na raiz do clone do monorepo na VM, execute o Compose diretamente.

Revise `PUBLIC_HOST`, `API_URL`, `TRAEFIK_NETWORK`, `TRAEFIK_ENTRYPOINT` e `TRAEFIK_CERTRESOLVER`. Depois:

```bash
docker compose --env-file ms-olivaw-hub-ng/docker/.env.prd \
  -f ms-olivaw-hub-ng/docker/docker-compose.yml \
  -f ms-olivaw-hub-ng/docker/docker-compose.prd.yml up -d --build
```

Para outro ambiente, troque `prd` por `dev`, `qas`, `rls` ou `stg` nos três caminhos. Não combine dotenv de um ambiente com override de outro. Os nomes dos projetos Compose são distintos, permitindo coexistência sem substituir os containers de outro ambiente.

O executor DevOps usa a mesma combinação a partir da pasta do serviço: `--env-file docker/.env.<ambiente> -f docker/docker-compose.yml -f docker/docker-compose.<ambiente>.yml up -d --build`. Antes de usá-lo, autorize `ms-olivaw-hub-ng` no caminho relativo completo da allowlist da VM, conforme o [guia do executor](../../docs/deploy-executor.md). A alteração local não atualiza essa allowlist automaticamente.

## Desenvolvimento local em Docker

```bash
docker compose -f ms-olivaw-hub-ng/docker/docker-compose.yml \
  -f ms-olivaw-hub-ng/docker/docker-compose.override.yml up -d --build
```

A porta padrão é 8081, ajustável por `FRONTEND_PORT`. O endereço da API padrão local é `http://localhost:5137`. O navegador chama a API diretamente; hostname interno Docker não é endereço público. Não há proxy para API no Angular ou Nginx.

## Settings em runtime

Em `npm start`, os settings continuam em `public/config.json`. No container, `API_URL` do Compose/dotenv gera `/tmp/olivaw/config.json`, servido sem cache em `/config.json`. Alterar essa variável e recriar o container não exige recompilar a imagem. Esse JSON temporário não altera os arquivos do build, que pertencem ao usuário da imagem.

`API_URL` deve ser uma URL absoluta HTTP(S), sem credenciais, query, fragmento ou caracteres de escape. O script admite nomes DNS, IPv4 e caminhos com caracteres ASCII de URL. URLs literais IPv6 não são suportadas por essa validação. Settings são públicos e nunca devem conter segredos. `PUBLIC_HOST` controla o router Traefik; canonical e metadados continuam definidos no build da aplicação.

Frontend e API têm publicações independentes. A API precisa permitir a origem do frontend em CORS. Não é necessário que os dois containers compartilhem rede entre si; a rede Traefik existe para o proxy de entrada. DNS, TLS e infraestrutura precisam estar preparados antes da publicação, conforme o [guia de nuvem](../../docs/cloud.md).

## Validação

```bash
docker compose --env-file ms-olivaw-hub-ng/docker/.env.prd \
  -f ms-olivaw-hub-ng/docker/docker-compose.yml \
  -f ms-olivaw-hub-ng/docker/docker-compose.prd.yml config --quiet
```

Repita para os demais ambientes. `GET /health` deve retornar 200; a raiz contém HTML público prerenderizado, rotas internas usam `index.csr.html` e assets ausentes retornam 404. O container executa Nginx sem root. As imagens base seguem tags de manutenção; fixe digests revisados se precisar reproduzir as camadas do sistema operacional.

Os dotenv dos seis ambientes (local e cinco de nuvem) são versionados e chegam prontos no clone. O Schematics os personaliza para os novos clientes. Eles contêm somente configuração pública do frontend. Os ambientes de todos os projetos acompanham o Git nesta etapa; a migração para Key Vault fica planejada para o futuro.

## Comando pelo ms-devops-api

Da raiz do monorepo, com a branch já publicada e o frontend autorizado no executor da VM:

```bash
bash scripts/deployment/deploy.sh ms-olivaw-hub-ng prd master
```

O script autentica com o root configurado e acompanha o job. O serviço, o ambiente e a branch são argumentos obrigatórios. Veja [executor DevOps](../../docs/deploy-executor.md#deploy-de-servicos-pelo-devops).

O build define explicitamente a configuração Nginx como `0644` e torna os arquivos estáticos legíveis e seus diretórios acessíveis. Assim a imagem funciona com Nginx sem root mesmo quando o executor usa `umask 077` no host.
