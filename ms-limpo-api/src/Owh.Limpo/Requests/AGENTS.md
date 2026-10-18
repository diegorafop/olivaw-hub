# Requests — Owh.Limpo

Estas regras complementam o `AGENTS.md` do núcleo para comunicação com outros microservices.

- Use esta pasta somente para contratos de transporte enviados a microservices fora do contexto atual e para os respectivos contratos de resposta.
- Organize por serviço e feature, separando `Commands` e `Queries` quando houver volume que justifique, por exemplo `Requests/Identity/UserProfiles/Commands`.
- Commands e Queries externos seguem os verbos do padrão Olivaw: `Create`, `Read`, `Update` e `Delete` para operações CRUD; não use `Get`. Outros verbos de negócio, como `Process`, são válidos quando descrevem a intenção.
- Não coloque aqui os requests, handlers ou validators MediatR das features próprias da API; eles ficam junto à feature em `Owh.Limpo.Api`.
- Mantenha os DTOs focados no contrato serializado. Autenticação, envio HTTP, retry e demais detalhes de integração pertencem à implementação do client/service, não aos tipos de request.
- Confira o contrato publicado pelo microservice de destino antes de definir nomes, campos e formatos; não invente compatibilidade presumida.
