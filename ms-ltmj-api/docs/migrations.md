# Migration inicial PostgreSQL — LTMJ

Gerada a pedido do mantenedor: `20261018014627_InitialIdentityAndPortfolio`, em `src/Mps.Ltmj.Dados.PostgreSql/Migrations`, com arquivo de operações, designer e snapshot.

## Modelo e nomes

Tabelas de domínio em lowercase, sem hífens ou underscores: `autorizacao`, `example`, `perfilacesso`, `portfolioevent`, `usuario`, `perfilacessoautorizacao`, `portfoliophoto` e `usuarioperfilacesso`. `example` pertence ao modelo herdado do template. Índices, constraints e a tabela técnica de histórico do EF mantêm as convenções de infraestrutura existentes.

Os mapeamentos incorretos `portfolio_events` e `portfolio_photos` foram corrigidos antes da geração desta migration. O `Up` cria tabelas e índices; não contém exclusão ou transformação de dados. O `Down` reverte a criação e deve ser tratado como destrutivo.

## Configuração atual

- Banco de produção: `ltmj_prd`, criado pelo mantenedor conforme relato.
- Usuário configurado na conexão: `postgres`; não foi criado usuário PostgreSQL exclusivo da LTMJ.
- Search path e histórico de migrations: `ms_ltmj`.
- A migration não inclui contas ou senhas; root é criado pelo bootstrap de identidade após o schema estar pronto.

Geração e revisão não conectaram ao banco remoto. Se tabelas antigas já tiverem sido criadas na AWS, conferir o histórico e os dados antes de aplicar: esta é uma migration inicial, não uma migration de renomeação de uma versão anterior.

## Gerar próximas migrations

O projeto do provedor contém `WriteDbContextFactory` para design time, evitando inicializar API, broker, bootstrap ou serviços HTTP. Sua conexão local de design não contém senha nem aponta à AWS; a factory não abre conexão ao gerar arquivos.

```bash
dotnet ef migrations add NomeDaMigration \
  --project ms-ltmj-api/src/Mps.Ltmj.Dados.PostgreSql \
  --startup-project ms-ltmj-api/src/Mps.Ltmj.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --output-dir Migrations
```

## Aplicação no startup

O startup sempre aplica as migrations pendentes e garante root antes de atender requisições. Não há flags de habilitação. As alterações locais não executaram SQL nem criaram root na VM; isso acontece ao publicar a nova imagem e inicializar a API.

O runtime usa a conexão real e executa `Database.MigrateAsync()` antes do bootstrap, com cancelamento propagado. O usuário PostgreSQL precisa de permissão para criar schema e tabelas. A conexão usa `SearchPath=ms_ltmj`; não executar SQL avulso com search path diferente.

O bootstrap obrigatório usa o e-mail root Olivaw configurado e o segredo existente; se root já existir, preserva senha e cadastro. A conta da equipe não é root. Após isso, cadastrar a equipe e conceder somente `portfolio.manage`. A criação do banco vazio, sozinha, não executa esses passos.

## Conferências executadas

Build da API com zero erros e avisos. Script SQL idempotente gerado para revisão local, sem execução no banco. Nenhum teste automatizado foi adicionado ou executado nesta alteração.
