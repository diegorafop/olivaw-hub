# Núcleo — Owh.Devops

Estas regras complementam o `AGENTS.md` da raiz para arquivos do núcleo `Owh.Devops`.

- Target: `netstandard2.1`, linguagem C# 10.
- O núcleo contém apenas regras e contratos específicos do microservice. Os tipos genéricos dessas pastas vêm de `Owh.OlivawCore`.
- Não referencie API, ASP.NET Core, EF Core ou implementações de infraestrutura.
- Interfaces consumidas por camadas externas devem nascer aqui quando representarem abstrações do domínio ou da aplicação.

## Domínio

- Entidades persistidas derivam de `Entity`, que concentra identidade, auditoria e remoção lógica.
- Entidades mantêm comportamento e invariantes; não devem ser DTOs moldados por controllers.
- Para entidades e value objects com invariantes, prefira fábricas ou métodos de criação que garantam estado válido. Use `new` normalmente para DTOs e objetos sem invariantes.
- Preserve `ExternalId`, `ActorUserId`, `CreatedAt`, `UpdatedAt` e `DeletedAt` quando aplicáveis.
- Não introduza tipos de transporte, atributos web ou detalhes de banco no domínio.

## Contratos e abstrações

- Reutilize `IRepository<T>` para CRUD genérico.
- Crie contratos específicos apenas quando uma operação expressar linguagem do domínio ou exigir otimização própria.
- Services do núcleo são portas para APIs, filesystem, mensageria ou outros recursos externos; regras de negócio permanecem no domínio/caso de uso.
- Mantenha contratos pequenos, explícitos e sem parâmetros opcionais que escondam informação necessária.

## Tipos compartilhados e identidade

- Use `Entity` e `IRepository<T>` diretamente de `Owh.OlivawCore`; não crie cópias ou subclasses vazias.
- Distinga a chave interna `Id` da identidade pública `ExternalId`. Preserve os campos de auditoria nos contratos e conversões aplicáveis.
- Expresse regras com decisões simples e métodos nomeados, conforme as diretrizes da raiz.
