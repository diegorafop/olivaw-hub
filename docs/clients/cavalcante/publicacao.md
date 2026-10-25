# Publicação da Cavalcante

## Endereço definido

Produção: **https://cavalcante.olivaw.com.br**. API reservada e independente: `https://api.cavalcante.olivaw.com.br`; não é necessária para o protótipo público.

O frontend já configura:

- `PUBLIC_HOST=cavalcante.olivaw.com.br` em `ms-cavalcante-ng/docker/.env.prd`.
- Regra Host do Traefik com o mesmo domínio, TLS e porta interna 8080 em `docker/docker-compose.prd.yml`.
- `server_name cavalcante.olivaw.com.br` no Nginx.
- Canonical e `og:url` com a URL definida no HTML.
- Ambientes separados como `cavalcante-dev.olivaw.com.br`, `cavalcante-qas.olivaw.com.br`, `cavalcante-rls.olivaw.com.br` e `cavalcante-stg.olivaw.com.br`.

## Preparação local reproduzível

Na raiz do monorepo:

```bash
docker compose --env-file ms-cavalcante-ng/docker/.env.prd \
  -f ms-cavalcante-ng/docker/docker-compose.yml \
  -f ms-cavalcante-ng/docker/docker-compose.prd.yml config --quiet

docker build -f ms-cavalcante-ng/docker/Dockerfile \
  -t ms-cavalcante-ng:prd ms-cavalcante-ng
```

Esses comandos validam configuração e constroem imagem; não publicam. O workflow de frontend inclui o novo projeto nas verificações.

A implantação deve usar os mecanismos existentes do Olivaw, ambiente `prd` e branch/tag definida pelo mantenedor. Não tomar o nome da branch feature como autorização de publicação. Neste estágio não executar a API: o startup herdado pode inicializar identidade e aplicar migrations pendentes dos provedores.

## Antes de disponibilizar

- Confirmar aprovação visual e dados comerciais, inclusive autorização de fotos.
- Confirmar registro DNS de `cavalcante.olivaw.com.br` apontando para a infraestrutura autorizada e certificado emitido pelo Traefik. Configuração local não cria DNS nem TLS remoto.
- Definir se a primeira publicação será demonstração ou versão oficial. Demonstração mantém `noindex,nofollow` e `robots.txt` bloqueado; versão oficial remove o bloqueio e publica sitemap com URLs aprovadas.
- Definir como o domínio antigo se relaciona com o novo endereço antes de redirecionamentos ou mudança de canonical no site antigo.
- Após implantação autorizada, verificar HTTP/HTTPS, certificado, HTML prerenderizado, fotos, favicon, links e configuração runtime sem cache persistente. Validar no celular.

## Publicação autorizada — 24/10/2026

O mantenedor autorizou executar a publicação do frontend na branch `feature/issue024-cavalcante`. DNS público de `cavalcante.olivaw.com.br` e `api.cavalcante.olivaw.com.br` confirmado em `98.95.250.122`. A API não foi implantada: o registro DNS apenas reserva o endereço.

Foi configurada a conta técnica de deployment da Cavalcante na identidade Olivaw, sem root e com somente `devops.deploy` e `devops.deploy:olivaw/ms-cavalcante-ng`. A credencial fica em `ms-cavalcante-ng/deployment/olivaw-credentials.env`, conforme a política do repositório privado; o Docker já exclui esse caminho.

A allowlist da VM recebeu somente `olivaw/ms-cavalcante-ng`. O checkout anterior falhava porque dez arquivos de infraestrutura já existiam fora do índice. O conteúdo era idêntico ao da branch; foram preservados em backup privado na VM, restaurados após o checkout e conferidos com `git status` limpo. Nenhum serviço de infraestrutura foi reiniciado.

Comando reproduzível na raiz:

```bash
bash scripts/deployment/deploy.sh ms-cavalcante-ng prd feature/issue024-cavalcante
```

Job: `45bd5895-ed3d-4d1b-aefb-6ea4ad7286b0`, concluído com `succeeded`. Container `ms-cavalcante-ng` em `running/healthy`. A imagem contém os fontes do commit `550b37e`; os commits posteriores registram credencial e documentação, sem alteração no frontend publicado.

Verificação externa: landing e `/health` HTTP 200; favicon, avatar, foto principal e `config.json` HTTP 200; asset inexistente HTTP 404. Certificado TLS válido para `cavalcante.olivaw.com.br`, com expiração em 23/01/2027. O HTML público contém a landing prerenderizada e mantém `noindex` para o protótipo.

A consulta HTTP do job sofreu timeout durante o build na VM de 1 GB; o worker continuou e foi acompanhado por SSH até concluir. O job não foi reenviado. O resolver local da VM ainda mantinha o resultado DNS anterior; a resolução pública e os testes HTTPS externos funcionaram. Não foi necessário reiniciar a infraestrutura.
