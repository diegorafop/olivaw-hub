# Repository Guidelines

## Project Structure & Module Organization

Olivaw is a monorepo containing Angular frontends, ASP.NET Core microservices and shared libraries.

- `ms-limpo-api` and `ms-limpo-ng` are the templates.
- `ms-olivaw-api` owns internal administrative identity; `ms-devops-api` consumes its JWTs locally.
- Customer services such as `ms-ltmj-api` own separate databases, identities and JWT secrets. Their technical service accounts in the Olivaw are independent.
- `olivaw-identity` contains reusable local-authority and JWT-consumer libraries; `ms-olivaw-core-api` contains shared infrastructure.
- API projects, namespaces and assemblies use prefix `Owh`. Entities belong in the core project's `Domain`; interfaces belong in `Repositories`, `Services` and `Requests` as documented locally.
- Commands and queries use English CRUD names: Create, Read, Update, Delete. Identity tables use lowercase English names and internal int PK/FK; GUIDs are public ExternalId values.
- Migrations belong to providers and apply automatically at startup. Only identity authorities bootstrap root. Consumers never query the Olivaw to validate a token.

## Build, Test, and Development Commands

```bash
dotnet build Olivaw.sln -c Release -m:1
dotnet test Olivaw.sln -c Release --no-build
npm --prefix ms-ltmj-ng run build
python3 -m unittest discover -s scripts/deployment/tests -v
```

Read each module's `AGENTS.md` before modifying it. Generate API services through `bash ms-limpo-api/scripts/new-microservice.sh`; generated clients must have independent identity secrets. Document commands alongside changes and keep relative links valid.

## Repositórios de aplicação

- Leituras e filtros comuns devem usar `IRepository<Entidade>` diretamente no QueryHandler, por exemplo `IRepository<UserProfile>`. Não criar repositório específico para encapsular uma consulta comum.
- Criar uma interface específica somente quando a consulta exigir operações ausentes no contrato genérico, como projeção JSON com paginação e contagem no banco. O nome deve seguir o prefixo `IRepository` e a entidade: `IRepositoryLead`, implementada por `RepositoryLead`, herdando os contratos e a implementação genéricos quando aplicável.

## Coding Style & Naming Conventions

Keep existing project documentation in Portuguese and preserve the **Olivaw** brand name. Use concise Markdown headings, short paragraphs, and hyphenated bullet lists. Wrap commands and paths in backticks, and use relative links for repository documents.

Follow the closest `.editorconfig`, language configuration and module instructions. Keep C# source in English and documentation in Portuguese.

## Testing Guidelines

Use the existing .NET, Angular and Python test frameworks. Cover security boundaries, schema contracts and changed behavior with meaningful tests. Avoid live customer databases or remote deployments during verification.

## Git Flow e rastreabilidade

Siga [docs/gitflow.md](docs/gitflow.md): master/develop e Git Flow clássico, usando `git flow feature start issue001-descricao`. `feature finish` integra em develop; `release finish` e `hotfix finish` integram a entrega conforme Git Flow e criam tags. PRs são opcionais.

Commits descrevem a alteração com `feat:`, `fix:`, `docs:`, `test:` ou `chore:`. Os hooks versionados acrescentam `Refs #N` com base na issue do branch; hotfixes versionadas usam a associação local descrita no guia. Ative-os por `python3 scripts/gitflow/setup.py` em cada clone. Não reescreva commits antigos para adicionar referências sem pedido explícito.

A Action registra branches nas issues e fecha as issues referenciadas apenas ao chegar código em master, após publicação. Não feche ao integrar em develop nem imponha PR para usar finish. Quando uma tarefa mencionar issue, confira o branch e preserve a rastreabilidade; finish, push, alterações remotas e implantação exigem autorização na tarefa.

## Security & Configuration

Por decisão explícita do mantenedor, os arquivos `.env` de todos os projetos são versionados no repositório privado nesta etapa, incluindo as configurações de serviço já presentes nas APIs. Contas técnicas do Olivaw usadas exclusivamente para deployment também ficam em `deployment/olivaw-credentials.env` dentro de cada serviço; o arquivo deve ser excluído de contextos Docker. A migração para Key Vault está planejada para o futuro. Por exceção explícita adicional do mantenedor em 18/10/2026, os arquivos já presentes em `docs/secrets/`, incluindo `aws.dat`, `clients.dat`, `fintalk-key.pem` e `registrobr.dat`, também permanecem versionados temporariamente neste repositório privado até a migração para Key Vault. Não remova esses arquivos do índice nem acrescente a pasta ao `.gitignore` sem nova solicitação. A exceção fica limitada a esses arquivos e não autoriza incluir outros segredos ou dados de clientes. Não imprima senhas ou chaves nos logs, na documentação ou nas respostas.

## Padronização de migrations

- Gere migrations somente mediante pedido explícito; `new feature` não inclui essa etapa.
- Prefira uma única migration por issue, gerada ao final após estabilizar o modelo e validar a feature. Preserve migrations já publicadas; não consolide nem reescreva históricos existentes para cumprir essa preferência.
- Confira a branch atual. Use a issue em PascalCase como nome: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se já existir uma migration com esse nome, preserve o histórico e acrescente um sufixo descritivo, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome com o mantenedor.
- Gere somente para os provedores solicitados, no projeto do provedor da API consumidora. O Core não contém migrations. Não copie históricos entre serviços.
- Use a factory de design time quando disponível, sem iniciar o host nem acessar bancos reais. Compile com `-m:1`; use `--no-build` no EF somente após compilar o modelo atual. `--no-restore` pressupõe dependências restauradas.
- Revise migration, designer, snapshot e SQL entre a migration anterior e a nova. Preserve BOM UTF-8 e CRLF dos arquivos C#. Não altere migrations antigas já publicadas.
- Gerar e revisar não autoriza `database update`, conexão a bancos reais ou redeploy. O startup das APIs aplica as migrations pendentes; registre essa consequência antes da publicação.
- Documente comandos reproduzíveis no README da API e consulte os [comandos do Core](ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores). Informe as validações executadas e os provedores não contemplados.

## Localização do domínio

- Projetos com sufixo `.Api` não podem conter pasta `Domain`. Entidades, objetos de valor e invariantes ficam no projeto de domínio correspondente, como `Owh.Olivaw/Domain` ou `Owh.Ltmj/Domain`.
- Modelos internos da API ficam junto às features, em `Internals` quando necessário; contratos públicos ficam no projeto cliente quando existente. Preserve interfaces de domínio nas pastas `Repositories`, `Services` e `Requests` do núcleo.
- O template `ms-limpo-api` e os serviços gerados devem preservar essa organização.

## Limites de versões por licenciamento

Por decisão explícita do mantenedor, manter **MassTransit e seus transportes em 8.5.x** e **MediatR em 12.5.x**, preservando as linhas escolhidas para uso gratuito. Não atualizar essas famílias para outra minor/major, nem adicionar pacote que exija versões fora desses intervalos, sem nova autorização explícita.

- Usar versões estáveis explícitas nos projetos; alinhar todos os pacotes `MassTransit.*` na mesma versão 8.5.x.
- Conferir dependências transitivas após restore, além das referências diretas. Não usar atualização automática irrestrita para contornar os limites.
- A regra inclui Core, identidade, APIs, testes, template e serviços gerados. Ao atualizar dependências, verificar também compatibilidade entre EF Core, provedores e frameworks.
