# Núcleo — Owh.Limpo

Estas regras complementam o `AGENTS.md` da raiz para arquivos do núcleo `Owh.Limpo`.

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
- Para leituras comuns, comece com `IRepository<T>` e seus métodos genéricos, usando `Expression<Func<T, bool>>` para filtros e os métodos básicos de consulta. Não implemente um repository/reader específico quando o contrato genérico já atender.
- Crie um repository específico somente quando a consulta for realmente especializada ou exigir otimização própria. Nesse caso, mantenha o contrato em `Repositories/` e a implementação na camada de dados, aproveitando o repository base existente sempre que possível.
- Services do núcleo são portas para APIs, filesystem, mensageria ou outros recursos externos; regras de negócio permanecem no domínio/caso de uso.
- Mantenha contratos pequenos, explícitos e sem parâmetros opcionais que escondam informação necessária.

## Estrutura de pastas

- Mantenha `Repositories/`, `Services/` e `Requests/` como pastas permanentes do núcleo, mesmo quando vazias; cada uma tem instruções próprias em `AGENTS.md`.
- `Domain/` contém entidades, value objects e regras de domínio; não coloque interfaces de serviços ou repositórios nessa pasta.
- `Services/` contém interfaces de portas para recursos que não são persistência, como armazenamento de arquivos, APIs externas e mensageria.
- `Repositories/` contém contratos específicos de leitura ou persistência. Reutilize `IRepository<T>` de `Owh.OlivawCore` para CRUD genérico; não crie outra interface equivalente.
- `Requests/` contém contratos de transporte usados para enviar Commands/Queries a outros microservices, fora do contexto atual. Não coloque aqui requests e handlers MediatR das próprias features da API.
- Implementações ficam nas camadas concretas, como `Owh.<Nome>.Api/Services` e `Owh.<Nome>.Dados/Repositories`.

## Tipos compartilhados e identidade

- Use `Entity` e `IRepository<T>` diretamente de `Owh.OlivawCore`; não crie cópias ou subclasses vazias.
- Distinga a chave interna `Id` da identidade pública `ExternalId`. Preserve os campos de auditoria nos contratos e conversões aplicáveis.
- Expresse regras com decisões simples e métodos nomeados, conforme as diretrizes da raiz.
