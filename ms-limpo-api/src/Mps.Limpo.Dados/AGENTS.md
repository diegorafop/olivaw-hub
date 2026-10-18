# Persistência — Mps.Limpo.Dados

Estas regras complementam o `AGENTS.md` da raiz para persistência compartilhada.

- Target: `net10.0`.
- Dados depende somente do núcleo e não referencia a API.
- Mantenha aqui `Repository<T>`, `IWriteDbContext`, `WriteDbContext`, mapeamentos e DI próprios. `IRepository<T>`, `EntityValidator<T>` e convenções compartilhadas vêm de `Mps.OlivawCore` e `Mps.OlivawCore.Dados`.
- Código específico de provedor permanece no projeto `Mps.Limpo.Dados.<Provedor>`.

## Entity Framework e repositórios

- Tabelas de domínio usam somente letras minúsculas, sem hífens ou underscores: `portfolioevent`, `portfoliophoto`. Não introduza snake_case em nomes de tabelas.

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
- Considere que `InitializeDatabaseAsync()` sempre aplica migrations no startup, antes de atender requisições e avalie compatibilidade retroativa antes de mudanças destrutivas.
- Respeite a convenção de nomes do provedor: minúsculas no PostgreSQL e maiúsculas no Oracle.
- `SetupConstraints` do Core usa sufixos minúsculos `_pk`, `_fk`, `_ix` e limite de 63 caracteres. Avalie o efeito em migrations existentes antes de alterar schema.

## Contratos observados

- `GetFirstAsync` valida ausência; `GetFirstOrDefaultAsync` permite ausência. Preserve essa diferença nos handlers e testes.
- `RemoveAsync` usa `DbSet.Remove` na implementação atual. Confirme o fluxo da entidade antes de assumir remoção lógica.
- Ao simplificar filtros, preserve a tradução pelo EF e a combinação lógica dos predicados.
