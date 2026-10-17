# Preparação da API LTMJ

## Entrega atual

Esqueleto gerado do template, com core e identidade por `ProjectReference`. Domínio técnico proposto: `api.ltmj.olivaw-hub.duckdns.org`. As cinco configurações e dotenv fazem parte do projeto; a configuração é versionada temporariamente no repositório privado até Key Vault.

Não existe implementação de catálogo, CRUD de eventos, upload, comentários ou pedido comercial nesta etapa. Não foram criadas migrations, banco, usuário root, schema ou filas. Os testes herdados validam o template, não funcionalidades futuras.

## Antes de executar com infraestrutura real

- Preparar o banco `ltmj_<ambiente>`, schema `ms_ltmj` e permissões do usuário de conexão.
- Revisar e criar migrations somente quando autorizado.
- Confirmar endereço da API e origem do frontend no CORS. O template atual permite origens amplamente; restringir às origens aprovadas antes de produção.
- Confirmar e-mail do root e habilitar seu bootstrap somente depois do schema preparado. Não há conta root já criada nem senha de cliente divulgada no protótipo.
- Manter chaves JWT exclusivas desta organização/ambiente; não usar chave do Hub ou DevOps.
- Manter migração automática e broker desabilitados até haver necessidade e autorização.
- Validar health e logs sem imprimir dotenv ou segredos.

## Rodar o projeto

Da raiz do monorepo:

```bash
dotnet build ms-ltmj-api/Ltmj.sln -c Release
dotnet test ms-ltmj-api/tests/Mps.Ltmj.Tests/Mps.Ltmj.Tests.csproj -c Release
dotnet run --project ms-ltmj-api/src/Mps.Ltmj.Api --no-launch-profile
```

`dotnet run` não carrega dotenv automaticamente. Forneça as configurações de infraestrutura pelo ambiente ou use o Compose, que seleciona o dotenv. Rodar sem preparar a conexão não comprova disponibilidade das dependências.

## Evolução de negócio proposta

Contratos em inglês e features CQS seguindo o template. Antes de implementar, aprovar campos e comportamento para `Event`, `GalleryImage`, `QuoteRequest` e `Review`. Publicação de imagens exige armazenamento, limites, validação e revisão de autorização de uso; comentários exigem moderação e anti-spam. Essas escolhas ficam fora do protótipo inicial.

As conversas e decisões do cliente são registradas no [histórico canônico da LTMJ](../../ms-ltmj-ng/docs/historico.md), junto ao briefing inicial. Não manter uma cópia divergente na API.
