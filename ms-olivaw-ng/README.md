# ms-olivaw-ng

Frontend gerado do template **Olivaw**, com build e publicação independentes da API.

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

Settings públicos: `public/config.json`. Tema: `src/theme/_brand.scss`. Site: https://olivaw-hub.duckdns.org. API: https://api.olivaw-hub.duckdns.org.

Docker: `docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d`. Configure CORS na API para a origem do frontend. Não há proxy de API, refresh token nem persistência da sessão. Consulte o [template](../ms-limpo-ng/README.md) para os padrões compartilhados. Arquivos existentes no destino são preservados; a geração recusa conflitos e não atualiza projetos já gerados.

## Landing page institucional

A landing page foi implementada como componentes Angular em `src/app/features/public`, a partir do template. `docs/prot_tipo_da_landing_page.html` é apenas referência visual. As decisões atuais estão em [Implementação](docs/implementacao.md).

- Site: `https://olivaw-hub.duckdns.org`.
- API futura e independente: `https://api.olivaw-hub.duckdns.org` em `public/config.json`.
- Não há endpoint de contato implementado. O resumo de projeto é copiado localmente; nada é enviado ou armazenado.
- Telefone, e-mail, redes sociais, demonstrações e clientes não são inventados. Confirmar esses conteúdos antes de divulgar o canal de atendimento.
- Material permanece nos formulários e Tailwind no layout, conforme o padrão universal do template. A marca é configurada em `src/theme/_brand.scss`.
- DNS, TLS, hospedagem e API não foram criados/publicados por essa geração.

Os E2E simulam a API futura; não dependem de esse domínio responder. A raiz é prerenderizada e inclui o conteúdo institucional no HTML do build. Não há clientes ou resultados fictícios publicados.

## Ambientes AWS

Docker segue o padrão `dev/qas/rls/stg/prd` das APIs, com configurações públicas por ambiente e Traefik em HTTPS. Consulte [Docker e ambientes](docker/README.md) para executar o deploy com os dotenv já versionados. A URL da API é configurada por `API_URL` no container; `public/config.json` é usado no desenvolvimento com Angular CLI.
