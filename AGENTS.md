# Repository Guidelines

## Project Structure & Module Organization

Olivaw Hub is currently a planning repository for technology services aimed at small businesses. Implementation has not started.

- `README.md` introduces the project and links to its initial context.
- `BRIEFING.md` records the business proposal, intended technical direction, and unresolved decisions.

There are no source, test, or asset directories yet. Angular templates, an API template, schematics, and AWS deployment are proposed directions, not implemented components. Document agreed architecture before introducing a module layout or infrastructure.

## Build, Test, and Development Commands

No build system, dependency manifest, development server, or automated test commands exist yet. Do not assume commands such as `npm install`, `npm test`, or `ng serve` are available.

For documentation changes, inspect the rendered Markdown and check that relative links resolve, especially the link from `README.md` to `BRIEFING.md`. When adding executable code, document its installation, local development, build, and test commands alongside the implementation.

## Coding Style & Naming Conventions

Keep existing project documentation in Portuguese and preserve the **Olivaw Hub** brand name. Use concise Markdown headings, short paragraphs, and hyphenated bullet lists. Wrap commands and paths in backticks, and use relative links for repository documents.

No programming-language style, indentation standard, formatter, or linter has been established. Choose and document these when introducing the first implementation; follow the resulting configuration consistently.

## Testing Guidelines

There is no test framework or coverage requirement yet. Review documentation for factual consistency and clearly distinguish confirmed decisions from proposals. For future executable components, introduce relevant tests and document the framework, test naming convention, and execution command in the same change.

## Git Flow e rastreabilidade

Siga [docs/gitflow.md](docs/gitflow.md): master/develop e Git Flow clássico, usando `git flow feature start issue001-descricao`. `feature finish` integra em develop; `release finish` e `hotfix finish` integram a entrega conforme Git Flow e criam tags. PRs são opcionais.

Commits descrevem a alteração com `feat:`, `fix:`, `docs:`, `test:` ou `chore:`. Os hooks versionados acrescentam `Refs #N` com base na issue do branch; hotfixes versionadas usam a associação local descrita no guia. Ative-os por `python3 scripts/gitflow/setup.py` em cada clone. Não reescreva commits antigos para adicionar referências sem pedido explícito.

A Action registra branches nas issues e fecha as issues referenciadas apenas ao chegar código em master, após publicação. Não feche ao integrar em develop nem imponha PR para usar finish. Quando uma tarefa mencionar issue, confira o branch e preserve a rastreabilidade; finish, push, alterações remotas e implantação exigem autorização na tarefa.

## Security & Configuration

Por decisão explícita do mantenedor, os arquivos `.env` de todos os projetos são versionados no repositório privado nesta etapa, incluindo as configurações de serviço já presentes nas APIs. Não ignore esses arquivos nem introduza scripts para reconstruí-los em cada clone. A migração para Key Vault está planejada para o futuro. Essa decisão não autoriza incluir credenciais AWS, tokens pessoais, chaves SSH privadas ou dados de clientes no Git. Não imprima senhas ou chaves nos logs, na documentação ou nas respostas.
