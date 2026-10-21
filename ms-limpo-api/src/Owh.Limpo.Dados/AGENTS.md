# Persistência — Owh.Limpo.Dados

Estas regras complementam o `AGENTS.md` da raiz para persistência compartilhada.

- Target: `net10.0`.
- Dados depende somente do núcleo e não referencia a API.
- Mantenha aqui `Repository<T>`, `IWriteDbContext`, `WriteDbContext`, mapeamentos e DI próprios. `IRepository<T>`, `EntityValidator<T>` e convenções compartilhadas vêm de `Owh.OlivawCore` e `Owh.OlivawCore.Dados`.
- Código específico de provedor permanece no projeto `Owh.Limpo.Dados.<Provedor>`.

## Entity Framework e repositórios

- Tabelas de domínio usam somente letras minúsculas, sem hífens ou underscores, por exemplo `cliente` e `clienteimagem`. Não introduza snake_case em nomes de tabelas.

- Reutilize `IRepository<T>` e `Repository<T>` para CRUD e consultas comuns com filtros genéricos por expressão. Não crie uma classe repository/reader específica quando esses métodos atenderem.
- Implemente um repository específico somente para uma consulta especializada ou uma otimização que os métodos genéricos não cubram. Nesse caso, mantenha contratos de núcleo em `Owh.Limpo/Repositories` quando forem necessários e estenda `Repository<T>` na camada de dados (por exemplo, `ClienteRepository : Repository<Cliente>`), herdando `IRepository<Cliente>` sem reimplementar o CRUD.
- Escritas (`Add`, `Update`, `Remove`) e `SaveChangesAsync` permanecem explícitas; o chamador controla a unidade de persistência.
- Propague `CancellationToken` em toda operação assíncrona.
- Evite tracking, dados e includes desnecessários, N+1 e materialização precoce. Paginação é obrigatória para coleções potencialmente grandes.
- Prefira LINQ/EF Core e Fluent API com lambdas. SQL bruto exige necessidade comprovada, parametrização e testes; nunca concatene entrada.
- Novas entidades persistidas devem ter mapping em `Extensions/Tables` e registro em `TableExtensionsFacade`.
- Entidades filhas que derivam de `Entity` e têm identidade própria devem ser mapeadas como entidades normais com PK `int`, FK `int` e navegações explícitas usando `HasMany`/`WithOne`/`HasForeignKey`. Reserve `OwnsMany` para objetos de valor sem identidade própria.
- Preserve remoção lógica e auditoria definidas por `Entity`.
- Não renomeie tabelas, colunas, chaves ou índices sem avaliar dados e implantação existentes.

## Provedores e migrations

- `DatabaseProvider` reconhece `PostgreSql`, `SqlServer` e `Oracle`; Oracle não deve ser tratado como funcional sem completar e testar sua integração.
- Migrations pertencem exclusivamente ao projeto do provedor correspondente e nunca devem ser misturadas.
- O comando Codex `new feature` não cria, altera nem executa migrations. Aguarde pedido explícito do mantenedor.
- Mudanças de schema devem ser validadas contra todos os provedores declarados como suportados.
- Considere que `InitializeDatabaseAsync()` sempre aplica migrations no startup, antes de atender requisições e avalie compatibilidade retroativa antes de mudanças destrutivas.
- Respeite a convenção de nomes do provedor: minúsculas no PostgreSQL e maiúsculas no Oracle.
- `SetupConstraints` do Core usa sufixos minúsculos `_pk`, `_fk`, `_ix` e limite de 63 caracteres. Avalie o efeito em migrations existentes antes de alterar schema.

## Contratos observados

- `GetFirstAsync` valida ausência; `GetFirstOrDefaultAsync` permite ausência. Preserve essa diferença nos handlers e testes.
- `RemoveAsync` usa `DbSet.Remove` na implementação atual. Confirme o fluxo da entidade antes de assumir remoção lógica.
- Ao simplificar filtros, preserve a tradução pelo EF e a combinação lógica dos predicados.

## Padronização de migrations

- Gere migrations somente mediante pedido explícito; `new feature` não inclui essa etapa.
- Confira a branch atual. Use a issue em PascalCase como nome: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se já existir uma migration com esse nome, preserve o histórico e acrescente um sufixo descritivo, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome com o mantenedor.
- Gere somente para os provedores solicitados, no projeto do provedor da API consumidora. O Core não contém migrations. Não copie históricos entre serviços.
- Use a factory de design time quando disponível, sem iniciar o host nem acessar bancos reais. Compile com `-m:1`; use `--no-build` no EF somente após compilar o modelo atual. `--no-restore` pressupõe dependências restauradas.
- Revise migration, designer, snapshot e SQL entre a migration anterior e a nova. Preserve BOM UTF-8 e CRLF dos arquivos C#. Não altere migrations antigas já publicadas.
- Gerar e revisar não autoriza `database update`, conexão a bancos reais ou redeploy. O startup das APIs aplica as migrations pendentes; registre essa consequência antes da publicação.
- Documente comandos reproduzíveis no README da API e consulte os [comandos do Core](../../../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores). Informe as validações executadas e os provedores não contemplados.
