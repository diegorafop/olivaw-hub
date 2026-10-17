# Docker do site institucional

O projeto usa a mesma estrutura e o mesmo runtime do template Angular. Build e publicação são independentes da API.

Da raiz do monorepo:

```bash
docker compose -f ms-olivaw-hub-ng/docker/docker-compose.yml up --build -d
```

A porta local padrão é 8081; altere `FRONTEND_PORT` se outro frontend estiver usando essa porta. O Nginx serve somente arquivos estáticos e o shell CSR, sem proxy para a API. `GET /health` responde ao healthcheck.

Domínio previsto do site: `olivaw-hub.duckdns.org`. A API futura tem URL `https://api.olivaw-hub.duckdns.org`, configurada em `public/config.json`. O navegador acessa a API diretamente; ela deve permitir a origem do site via CORS.

O DNS está em `nginx.conf`, canonical e sitemap, mas gerar esses arquivos não registra DNS nem configura TLS. Hospedagem e TLS precisam ser preparados na infraestrutura antes de publicar. O formulário atual não chama a API e não envia dados.

Consulte o [guia do template](../../ms-limpo-ng/docker/README.md) para substituir a configuração pública via volume e validar o runtime. Não coloque segredos no settings.
