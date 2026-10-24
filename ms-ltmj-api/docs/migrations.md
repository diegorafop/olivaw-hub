# Migration inicial — LTMJ

O histórico foi recriado para uma base vazia, conforme autorização do mantenedor. A migration `InitialSchema` contém `authorization`, `accessprofile`, `accessprofileauthorization`, `userprofile`, `userprofileaccessprofile`, `portfolioevent` e `portfoliophoto`. Nenhuma tabela demonstrativa é criada.

`PortfolioPhoto` deriva de `Entity`, possui PK `int`, `ExternalId` GUID público, auditoria e FK `PortfolioEventId` `int` para `PortfolioEvent`. O mapping usa uma relação normal 1:N, com navegação de classes e exclusão em cascata. Tabelas e colunas usam lowercase sem underscores; constraints e histórico técnico do EF mantêm as convenções próprias.

O banco de produção é `ltmj_prd`, com schema `ms_ltmj`. O mantenedor deve recriar o banco vazio antes de publicar a imagem desta entrega. O startup sempre aplica migrations e executa bootstrap de root; usuários, eventos e fotos não são recriados a partir dos dados antigos.

```bash
dotnet ef migrations add NomeDaMigration \
  --project ms-ltmj-api/src/Owh.Ltmj.Dados.PostgreSql \
  --startup-project ms-ltmj-api/src/Owh.Ltmj.Dados.PostgreSql \
  --context WriteDbContext --configuration Release --output-dir Migrations
```

A factory é de design time e não abre conexão ao gerar arquivos. Nenhum banco remoto foi alterado durante esta tarefa. O root fica com a operação Olivaw; a equipe do cliente usa conta local sem root e permissão `portfolio.manage`. A conta técnica `ltmj` no Olivaw é outra identidade e não compartilha o banco ou o secret desta API.
