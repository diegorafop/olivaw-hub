# Biblioteca de infraestrutura de dados — Mps.OlivawCore.Dados

Estas regras complementam o [AGENTS.md da raiz](../../AGENTS.md).

## Responsabilidade e dependências

- Target: `net10.0`; pacote `Mps.OlivawCore.Dados`.
- Depende do núcleo e de bibliotecas de persistência/configuração; não referencia Api.
- Contém `ConstraintExtensions`/`ConstraintBuilder`, `DbContextOptionsBuilderExtensions`, helpers de configuração em `ServiceCollectionExtensions` e `EntityValidator<T>`.
- `IRepository<T>` pertence ao núcleo. Atualmente não há implementação `Repository<T>`, `IWriteDbContext`, `WriteDbContext`, mapeamentos de entidades ou `TableExtensionsFacade` nesta solução.
- Os microservices consumidores fornecem seus contextos, entidades de negócio, mappings, implementação de repositório, conexão e provedor EF Core.

## Helpers de persistência

- Preserve o contrato de `EntityValidator<T>` e a propagação de `CancellationToken`; teste presença/ausência de entidade e mensagens quando alteradas.
- `OnConfiguring` aplica convenções de nomes a partir de `PostgreSql`, `SqlServer` ou `Oracle`. Não registra o driver nem configura uma conexão por si só.
- `GetDbContextHost` lê `DatabaseProvider` e `ConnectionStrings:DbContext`. Teste parsing, chaves esperadas e entradas inválidas quando alterar esse helper; nunca exponha a connection string em logs/erros.
- `SetupConstraints` usa nomes em minúsculas, sufixos `_pk`, `_fk`, `_ix` e limite configurado de 63 caracteres. Não substitua isso por convenções de 30 caracteres copiadas de outro projeto.
- Alterar convenções pode mudar o schema gerado pelos consumidores. Teste nomes longos, truncamento e colisões conforme o escopo e documente o impacto.
- Não prometa suporte completo a um banco pela existência de uma ramificação em um helper. Integração específica exige driver, implementação e testes apropriados.

## Evolução de infraestrutura compartilhada

- Se solicitada uma implementação genérica de repositório/contexto, mantenha-a desacoplada de entidades e regras de negócio de um microservice e compatível com `IRepository<T>`.
- Preserve unidade de persistência explícita, cancelamento, auditoria e semântica de remoção lógica; não presuma que apenas herdar `Entity` aplica esses comportamentos automaticamente.
- Evite tracking, includes, N+1 e materialização desnecessários. Ofereça limites/paginação quando o contrato permitir coleções potencialmente grandes.
- Prefira LINQ/EF Core e Fluent API. SQL bruto exige necessidade comprovada, parametrização e testes.
- Não adicione uma dependência específica de provedor à biblioteca compartilhada incidentalmente; avalie separação e impacto transitivo quando esse suporte for solicitado.

## Banco e migrations dos consumidores

- Este repositório não possui projetos de provedores nem migrations. Criação de schema, migrations e atualização de banco pertencem ao microservice que mantém o contexto.
- Não introduza `UseMigrations()`, atualização automática de banco, conexão a banco corporativo ou comandos `dotnet ef` como parte de tarefas comuns destes pacotes.
- Mudanças de metadados/convenções devem informar possíveis efeitos nas migrations dos consumidores sem gerar migrations aqui.

## Validação

- Teste helpers e metadados em isolamento quando possível. Comportamento SQL específico exige teste de integração explícito com o provedor correspondente.
- Não trate mocks ou provedores em memória como prova de compatibilidade SQL. Não adicione um driver de produção apenas para viabilizar um teste unitário.
