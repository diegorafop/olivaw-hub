# Services — Owh.OlivawHub

Estas regras complementam o `AGENTS.md` do núcleo para portas de capacidades externas.

- Declare aqui interfaces pequenas para recursos que não são persistência, como armazenamento de arquivos, APIs externas, mensageria ou serviços de terceiros.
- Mantenha os contratos independentes de ASP.NET Core, Entity Framework Core e detalhes de infraestrutura.
- Coloque implementações concretas nas camadas adequadas, normalmente em `Owh.OlivawHub.Api/Services` ou em um projeto de integração específico.
- Regras de negócio pertencem ao domínio ou ao caso de uso; não use Services como depósito de lógica sem responsabilidade definida.
- Repositórios e operações de banco seguem as instruções de `../Repositories/AGENTS.md`.
