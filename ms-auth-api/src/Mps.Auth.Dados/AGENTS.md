# Persistência — Mps.Auth.Dados

Estas regras complementam o `AGENTS.md` da raiz para persistência compartilhada.

- Target: `net10.0`.
- Dados depende somente do núcleo e não referencia a API.
- Reutilize `IRepository<T>` do pacote `Mps.HolosCore`; centralize aqui sua implementação `Repository<T>`, `IWriteDbContext`, `WriteDbContext`, validações, convenções, mapeamentos e DI comuns.
- Código específico de provedor permanece no projeto `Mps.Auth.Dados.<Provedor>`.

## Entity Framework e repositórios

- Reutilize `Repository<T>` para CRUD genérico; implemente contratos específicos do núcleo quando necessário.
- Escritas (`Add`, `Update`, `Remove`) e `SaveChangesAsync` permanecem explícitas; o chamador controla a unidade de persistência.
- Propague `CancellationToken` em toda operação assíncrona.
- Evite tracking, dados e includes desnecessários, N+1 e materialização precoce. Paginação é obrigatória para coleções potencialmente grandes.
- Prefira LINQ/EF Core e Fluent API com lambdas. SQL bruto exige necessidade comprovada, parametrização e testes; nunca concatene entrada.
- Novas entidades persistidas devem ter mapping em `Extensions/Tables` e registro em `TableExtensionsFacade`.
- Preserve remoção lógica e auditoria definidas por `Entity`.
- Não renomeie tabelas, colunas, chaves ou índices sem avaliar dados e implantação existentes.

## Provedores e migrations

- `DatabaseProvider` reconhece `PostgreSql`, `SqlServer` e `Oracle`; Oracle não deve ser tratado como funcional sem completar e testar sua integração.
- Migrations pertencem exclusivamente ao projeto do provedor correspondente e nunca devem ser misturadas.
- O comando Codex `new feature` não cria, altera nem executa migrations. Aguarde pedido explícito do mantenedor.
- Mudanças de schema devem ser validadas contra todos os provedores declarados como suportados.
- Considere que `UseMigrations()` aplica migrations no startup e avalie compatibilidade retroativa antes de mudanças destrutivas.
- Respeite a convenção de nomes do provedor: minúsculas no PostgreSQL e maiúsculas no Oracle.
- Para PostgreSQL, PKs, FKs e índices devem ter no máximo 30 caracteres e seguir `Tabela_PK`, `Tabela1_Tabela2_Coluna_FK` e `Tabela_Coluna_IX`.

## Contratos observados

- `GetFirstAsync` valida ausência por `EntityValidator<T>.NotFoundAsync`; `GetFirstOrDefaultAsync` permite ausência. Preserve essa diferença e não simule retorno nulo de `GetFirstAsync` como fluxo normal nos testes de handlers.
- `RemoveAsync` usa `DbSet.Remove` na implementação atual. Não presuma que toda remoção é lógica apenas pela existência de `DataRemocao`; confirme o fluxo da entidade antes de alterar a semântica.
- Ao simplificar filtros, evite ternários e condições concatenadas; preserve a tradução pelo EF e a combinação lógica dos predicados.
