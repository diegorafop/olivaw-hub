# ms-devops-api

API .NET 10 do Olivaw Hub, gerada a partir de `ms-limpo-api`. A estrutura inicial está pronta; as funcionalidades específicas de DevOps ainda serão definidas.

## Estrutura

- `Devops.sln`: solução local do serviço.
- `src/Mps.Devops.Api`: host HTTP, composição, readiness, versão e feature de exemplo herdada do template.
- `src/Mps.Devops`: domínio e contratos específicos do serviço.
- `src/Mps.Devops.Dados`: contexto, repositórios e mapeamentos locais.
- `src/Mps.Devops.Dados.PostgreSql` e `.SqlServer`: projetos de provedor para as migrations do serviço.
- `src/Mps.Devops.Client`: biblioteca base para futuro cliente REST.
- `tests/Mps.Devops.Tests`: testes herdados do template, incluindo integração de identidade.

Oracle permanece reservado, sem integração validada. Core e identidade são importados por `ProjectReference` das pastas irmãs do monorepo. As funcionalidades de autenticação não são copiadas para o serviço; seu `WriteDbContext` aplica os mapeamentos compartilhados no banco próprio.

## Desenvolvimento

Na raiz do monorepo:

```bash
dotnet build ms-devops-api/Devops.sln -c Release
dotnet test ms-devops-api/tests/Mps.Devops.Tests/Mps.Devops.Tests.csproj -c Release --no-build
```

Os projetos também estão em `OlivawHub.sln`. Antes de executar a API, configure por ambiente ou secret store:

```text
ConnectionStrings__DbContext=<conexão do banco do serviço>
Identity__Issuer=ms-devops-api
Identity__Audience=ms-devops-api
Identity__SigningKey=<chave aleatória exclusiva com pelo menos 32 bytes>
```

Depois de preparar o schema:

```bash
dotnet run --project ms-devops-api/src/Mps.Devops.Api/Mps.Devops.Api.csproj -c Release --no-launch-profile
```

Nenhuma migration ou banco foi criado na geração. Aplicação de migrations e bootstrap de root vêm desabilitados. O [guia de identidade](../olivaw-identity/README.md) explica como preparar o usuário inicial com senha externa; não há senha padrão.

## Docker

O contexto é a raiz do monorepo:

```bash
docker build -f ms-devops-api/src/Mps.Devops.Api/Dockerfile -t olivaw-devops .
```

O container usa HTTP na porta 8080, sem PFX, e executa sem root. O Compose fica em `docker/docker-compose.yml`; o arquivo `.env.example` contém exemplos que devem ser substituídos antes do uso. Não há configuração de implantação AWS ou Traefik específica deste serviço nesta etapa.

Consulte o [fluxo de desenvolvimento](../docs/desenvolvimento.md), o [padrão Git Flow](../docs/gitflow.md) e os `AGENTS.md` locais antes de acrescentar funcionalidades.
