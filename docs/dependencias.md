# Dependências e limites de atualização

## Escopo

Atualização conferida em 22/10/2026 no monorepo: Core, identidade, APIs Olivaw/DevOps/LTMJ, templates de API e Angular e os três frontends. Projetos externos ao checkout não fazem parte desta atualização. Foram atualizadas dependências, não versões de publicação dos pacotes próprios, bancos ou ambientes remotos.

## Linhas mantidas por decisão do mantenedor

- MassTransit e seus transportes: **8.5.x**, alinhados em **8.5.12**.
- MediatR: **12.5.x**, mantido em **12.5.0**.
- Não atualizar essas famílias para outra minor/major, nem introduzir dependência transitiva que viole os limites, sem nova autorização explícita. A restrição foi registrada no `AGENTS.md` da raiz, do Core, da infraestrutura API do Core, da identidade, do template e das APIs.

Os pacotes efetivamente resolvidos no restore também foram conferidos, incluindo `MassTransit.Abstractions`. A regra se aplica aos serviços gerados pelo template.

## Principais versões verificadas

| Família                                                   | Versão selecionada                 |
| --------------------------------------------------------- | ---------------------------------- |
| ASP.NET Core, EF Core e bibliotecas Microsoft atualizadas | 10.0.13                            |
| FluentValidation e integração de DI                       | 12.2.0                             |
| OpenTelemetry Hosting e instrumentação ASP.NET Core       | 1.20.1                             |
| Swashbuckle e Swagger UI                                  | 10.3.1                             |
| Oracle EF Core, onde já referenciado                      | 10.23.26400                        |
| Microsoft.NET.Test.Sdk                                    | 18.11.0                            |
| Moq                                                       | 4.21.0                             |
| Angular, CLI, Build e SSR                                 | 22.2.3                             |
| Angular Material e CDK                                    | 22.2.4                             |
| TypeScript                                                | 6.0.3                              |
| ESLint / angular-eslint / typescript-eslint               | 10.13.0 / 22.6.0 / 8.72.0          |
| Playwright                                                | 1.64.1                             |
| Tailwind CSS e integração PostCSS                         | 4.3.3                              |
| Vitest                                                    | 5.0.5                              |
| Tipos do Node                                             | 24.19.2, acompanhando o runtime 24 |
| npm declarado em `packageManager`                         | 11.21.1                            |

Outras referências foram conferidas nos registros oficiais; pacotes que já estavam na versão estável selecionada foram mantidos. As versões exatas estão nos `.csproj`, `package.json` e lockfiles. TypeScript 7 não foi adotado porque Angular 22.2 e typescript-eslint exigem TypeScript anterior a 6.1. Tipos do Node 26 não foram escolhidos para um runtime 24.

Referências: [compatibilidade Angular](https://angular.dev/reference/versions), [NuGet](https://www.nuget.org/) e [registro npm](https://registry.npmjs.org/).

## Patch transitivo do Vite

Angular Build 22.2.3 exigia Vite 8.3.2, apontado pela auditoria. Os três frontends usam `overrides.vite = "8.3.5"`, um patch da mesma linha. Não foi usado `npm audit fix --force`, nem downgrade de Angular para contornar o aviso.

O override deve ser reavaliado nas próximas atualizações e removido quando a cadeia oficial incorporar uma versão corrigida equivalente. Builds, testes e execução do servidor de desenvolvimento foram verificados com o patch.

## Validação realizada

- Restore e build da solução .NET em Release, com `-m:1`: sem erros ou warnings de compilação.
- 576 testes .NET aprovados: Core, identidade e as quatro APIs, usando dados locais e bancos de teste isolados.
- Build, lint, testes unitários e verificação de formatação nos três frontends. Avisos já existentes de tamanho dos estilos públicos permanecem documentados nos respectivos builds.
- 45 testes unitários Angular e sete testes do gerador Angular aprovados.
- 48 cenários de navegador aprovados entre template, Olivaw e LTMJ, com APIs simuladas. A imagem social foi regenerada e seu teste específico repetido após a correção do arquivo ausente.
- Os três pacotes NuGet do Core foram gerados localmente em `/tmp/olivaw-updated-core-packages`, sem publicação. As versões próprias dos pacotes foram preservadas.
- Auditoria NuGet dos 40 projetos, incluindo transitivas, sem vulnerabilidades reportadas no momento da consulta. Instalação/auditoria npm dos três frontends também sem vulnerabilidades reportadas após o patch do Vite. Auditorias são temporais e precisam ser repetidas em futuras entregas.

## Comandos de manutenção

Na raiz:

```bash
dotnet restore Olivaw.sln --disable-parallel
dotnet build Olivaw.sln -c Release --no-restore -m:1
dotnet test Olivaw.sln -c Release --no-build --no-restore -m:1
dotnet list Olivaw.sln package --vulnerable --include-transitive
```

Em cada frontend, `ms-limpo-ng`, `ms-olivaw-ng` e `ms-ltmj-ng`:

```bash
npm ci
npm run build
npm run lint
npm test
npm run format:check
npx playwright install chromium
npm run e2e
npm audit
```

O template também oferece `npm run test:schematics`. Para testar os três servidores simultaneamente, use portas distintas; o comando padrão de cada projeto pode compartilhar porta com outro serviço local.

Não gerar migrations, acessar bancos reais, publicar pacotes ou executar deploy como efeito colateral de uma atualização de dependências.
