# Domain da API — Owh.Devops.Api

Esta pasta contém modelos internos específicos da apresentação ou da execução dos casos de uso desta API. Entidades persistidas e invariantes de negócio pertencem a `../../Owh.Devops/Domain`, no projeto de núcleo; não duplique entidades, DTOs públicos ou mapeamentos EF aqui.

- Mantenha tipos pequenos, com nomes e propriedades em inglês, sem dependências de banco.
- Commands e queries permanecem em `Features/<Recurso>Feature/Commands` e `Queries`, com verbos `Create`, `Read`, `Update` e `Delete`.
- Interfaces para recursos externos pertencem a `Owh.Devops/Services`; contratos de outros microservices ficam em `Owh.Devops/Requests`.
