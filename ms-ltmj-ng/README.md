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

## Protótipo LTMJ Eventos

Projeto gerado por Schematics, preservando o padrão Angular standalone, SignalStore, Material, Tailwind e frontend separado da API. O estudo do cliente, as fontes e o plano de evolução estão em [briefing e MVP](docs/briefing-e-mvp.md). O protocolo reutilizável do Hub está em [descoberta de clientes](../docs/protocolo-descoberta-cliente.md).

A landing page apresenta serviços reais descritos no briefing, o logo obtido do perfil público e os links sociais oficiais. O assistente prepara um resumo local; não registra dados nem simula envio. Para ativar o link de WhatsApp, preencha o número comercial confirmado em `src/app/features/public/ltmj-content.ts`, com país e DDD, somente dígitos. O número ainda não foi informado.

Fotos de eventos ainda não foram fornecidas/recuperadas. O mural próprio tem filtros por ocasião e detalhes com galeria. Enquanto o acervo real não chega, exibe modelos identificados, com acesso às redes oficiais; nenhum modelo é apresentado como trabalho realizado. O logo e sua origem estão em `public/assets/ltmj/`.

Domínios técnicos propostos: `ltmj.olivaw-hub.duckdns.org` e `api.ltmj.olivaw-hub.duckdns.org`. Os cinco ambientes e dotenv acompanham o padrão do template; não houve publicação, registro DNS ou criação de banco. A área administrativa herdada ainda não gerencia eventos. Para publicar, preparar infraestrutura, autorizar o serviço no executor e obter aprovação do cliente.

Os testes E2E simulam a API futura e validam login, mobile e orçamento local. O protocolo de briefing em `docs/prototipos` da raiz é uma prova de conceito separada, não uma nova tela do site institucional.

Para execução simultânea com o site do Hub, use `npm start -- --port 4201`. No Docker local, use `FRONTEND_PORT=8083` para evitar disputar a porta 8081. Os hosts e valores de publicação são propostas técnicas e não representam domínio já aprovado pelo cliente.

O [índice da documentação do cliente](docs/README.md) reúne o briefing inicial e o histórico de conversas/decisões, atualizado a cada nova informação relevante.
