# Diretrizes — Frontend Angular derivado

## Arquitetura

Preserve componentes standalone, TypeScript/templates estritos e carregamento por funcionalidade. `core` contém infraestrutura, `shared` contém blocos sem regras de negócio e `features` contém funcionalidades. Implemente funcionalidades específicas em `features`, preservando os padrões herdados.

O par padrão é `ms-olivaw-ng` / `ms-limpo-api`, com serviços, imagens, builds e publicações independentes. A URL absoluta da API fica em `public/config.json`; o navegador chama a API diretamente. Não adicione proxy Angular/Nginx nem prefixo `/api` para juntar os serviços. Use os contratos existentes de identidade; mantenha o estado compartilhado `autenticacao` em SignalStore. Refresh token e persistência de sessão não fazem parte da etapa inicial. Guards e apresentação por permissão não substituem autorização na API.

A raiz pública é prerenderizada; administração e login usam CSR. Não use APIs do navegador no prerender sem verificar a plataforma. Nginx serve o resultado estático; alterar isso para SSR requer revisar Docker e publicação.

## Configuração e estilos

`public/config.json` é público e carregado em runtime; nunca inclua segredos. Os tokens visuais ficam em `src/theme/_brand.scss`, com utilitários Tailwind referenciando Material. Use as APIs públicas de tema do Material, evitando seletores internos de componentes. Preserve a integração sem Preflight.

Use dois espaços, Prettier, ESLint e mensagens/documentação em português. Novas funcionalidades de negócio devem seguir a convenção de inglês documentada no monorepo; os contratos de identidade atuais e o estado `autenticacao` preservam os nomes existentes/acordados.

## Validação

Os comandos estão no README. Testes unitários ficam em `*.spec.ts` próximos ao código; E2E usam Playwright em `e2e/`. Simule contratos HTTP nos testes sem credenciais reais. Alterações de infraestrutura devem preservar fallback CSR, HTML público prerenderizado, 404 para assets ausentes e configuração sem cache persistente.

Não publique imagens, implante, faça finish ou push como efeito da validação local.

## Atualização de dependências

Preserve compatibilidade entre Angular, CLI, Material/CDK, TypeScript e Node. Angular 22.2 requer TypeScript 6.0.x; não instalar TypeScript 7 apenas por ser a versão latest. Os tipos do Node acompanham o runtime 24 usado pelo projeto. Atualize também o lockfile e valide build, lint, testes unitários e E2E.

O `overrides.vite` fixa o patch 8.3.5 para corrigir avisos da versão 8.3.2 exigida pelo Angular Build 22.2.3. Reavalie o override em futuras atualizações e remova quando a cadeia oficial incorporar uma versão corrigida equivalente; não mantenha um pin antigo sem consultar auditoria e compatibilidade.

## Navegação da operação

- A navegação principal usa Visão geral, Leads e Configuração. Cadastros e ferramentas internas ficam nos cards de `/admin/configuration`, sem novos menus ou submenus laterais para cada funcionalidade.
- Ao acrescentar um cadastro, acrescente seu card ao hub e mantenha a autorização da rota e da API. O menu Configuração permanece ativo ao navegar pelos cadastros.
- O breadcrumb acompanha os níveis de cada rota e mantém os ancestrais clicáveis: Visão geral, Configuração, cadastro e ficha/criação. O item atual usa `aria-current="page"`; query strings não alteram a hierarquia. Implantações retornam a Serviços, sem criar um link para uma lista de ambientes inexistente.
- Preserve navegação por teclado, cards responsivos e acesso de volta à Configuração. Operadores sem root não veem nem acessam os módulos restritos.
- Estudos de planos são cenários internos, com limites e funcionalidades em elaboração; não os apresente como ofertas aprovadas nem altere contratos ao salvar cenários.
