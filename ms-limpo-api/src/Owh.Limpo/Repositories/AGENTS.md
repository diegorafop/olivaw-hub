# Repositories — Owh.Limpo

Estas regras complementam o `AGENTS.md` do núcleo para contratos de persistência e leitura.

- Reutilize `IRepository<T>` de `Owh.OlivawCore` para operações CRUD e consultas comuns com filtros por `Expression<Func<T, bool>>`.
- Antes de criar uma abstração específica, confira se os métodos básicos de `IRepository<T>` atendem ao caso. Não crie repository ou reader só para encapsular um filtro comum.
- Crie contrato e implementação específicos apenas para consultas realmente especializadas ou que precisem de otimização própria no banco.
- Contratos específicos pertencem aqui; implementações ficam em `Owh.Limpo.Dados/Repositories`.
- Quando precisar de implementação específica, herde do `Repository<T>` existente sempre que possível, por exemplo `PortfolioRepository : Repository<PortfolioEvent>`, mantendo as operações genéricas já disponíveis.
- Não recrie `IRepository<T>` nem coloque implementação de EF Core neste projeto.
