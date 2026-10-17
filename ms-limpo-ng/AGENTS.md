# Diretrizes — Template Angular

## Arquitetura

Preserve componentes standalone, TypeScript/templates estritos e carregamento por funcionalidade. `core` contém infraestrutura, `shared` contém blocos sem regras de negócio e `features` contém funcionalidades. Não acrescente menu obrigatório nem CRUD de um cliente ao template.

O par padrão é `ms-limpo-ng` / `ms-limpo-api`, com serviços, imagens, builds e publicações independentes. A URL absoluta da API fica em `public/config.json`; o navegador chama a API diretamente. Não adicione proxy Angular/Nginx nem prefixo `/api` para juntar os serviços. Use os contratos existentes de identidade; mantenha o estado compartilhado `autenticacao` em SignalStore. Refresh token e persistência de sessão não fazem parte da etapa inicial. Guards e apresentação por permissão não substituem autorização na API.

A raiz pública é prerenderizada; administração e login usam CSR. Não use APIs do navegador no prerender sem verificar a plataforma. Nginx serve o resultado estático; alterar isso para SSR requer revisar Docker e publicação.

## Configuração e estilos

`public/config.json` é público e carregado em runtime; nunca inclua segredos. Os tokens visuais ficam em `src/theme/_brand.scss`, com utilitários Tailwind referenciando Material. Use as APIs públicas de tema do Material, evitando seletores internos de componentes. Preserve a integração sem Preflight.

Use dois espaços, Prettier, ESLint e mensagens/documentação em português. Novas funcionalidades de negócio devem seguir a convenção de inglês documentada no monorepo; os contratos de identidade atuais e o estado `autenticacao` preservam os nomes existentes/acordados.

## Validação

Os comandos estão no README. Testes unitários ficam em `*.spec.ts` próximos ao código; E2E usam Playwright em `e2e/`. Simule contratos HTTP nos testes sem credenciais reais. Alterações de infraestrutura devem preservar fallback CSR, HTML público prerenderizado, 404 para assets ausentes e configuração sem cache persistente.

Não publique imagens, implante, faça finish ou push como efeito da validação local.
