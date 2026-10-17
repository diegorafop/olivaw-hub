# Olivaw Hub

Tecnologia e inteligência para pequenos negócios. A marca faz referência a R. Daneel Olivaw, personagem da literatura de Isaac Asimov.

Este monorepo reúne bibliotecas compartilhadas e o template de APIs .NET para clientes com bancos e implantações separados.

| Pasta | Conteúdo |
| --- | --- |
| [ms-olivaw-core-api](ms-olivaw-core-api/README.md) | Contratos e infraestrutura genéricos |
| [olivaw-identity](olivaw-identity/README.md) | Bibliotecas de usuários, autenticação, perfis e permissões |
| [ms-limpo-api](ms-limpo-api/README.md) | Template de API que integra os módulos por `ProjectReference` |
| [docs/desenvolvimento.md](docs/desenvolvimento.md) | Organização do monorepo, soluções e fluxo de desenvolvimento |
| [docs/identidade.md](docs/identidade.md) | Decisões e validação da extração do módulo |

`olivaw-identity` não é um microservice: suas rotas executam dentro da API consumidora e suas entidades usam o banco desse consumidor. Não é necessário publicar NuGet para desenvolver neste monorepo.

```bash
dotnet build OlivawHub.sln -c Release
dotnet test OlivawHub.sln -c Release --no-build
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
```

A geração cria o projeto do cliente ao lado das bibliotecas compartilhadas. Consulte [TEMPLATE.md](ms-limpo-api/TEMPLATE.md) e a documentação de identidade antes de preparar o banco e o root inicial. Credenciais são configuradas por ambiente; não há senha root padrão.

O [briefing inicial](BRIEFING.md) preserva a proposta de negócio e as decisões ainda em aberto.

O padrão de branches e rastreabilidade está em [Git Flow](docs/gitflow.md). Para ativar os hooks neste clone, execute `python3 scripts/gitflow/setup.py`.
