# ms-cavalcante-ng

Frontend Angular da **Cavalcante Terraplenagem e Locação**, gerado do template `ms-limpo-ng` do **Olivaw**, issue #24. Protótipo público estático, sem novas funcionalidades de API.

## Executar

Node 24 e npm 11. Dentro desta pasta:

```bash
npm ci
npm start
npm run build
npm run lint
npm run format:check
npm test
npm run e2e
```

Acesse `http://localhost:4200`. A landing não precisa do backend. Fotos locais do site do cliente, filtros por aplicação, navegação móvel, FAQ e links contextuais de WhatsApp. Contatos reais são abertos somente quando o visitante aciona o link; testes não enviam mensagens.

## Organização

- `src/app/features/public`: landing, conteúdo estático e Signals.
- `src/theme/_brand.scss`: cores do cliente.
- `public/tractor.svg` e `public/favicon.svg`: avatar e favicon originais em SVG.
- `public/images/sources.json`: procedência das fotos; direitos de reutilização a confirmar antes de publicação.
- `public/config.json`: URL pública da API, sem segredos.
- Login, administração e autenticação são infraestrutura herdada, sem produto administrativo implementado.

`/` é prerenderizada; login/admin permanecem CSR. Os dados estáticos mudam com rebuild. O protótipo usa `noindex,nofollow` e `robots.txt` bloqueado; a liberação para indexação exige versão oficial aprovada.

## Publicação

Endereço definido: **https://cavalcante.olivaw.com.br**. API reservada: `https://api.cavalcante.olivaw.com.br`. Host, TLS do Traefik, Nginx e canonical estão alinhados. Não há proxy para API. Os ambientes e Docker preservam o fluxo do template.

Ver [preparação da publicação](../docs/cavalcante/publicacao.md), [estudo e planejamento](../docs/cavalcante/README.md), [validação](../docs/cavalcante/validacao.md) e [template Angular](../ms-limpo-ng/README.md). O protótipo foi publicado com HTTPS validado; veja o registro e o job no guia de publicação. A API permanece sem implantação.
