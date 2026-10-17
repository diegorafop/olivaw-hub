# Diretrizes — Persistência de identidade

Estas regras complementam o [guia do módulo](../../AGENTS.md).

## Responsabilidade

Biblioteca `net10.0` com mapeamentos EF Core e `IdentityStore<TContext>`. O contexto concreto e a conexão pertencem ao consumidor. Não crie um banco, host, contexto global, driver de produção ou migrations próprios do módulo.

## Persistência

- Preserve índices únicos, chaves estrangeiras, tabelas `identity_*`, filtros de remoção lógica e o controle de concorrência por versão de segurança.
- `FindAsync` respeita filtros; `FindIncludingRemovedAsync` os ignora deliberadamente. `ExistsAsync` também considera registros removidos para reservar identificadores. Não uniformize esses comportamentos incidentalmente.
- Carregue as navegações dos vínculos na mesma consulta, evitando N+1 na resposta paginada.
- `PermissionsAsync` considera usuário, perfis e vínculos válidos; preserve a exclusão de perfis inativos e registros removidos.
- Propague cancelamento e mantenha `SaveAsync` explícito. Não registre valores sensíveis das entidades ou da conexão.

## Validação

Use testes relacionais isolados para constraints, filtros, restauração de vínculos e permissões. SQLite é usado na suíte; não o apresente como comprovação de comportamento PostgreSQL ou SQL Server. Mudanças de schema exigem avaliação das migrations dos consumidores, sem gerá-las como efeito colateral.
