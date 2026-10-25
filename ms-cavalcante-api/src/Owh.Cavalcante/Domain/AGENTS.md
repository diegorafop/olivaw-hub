# Domínio — Owh.Cavalcante

- Entidades persistidas derivam de `Owh.OlivawCore.Domain.Entity` e usam `Id` interno `int`, `ExternalId` público GUID, `ActorUserId`, `CreatedAt`, `UpdatedAt` e `DeletedAt`.
- Relacionamentos entre entidades usam FK `int` e navegações explícitas; seu mapeamento fica em `Owh.Cavalcante.Dados/Extensions/Tables` com `HasOne`/`WithMany` ou `HasMany`/`WithOne`.
- Entidades filhas com identidade própria também derivam de `Entity`; `OwnsMany` é reservado a objetos de valor sem identidade própria.
- Preserve invariantes e comportamento no domínio. Não referencie EF Core, ASP.NET Core ou detalhes de transporte.
- Interfaces de serviços, repositórios e requests externos ficam em `Services`, `Repositories` e `Requests`, fora de `Domain`.
