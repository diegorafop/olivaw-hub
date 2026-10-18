# ms-limpo-ng

Template Angular do **Olivaw** para produtos com site público e administração. Não contém funcionalidades de negócio, cadastro de eventos, upload de fotos ou agenda. Essas funcionalidades serão implementadas em cada cliente, com contratos próprios de API.

## Arquitetura

- Componentes standalone, TypeScript e templates estritos, SCSS com dois espaços e formatação pelo Prettier.
- Signals para estado local, NgRx SignalStore para o estado compartilhado `autenticacao` e RxJS para HTTP.
- `src/app/features`: funcionalidades e páginas carregadas sob demanda.
- `src/app/core`: configuração, autenticação e infraestrutura HTTP.
- `src/app/shared`: blocos reutilizáveis sem regras de negócio.
- `src/theme/_brand.scss`: paletas e tipografia em tempo de build. Tailwind usa os tokens do Material em `src/tailwind.css`, sem aplicar Preflight sobre seus componentes.
- Raiz com `router-outlet` e feedback HTTP, sem menu obrigatório ou layout administrativo fixo.

A rota `/` é prerenderizada no build e hidratada no navegador. `/login`, `/admin` e demais rotas usam renderização no navegador. O resultado é estático: Nginx serve a landing page e o shell `index.csr.html` das rotas internas; não há servidor Node em produção. A administração não recebe dados privados durante o build.

A página pública inicial é um ponto de extensão. Conteúdo incluído no HTML prerenderizado só muda com novo build. Para eventos, agenda e fotos publicados pela administração, defina no projeto consumidor se os dados serão consultados no navegador ou se a publicação disparará novo build. Se cada conteúdo precisar de HTML atualizado para indexação imediata, avalie SSR com runtime Node: o runtime Nginx deste template não executa SSR nem regeneração incremental.

## Preparação

Use Node 24 LTS e npm 11; `.nvmrc`, `engines` e `package-lock.json` registram a base de ferramentas e dependências. Consulte as versões instaladas com `npm ls --depth=0`. Angular CLI gerou a base com:

```bash
npx @angular/cli@22.2.3 new ms-limpo-ng --routing --style scss --strict --standalone --ssr --skip-git --skip-install --defaults
```

A partir desta pasta:

```bash
npm ci
npm start
```

Abra `http://localhost:4200`. O frontend inicia sozinho e não exige backend para servir páginas. Para login e funcionalidades autenticadas, execute o `ms-limpo-api` separadamente. O settings padrão usa `http://localhost:5137`, a porta HTTP do perfil local da API. Ajuste essa URL se iniciar a API em outra porta.

## Configuração e identidade visual

`ms-limpo-ng` e `ms-limpo-api` são serviços independentes que formam o par padrão de frontend/backend. `public/config.json` é o settings público do frontend:

```json
{
  "apiUrl": "http://localhost:5137"
}
```

O navegador chama essa URL diretamente, por exemplo `http://localhost:5137/identity/authentication/login`. Não existe proxy de API no Angular ou no Nginx. Em produção, configure a URL pública HTTPS da API, por exemplo `https://api.cliente.example.com`. O endereço deve ser acessível ao navegador do usuário; nomes internos Docker como `ms-limpo-api` não servem para isso.

A configuração é carregada antes das rotas, sem cache e sem passar pelos interceptors. A URL deve ser absoluta e usar HTTP(S), sem credenciais, query ou fragmento. Arquivo ausente ou inválido impede a inicialização. A API precisa permitir a origem pública do frontend via CORS, incluindo métodos utilizados e os headers `Authorization` e `Content-Type`. Não é necessário compartilhar rede Docker entre frontend e API, pois a chamada parte do navegador. A configuração CORS do backend deve ser revisada na implantação; este template não altera a API.

No Docker, `API_URL` no dotenv do ambiente é transformada em settings públicos na inicialização do container, sem recompilar a imagem. `public/config.json` continua sendo a configuração de `npm start`. Nunca inclua chaves JWT, senhas, tokens ou qualquer segredo nesses settings. Veja [Docker e ambientes](docker/README.md).

Para personalizar um cliente, altere as paletas e tipografia em `src/theme/_brand.scss`, a marca e textos em `features/public`, o título/descrição em `src/index.html` e os títulos de rotas. Adicione seus assets em `public/`. Material atende componentes e acessibilidade; Tailwind atende layout e espaçamento. As cores utilitárias referenciam os tokens do Material.

Idioma inicial: `pt-BR`. Pipes de datas e números usam esse locale; declare `BRL` ao usar moeda. Preserve labels, foco visível e navegação por teclado ao criar novos componentes.

## Autenticação

Integração com os contratos atuais do [Olivaw Identity](../docs/identidade.md):

- `POST /identity/authentication/login`: `{ login, senha }` → `{ accessToken, expiresAt }`.
- `GET /identity/authentication/acesso`: `{ usuarioId, nome, root, autorizacoes }`.
- `POST /identity/authentication/logout`: corpo `{}`; o backend revoga todas as sessões do usuário.

O prefixo da configuração pública é adicionado a esses caminhos. O estado `AuthenticationStore.autenticacao` contém token, validade e usuário com permissões somente após login e consulta de acesso bem sucedidos. Fica em memória; recarregar a página exige novo login. Não usa localStorage, cookies, persistência de senha ou refresh token.

O token acompanha apenas a origem e o prefixo configurados da API. Expiração e respostas 401 autenticadas encerram a sessão local. Logout tenta revogar a sessão no servidor e limpa o estado mesmo quando a API falha; nesse caso a revogação remota não é garantida. Permissões são um retrato da consulta de acesso, e a API continua responsável por validar cada operação.

`authGuard` protege a árvore `/admin`. Para uma funcionalidade com permissão, adicione `permissionGuard` e `data: { permissao: 'codigo.real.da.api' }` à rota. Para apresentação condicional, use `store.permite('codigo.real.da.api')` em um bloco `@if`. Esses controles de interface não substituem autorização na API.

## HTTP

`HttpState` apresenta loading global com contador de concorrência e um aviso de erro dispensável. O contador é liberado também em erro e cancelamento. Os interceptors interpretam `ProblemDetails` e `ValidationProblemDetails` da identidade, além de falhas de conexão e HTTP 401/403/429/5xx. Cada funcionalidade pode usar `HttpContext`:

- `SKIP_AUTH`: não anexar o token automaticamente.
- `SKIP_LOADING`: operação silenciosa ou com indicador próprio.
- `LOCAL_ERROR`: tratar o erro na tela sem repetir o aviso global.

Login usa feedback local. Não há serviço genérico de CRUD nem contrato de paginação inventado; incorpore os DTOs reais da funcionalidade consumida quando ela for criada.

## Qualidade

```bash
npm run lint
npm run format:check
npm test
npm run build
npx playwright install chromium
npm run e2e
```

`npm run format` aplica o Prettier; `npm run test:watch` acompanha testes unitários. Arquivos unitários usam `*.spec.ts` ao lado da implementação e Vitest pelo Angular CLI. E2E ficam em `e2e/*.spec.ts`, usam Playwright/Chromium e simulam a API sem credenciais reais. Cobrem proteção de rota, login, acesso, logout e erro de credenciais. Unitários cobrem restrição do token, concorrência/cancelamento, sessão expirada, permissões e erro global.

A [CI](../.github/workflows/frontend.yml) executa essas verificações, constrói a imagem e valida rotas e assets no Nginx. Não publica imagem nem implanta. Publicação futura deve seguir o Git Flow e a infraestrutura do Olivaw, com destino e credenciais definidos separadamente.

## Docker

Da raiz do monorepo:

```bash
docker compose -f ms-limpo-ng/docker/docker-compose.yml \
  -f ms-limpo-ng/docker/docker-compose.override.yml up --build -d
```

No override local, o site fica em `http://localhost:8081`; `GET /health` atende o healthcheck. A imagem usa Node somente no build e Nginx sem root na porta 8080. O contexto de build é apenas `ms-limpo-ng`, pois não há referências a projetos .NET.

O frontend tem imagem, Compose, build, configuração e publicação próprios. Os cinco ambientes `dev/qas/rls/stg/prd` seguem o padrão das APIs, com dotenv preenchido, rede e routers Traefik isolados por ambiente. Os dotenv públicos são versionados e acompanham cada clone, sem preparação adicional. O Nginx entrega somente os arquivos do frontend; não encaminha requisições ao backend. Publique o `ms-limpo-api` separadamente, configure sua URL pública no settings e libere a origem do frontend no CORS da API. Consulte [Docker](docker/README.md) para configuração e validação.

## Novos clientes com Angular Schematics

A collection local `schematics/collection.json` oferece o schematic `application`, executado pelo toolkit oficial Angular Schematics. A raiz do template é a fonte dos arquivos; não existe uma segunda cópia para manter. O gerador lê código, tema, configuração, testes e Docker atuais, personaliza os identificadores e cria uma pasta irmã na raiz do monorepo.

Depois de instalar as dependências do template, execute da raiz do monorepo:

```bash
npm --prefix ms-limpo-ng run new:client -- --name=ms-cliente-ng --api-url=https://api.cliente.example.com --site-domain=cliente.example.com
```

Use `--dry-run` para simular. O nome deve seguir `ms-<cliente>-ng`. `--api-url` aceita URL absoluta HTTP(S), com padrão local `http://localhost:5137`; `--site-domain` é opcional e recebe DNS sem protocolo. Quando informado, configura canonical, Nginx, robots e sitemap. O domínio não é registrado nem publicado pelo gerador.

Uma pasta existente pode conter documentos: eles são preservados. Se algum arquivo a gerar já existir, o processo recusa a operação antes de gravar arquivos; não há exclusão, overwrite ou modo force. O schematic pode ser conferido com `npm --prefix ms-limpo-ng run test:schematics`. Projetos já gerados não são atualizados por esse comando.

O gerador não transporta dependências instaladas, caches, build, segredos, preferência de analytics nem o próprio toolkit de geração. Ele preserva assets binários e personaliza os dotenv públicos versionados dos cinco ambientes, seguindo o padrão de ambientes versionados no repositório privado. Os testes E2E passam a simular a URL configurada da API. Depois de gerar, execute `npm ci` na nova pasta. Cada cliente mantém build e publicação independentes; evoluções do template não são propagadas automaticamente.

Documentação oficial: [Angular Schematics](https://angular.dev/tools/cli/schematics) e [autoria de schematics](https://angular.dev/tools/cli/schematics-authoring).
