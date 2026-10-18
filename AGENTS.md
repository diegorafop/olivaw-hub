# Repository Guidelines

## Project Structure & Module Organization

Olivaw Hub is a monorepo containing Angular frontends, ASP.NET Core microservices and shared libraries.

- `ms-limpo-api` and `ms-limpo-ng` are the templates.
- `ms-olivaw-hub-api` owns internal administrative identity; `ms-devops-api` consumes its JWTs locally.
- Customer services such as `ms-ltmj-api` own separate databases, identities and JWT secrets. Their technical service accounts in the Hub are independent.
- `olivaw-identity` contains reusable local-authority and JWT-consumer libraries; `ms-olivaw-core-api` contains shared infrastructure.
- API projects, namespaces and assemblies use prefix `Owh`. Entities belong in the core project's `Domain`; interfaces belong in `Repositories`, `Services` and `Requests` as documented locally.
- Commands and queries use English CRUD names: Create, Read, Update, Delete. Identity tables use lowercase English names and internal int PK/FK; GUIDs are public ExternalId values.
- Migrations belong to providers and apply automatically at startup. Only identity authorities bootstrap root. Consumers never query the Hub to validate a token.

## Build, Test, and Development Commands

```bash
dotnet build OlivawHub.sln -c Release -m:1
dotnet test OlivawHub.sln -c Release --no-build
npm --prefix ms-ltmj-ng run build
python3 -m unittest discover -s scripts/deployment/tests -v
```

Read each module's `AGENTS.md` before modifying it. Generate API services through `bash ms-limpo-api/scripts/new-microservice.sh`; generated clients must have independent identity secrets. Document commands alongside changes and keep relative links valid.

## Coding Style & Naming Conventions

Keep existing project documentation in Portuguese and preserve the **Olivaw Hub** brand name. Use concise Markdown headings, short paragraphs, and hyphenated bullet lists. Wrap commands and paths in backticks, and use relative links for repository documents.

Follow the closest `.editorconfig`, language configuration and module instructions. Keep C# source in English and documentation in Portuguese.

## Testing Guidelines

Use the existing .NET, Angular and Python test frameworks. Cover security boundaries, schema contracts and changed behavior with meaningful tests. Avoid live customer databases or remote deployments during verification.

## Git Flow e rastreabilidade

Siga [docs/gitflow.md](docs/gitflow.md): master/develop e Git Flow clássico, usando `git flow feature start issue001-descricao`. `feature finish` integra em develop; `release finish` e `hotfix finish` integram a entrega conforme Git Flow e criam tags. PRs são opcionais.

Commits descrevem a alteração com `feat:`, `fix:`, `docs:`, `test:` ou `chore:`. Os hooks versionados acrescentam `Refs #N` com base na issue do branch; hotfixes versionadas usam a associação local descrita no guia. Ative-os por `python3 scripts/gitflow/setup.py` em cada clone. Não reescreva commits antigos para adicionar referências sem pedido explícito.

A Action registra branches nas issues e fecha as issues referenciadas apenas ao chegar código em master, após publicação. Não feche ao integrar em develop nem imponha PR para usar finish. Quando uma tarefa mencionar issue, confira o branch e preserve a rastreabilidade; finish, push, alterações remotas e implantação exigem autorização na tarefa.

## Security & Configuration

Por decisão explícita do mantenedor, os arquivos `.env` de todos os projetos são versionados no repositório privado nesta etapa, incluindo as configurações de serviço já presentes nas APIs. Não ignore esses arquivos nem introduza scripts para reconstruí-los em cada clone. A migração para Key Vault está planejada para o futuro. Essa decisão não autoriza incluir credenciais AWS, tokens pessoais, chaves SSH privadas ou dados de clientes no Git. Não imprima senhas ou chaves nos logs, na documentação ou nas respostas.
