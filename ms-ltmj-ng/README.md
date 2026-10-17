# ms-ltmj-ng

Frontend gerado do template **Olivaw Hub**, com build e publicação independentes da API.

## Desenvolvimento

Node 24 e npm 11.

```bash
npm ci
npm start
npm run build
npm test
npm run lint
npm run format:check
npx playwright install chromium
npm run e2e
```

Settings públicos: `public/config.json`. Tema: `src/theme/_brand.scss`. Site: https://ltmj.olivaw-hub.duckdns.org. API: https://api.ltmj.olivaw-hub.duckdns.org.

Docker: `docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d`. Configure CORS na API para a origem do frontend. Não há proxy de API, refresh token nem persistência da sessão. Consulte o [template](../ms-limpo-ng/README.md) para os padrões compartilhados. Arquivos existentes no destino são preservados; a geração recusa conflitos e não atualiza projetos já gerados.
