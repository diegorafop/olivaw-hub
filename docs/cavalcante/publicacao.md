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

## Estado

Arquivos preparados localmente. Nenhum DNS, imagem remota, serviço em produção ou publicação foi alterado nesta tarefa. O endereço solicitado ainda precisa de implantação para servir esta versão.
