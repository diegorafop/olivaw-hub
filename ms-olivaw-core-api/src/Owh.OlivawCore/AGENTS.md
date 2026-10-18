# Biblioteca de contratos — Owh.OlivawCore

Estas regras complementam o [AGENTS.md da raiz](../../AGENTS.md).

## Responsabilidade e dependências

- Target: `netstandard2.1`, linguagem C# 10; pacote `Owh.OlivawCore`.
- Contém `Domain`, `Contracts`, `Requests`, `Repositories`, `Services` e `Settings` compartilhados pelos microservices.
- Não referencie Api, Dados, ASP.NET Core, EF Core ou implementações de infraestrutura.
- Preserve compatibilidade com .NET Standard 2.1. Não use APIs exclusivas de .NET 10 neste projeto.
- As dependências atuais são `System.ComponentModel.Annotations` e `System.Text.Json`; avalie o impacto de novas dependências em todos os consumidores.

## Entidades e contratos

- `Entity` concentra `Id`, `ExternalId`, `ActorUserId`, `CreatedAt`, `UpdatedAt` e `DeletedAt`. Preserve os tipos e a semântica desses membros.
- `Entity` fornece campos compartilhados; persistência, auditoria e filtros de remoção lógica precisam ser aplicados pela implementação consumidora.
- Preserve `BaseContract` e os contratos de requests/serialização. Mudanças de nome, tipo, nulabilidade ou atributos podem afetar mensagens e integrações já publicadas.
- Não introduza entidades de negócio específicas de um microservice, tipos de ASP.NET Core ou detalhes de banco.

## Interfaces compartilhadas

- `IRepository<T>` é o contrato genérico de persistência para entidades derivadas de `Entity`; sua implementação não está neste projeto.
- Preserve filtros por expressão, joins, ordenação, operações em coleção, `SaveChangesAsync`, `CanConnectAsync` e propagação de `CancellationToken`.
- Não adicione EF Core ao contrato nem presuma que operações de escrita salvam automaticamente. A unidade de persistência deve permanecer explícita.
- `IServiceHttpClient` e `IProviderAuthentication` definem contratos de integração HTTP; as implementações atuais ficam em Api. Não há `IService<T>`/`Service<T>` genérico na árvore atual.
- `ISetting` e `ISettingProviderAuthentication` descrevem configuração; leitura de arquivo, DI e acesso ao ambiente pertencem às camadas externas.
- Mantenha interfaces pequenas e explícitas. Adicionar um membro obrigatório afeta implementações nos microservices e exige avaliação de compatibilidade.

## Validação

- Ao alterar contratos, compile os projetos dependentes e teste o comportamento afetado, incluindo serialização quando aplicável.
- Avalie o impacto nos consumidores externos: compilar esta solução não prova compatibilidade com todas as implementações de interfaces já distribuídas.
