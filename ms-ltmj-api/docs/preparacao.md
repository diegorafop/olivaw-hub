# Preparação da API LTMJ

## Entrega atual

API derivada do template, com gestão de portfólio, com core e identidade por `ProjectReference`. Domínio técnico proposto: `api.ltmj.olivaw-hub.duckdns.org`. As cinco configurações e dotenv fazem parte do projeto; a configuração é versionada temporariamente no repositório privado até Key Vault.

CRUD de eventos, consulta pública, visibilidade e upload multipart estão implementados. Consulte [gestão de portfólio](../../ms-ltmj-ng/docs/gestao-portfolio.md). Comentários e pedidos comerciais continuam fora desta entrega. A migration inicial PostgreSQL foi gerada e revisada, sem execução remota; consulte [migrations](migrations.md). O mantenedor informou a criação de `ltmj_prd`. Root, schema e tabelas ainda dependem da preparação do ambiente. Há testes próprios de portfólio além dos testes herdados.

## Antes de executar com infraestrutura real

- Preparar o banco `ltmj_<ambiente>`, schema `ms_ltmj` e permissões do usuário de conexão.
- Revisar e aplicar a migration inicial no ambiente autorizado; não presumir que o deploy já aplicou SQL.
- Confirmar endereço da API e origem do frontend no CORS. O template atual permite origens amplamente; restringir às origens aprovadas antes de produção.
- O startup prepara as tabelas pelas migrations existentes e executa o bootstrap obrigatório usando o e-mail root confirmado. Não há conta root já criada nem senha de cliente divulgada no protótipo.
- Manter chaves JWT exclusivas desta organização/ambiente; não usar chave do Hub ou DevOps.
- Migrations pendentes e bootstrap sempre rodam no startup; manter broker desabilitado enquanto não houver necessidade.
- Validar health e logs sem imprimir dotenv ou segredos.

## Rodar o projeto

Da raiz do monorepo:

```bash
dotnet build ms-ltmj-api/Ltmj.sln -c Release
dotnet test ms-ltmj-api/tests/Owh.Ltmj.Tests/Owh.Ltmj.Tests.csproj -c Release
dotnet run --project ms-ltmj-api/src/Owh.Ltmj.Api --no-launch-profile
```

`dotnet run` não carrega dotenv automaticamente. Forneça as configurações de infraestrutura pelo ambiente ou use o Compose, que seleciona o dotenv. Rodar sem preparar a conexão não comprova disponibilidade das dependências.

## Evolução de negócio proposta

Contratos em inglês e features CQS seguindo o template. Antes de implementar, aprovar campos e comportamento para `Event`, `GalleryImage`, `QuoteRequest` e `Review`. Publicação de imagens exige armazenamento, limites, validação e revisão de autorização de uso; comentários exigem moderação e anti-spam. Essas escolhas ficam fora do protótipo inicial.

As conversas e decisões do cliente são registradas no [histórico canônico da LTMJ](../../ms-ltmj-ng/docs/historico.md), junto ao briefing inicial. Não manter uma cópia divergente na API.
