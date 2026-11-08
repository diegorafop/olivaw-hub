# Owh.OlivawCore

Bibliotecas base do ecossistema OlivawCore para compartilhar contratos, extensões e infraestrutura entre microservices. A distribuição é feita por pacotes NuGet, que incluem as DLLs e suas dependências declaradas, evitando copiar a mesma implementação para cada projeto.

O repositório `ms-olivaw-core-api` produz três pacotes. `Owh.OlivawCore.Api` é uma biblioteca de apoio a aplicações ASP.NET Core; a aplicação que hospeda endpoints e regras de negócio fica no microservice consumidor.

## Compatibilidade da renomeação

Os pacotes, assemblies e namespaces usam agora `Owh.OlivawCore`, `Owh.OlivawCore.Dados` e `Owh.OlivawCore.Api`. Os consumidores devem atualizar as referências NuGet, os `using` e os nomes de tipos afetados. A solução é `OlivawCore.sln` e o diretório do projeto é `ms-olivaw-core-api`.

A renomeação também altera identificadores de configuração, prefixos de mensageria e nomes usados por reflexão que continham a marca anterior. Revise essas configurações e integrações nos consumidores antes de adotar os novos pacotes. Esta alteração não publica pacotes nem modifica os microservices consumidores.

## Pacotes e responsabilidades

| Pacote | Target | Conteúdo principal |
| --- | --- | --- |
| `Owh.OlivawCore` | .NET Standard 2.1 | `Entity`, `BaseContract`, requests, `IRepository<T>`, `IServiceHttpClient`, `IProviderAuthentication` e interfaces de configuração. |
| `Owh.OlivawCore.Dados` | .NET 10 | Convenções e helpers de Entity Framework Core, nomes de constraints, leitura de configuração de conexão e `EntityValidator<T>`. |
| `Owh.OlivawCore.Api` | .NET 10 | Extensões ASP.NET Core, conversão de entidades/contratos, serialização, validação, clientes HTTP, autenticação e configuração de mensageria. |

A versão declarada atualmente nos três projetos é `1.0.1`. Cada pacote mantém seus metadados e sua versão no respectivo `.csproj`.

### Dependências entre projetos

```text
Owh.OlivawCore.Api   -> Owh.OlivawCore.Dados -> Owh.OlivawCore
Owh.OlivawCore.Api   -> Owh.OlivawCore
Owh.OlivawCore.Tests -> Owh.OlivawCore.Api
```

O núcleo usa C# 10 e não depende de ASP.NET Core ou EF Core. Os pacotes Dados e Api usam .NET 10. Os testes ficam em um projeto separado, com `IsPackable=false`.

### Recursos disponíveis

- **Contratos compartilhados:** identidade, auditoria e remoção lógica em `Entity`; identificação e rastreamento em `BaseContract`.
- **Persistência:** contrato `IRepository<T>` com filtros, ordenação, operações individuais/em coleção e persistência explícita por `SaveChangesAsync`; helpers de EF Core em Dados.
- **Extensões e helpers:** conversão por reflexão, expressões LINQ, JSON, texto, arquivos, requests e respostas HTTP.
- **Validação:** regras FluentValidation, pipeline MediatR, filtro de exceções e tratamento de respostas de serviços externos.
- **Integrações:** `ServiceHttpClient`, `ProviderAuthentication`, cache e configuração MassTransit para RabbitMQ, ActiveMQ e Azure Service Bus.
- **ASP.NET Core:** extensões para configuração de serviços, controllers, autenticação, CORS e OpenAPI/Swagger.

O código atual oferece `IRepository<T>`, mas ainda não inclui uma implementação `Repository<T>` nem um `Service<T>` genérico. O consumidor fornece a implementação de persistência e registra suas dependências.

Os helpers de dados reconhecem `PostgreSql`, `SqlServer` e `Oracle` para operações específicas de configuração/convenções. O driver EF Core, o `DbContext`, os mappings e a validação da integração com o banco são responsabilidade do consumidor. Esta solução não contém migrations nem projetos separados de provedores.

## Estrutura

```text
src/
  Owh.OlivawCore/          # Contratos e abstrações compartilhadas
  Owh.OlivawCore.Dados/    # Infraestrutura comum de dados
  Owh.OlivawCore.Api/      # Utilitários para APIs e integrações
tests/
  Owh.OlivawCore.Tests/    # Testes das bibliotecas
Dockerfile               # Build, testes e empacotamento
OlivawCore.sln             # Solução
```

## Gerar pacotes com Docker

Execute na raiz do repositório, em um shell compatível com Bash, com Docker e BuildKit disponíveis:

```bash
DOCKER_BUILDKIT=1 docker build \
  --progress=plain \
  --no-cache \
  -f Dockerfile \
  --build-arg BUILD_CONFIG=Release \
  --build-arg SKIP_UNITARY_TESTS=false \
  --target final \
  --output type=local,dest=./dist \
  .
```

O Dockerfile usa a imagem do SDK .NET 10 e instala a CA corporativa. O ambiente de build precisa alcançar o endereço da CA definido no Dockerfile e as fontes de pacotes usadas pelo restore e pelas ferramentas de teste.

| Opção | Efeito |
| --- | --- |
| `BUILD_CONFIG=Release` | Compila e empacota as bibliotecas em Release. |
| `SKIP_UNITARY_TESTS=false` | Executa a etapa de testes e gera relatórios. O Dockerfile só executa os testes quando o valor é literalmente `false`. |
| `--target final` | Reúne os pacotes e os resultados dos estágios anteriores. |
| `--output type=local,dest=./dist` | Exporta os arquivos para a pasta local `dist`. |
| `--no-cache` | Executa o build sem reutilizar o cache das camadas. |

Com a versão atual e os testes habilitados, a saída esperada é:

```text
dist/
  packages/
    Owh.OlivawCore.1.0.1.nupkg
    Owh.OlivawCore.Dados.1.0.1.nupkg
    Owh.OlivawCore.Api.1.0.1.nupkg
  results/
    coverage.cobertura.xml
    xunit-result.xml
    reports/
```

O estágio de testes executa a suíte com cobertura e depois novamente para gerar o XML xUnit. A configuração `BUILD_CONFIG` é aplicada ao build/pack; os comandos de teste atuais do Dockerfile usam a configuração padrão do `dotnet test`. Com `SKIP_UNITARY_TESTS=true`, a pasta de resultados é criada sem os relatórios.

O target `final` exporta artefatos. O comando acima gera os pacotes localmente; a publicação no Gitea ocorre em uma etapa separada.

## Publicar no Gitea interno

Os `.nupkg` gerados em `dist/packages` são os artefatos destinados ao registro NuGet do Gitea corporativo. O endereço Git do repositório de código não substitui a URL do feed NuGet.

1. Confirme a versão dos três pacotes e confira os resultados do build e dos testes.
2. Configure a fonte NuGet do Gitea e a autenticação no ambiente local ou de CI, usando a URL e o proprietário do registro fornecidos pela empresa.
3. Publique os três pacotes da entrega, para que as dependências entre eles possam ser resolvidas pelos consumidores.

Se a fonte autenticada estiver cadastrada com o nome `Gitea`, o envio pode ser feito com:

```bash
dotnet nuget push "./dist/packages/*.nupkg" --source Gitea
```

`Gitea` é o nome de fonte usado neste exemplo; ajuste ao nome configurado no ambiente. Este repositório não define a URL do feed nem um pipeline de publicação automática. Mantenha credenciais fora dos arquivos versionados e não reutilize uma versão já publicada para conteúdo diferente.

## Consumir nos microservices

Configure o feed NuGet do Gitea no ambiente do projeto consumidor, com acesso também às fontes das dependências externas. Depois, adicione os pacotes necessários ao `.csproj` correspondente. Exemplos usando a versão atual, após sua publicação:

```bash
dotnet add caminho/MeuDominio.csproj package Owh.OlivawCore --version 1.0.1
dotnet add caminho/MeusDados.csproj package Owh.OlivawCore.Dados --version 1.0.1
dotnet add caminho/MinhaApi.csproj package Owh.OlivawCore.Api --version 1.0.1
```

Os caminhos são exemplos e devem ser substituídos pelos projetos do microservice. Referencie diretamente o pacote usado por cada camada. O NuGet resolve as dependências transitivas; Dados e Api exigem um projeto consumidor compatível com .NET 10.

Para experimentar uma versão antes da publicação, adicione o caminho absoluto de `dist/packages` como fonte NuGet local no ambiente consumidor, mantendo as demais fontes necessárias ao restore.

No microservice, implemente as regras de negócio e a persistência, registre os serviços necessários e invoque as extensões de configuração aplicáveis. Instalar o pacote disponibiliza os componentes; não configura automaticamente o host, a autenticação, o broker ou o banco.

## Migrations dos microservices consumidores

O nome segue a issue da branch: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se o nome já existir, use um sufixo descritivo sem reescrever o histórico, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome antes de gerar. Gere somente os provedores solicitados.

O Core fornece convenções de persistência, mas não possui banco ou migrations próprios. Gere cada migration no projeto do provedor da API consumidora. Os comandos abaixo partem da raiz do monorepo e usam a `Issue013` do `ms-olivaw-api`, criada na branch `feature/issue013-logo-whatsapp`, como exemplo para PostgreSQL.

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --no-restore -m:1

dotnet ef migrations add Issue013 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --no-build
```

O `--no-restore` pressupõe dependências restauradas; o `--no-build` usa a compilação anterior, que deve refletir os mappings atuais. O projeto possui uma factory de design time: esses comandos não iniciam o host HTTP, não executam bootstrap e não aplicam alterações no banco.

Para revisar o SQL da migration sem conectar a um banco, após compilar novamente o provedor com a migration gerada:

```bash
dotnet build ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql --no-restore -m:1

dotnet ef migrations script 20261018111944_InitialSchema Issue013 \
  --project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --startup-project ms-olivaw-api/src/Owh.Olivaw.Dados.PostgreSql \
  --context WriteDbContext --no-build \
  --output /tmp/olivaw-issue013.sql
```

A `Issue013` cria a tabela `lead` e seu índice único de `externalid`. Revise o arquivo da migration, seu designer, o snapshot e o SQL antes da publicação. No `ms-olivaw-api`, as migrations pendentes são aplicadas automaticamente no startup após o redeploy; gerar os arquivos não atualiza o ambiente publicado. Este exemplo não gera migrations de SQL Server ou Oracle.

Para outros serviços e entregas, ajuste os projetos, o contexto, a migration anterior e o nome da nova migration. Consulte as [instruções de persistência](../ms-olivaw-api/src/Owh.Olivaw.Dados/AGENTS.md) e o [README da API Olivaw](../ms-olivaw-api/README.md).

## Desenvolvimento e testes

Para desenvolvimento local, instale o SDK .NET 10 e execute na raiz:

```bash
dotnet restore OlivawCore.sln
dotnet build OlivawCore.sln -c Release --no-restore -m:1
dotnet test tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c Release --no-build
```

Para gerar os pacotes localmente após o build:

```bash
dotnet pack OlivawCore.sln -c Release --no-build -o ./dist/packages
```

Os projetos têm `GeneratePackageOnBuild=false`; a geração de `.nupkg` é feita por `dotnet pack`, inclusive no estágio `pack` do Dockerfile.

Para coletar cobertura local:

```bash
dotnet test tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c Release --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory ./dist/test-results
```

Os testes usam xUnit v3 e Moq, com Coverlet para cobertura. A suíte cobre contratos, extensões, validação, serviços HTTP, configuração de DI e metadados EF Core. Os testes de mensageria montam a configuração sem iniciar conexões com brokers. O pacote `xunit.v3.mtp-off` mantém a execução via VSTest, compatível com o collector e o fluxo MSBuild do Docker.

A coleta inclui os três pacotes de produção, inclusive Dados. O arquivo [coverage.runsettings](coverage.runsettings) exclui código `*.generated.cs` produzido pelo framework. As exclusões por atributo são restritas a `NullPublishEndpoint` (publicação sem operação), ao hook vazio `FluentValidationExceptionFilter.OnActionExecuting` e à composição de pipeline em `WebApplicationExtensions`, que requer validação de integração no host consumidor. A lógica de validação permanece na cobertura.

Consulte os percentuais de **linhas** e **ramificações** separadamente no `coverage.cobertura.xml` gerado. Cobertura de linhas não garante todos os cenários nem integração com os microservices consumidores. O Docker usa o mesmo escopo de bibliotecas e arquivos gerados, mas mantém threshold zero; não bloqueia automaticamente quedas de cobertura.

## Contribuição e versionamento

- Siga o [AGENTS.md principal](AGENTS.md) e as regras de [Core](src/Owh.OlivawCore/AGENTS.md), [Dados](src/Owh.OlivawCore.Dados/AGENTS.md), [Api](src/Owh.OlivawCore.Api/AGENTS.md) e [Testes](tests/Owh.OlivawCore.Tests/AGENTS.md).
- Preserve contratos públicos e avalie o impacto de alterações em todos os consumidores. Documente mudanças de configuração e compatibilidade.
- Use versionamento semântico nos pacotes e mantenha coerência das versões de cada entrega.
- Siga GitFlow: `master`, `develop`, `feature/MH-0001_descricao_branch`, `hotfix/MH-0002_descricao_branch`, releases e tags.
- Não versione os pacotes e relatórios gerados em `dist`.

## Atualização de dependências

As dependências foram conferidas na atualização de 22/10/2026. MassTransit permanece em 8.5.x e MediatR em 12.5.x por decisão explícita do mantenedor, incluindo referências transitivas e serviços gerados. Ver [versões, limites e verificações do monorepo](../docs/dependencias.md). Não houve publicação de pacotes ou alteração de banco como parte desta atualização.
