# Olivaw

Tecnologia e inteligência para pequenos negócios. A marca faz referência a R. Daneel Olivaw, personagem da literatura de Isaac Asimov.

Este monorepo reúne bibliotecas compartilhadas e os templates de APIs .NET e frontend Angular para clientes com bancos e implantações separados.

| Pasta | Conteúdo |
| --- | --- |
| [ms-olivaw-core-api](ms-olivaw-core-api/README.md) | Contratos e infraestrutura genéricos |
| [olivaw-identity](olivaw-identity/README.md) | Bibliotecas de usuários, autenticação, perfis e permissões |
| [ms-limpo-api](ms-limpo-api/README.md) | Template de API que integra os módulos por `ProjectReference` |
| [ms-limpo-ng](ms-limpo-ng/README.md) | Template Angular e gerador local por Schematics |
| [ms-olivaw-ng](ms-olivaw-ng/README.md) | Landing page institucional derivada do template |
| [ms-ltmj-ng](ms-ltmj-ng/README.md) | Protótipo público da LTMJ Eventos |
| [ms-ltmj-api](ms-ltmj-api/README.md) | Fundação de backend independente da LTMJ |
| [ms-devops-api](ms-devops-api/README.md) | Nova API DevOps, com estrutura inicial gerada |
| [docs/desenvolvimento.md](docs/desenvolvimento.md) | Organização do monorepo, soluções e fluxo de desenvolvimento |
| [docs/identidade.md](docs/identidade.md) | Decisões e validação da extração do módulo |

`olivaw-identity` não é um microservice: suas rotas executam dentro da API consumidora e suas entidades usam o banco desse consumidor. Não é necessário publicar NuGet para desenvolver neste monorepo.

```bash
dotnet build Olivaw.sln -c Release
dotnet test Olivaw.sln -c Release --no-build
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
```

A geração cria o projeto do cliente ao lado das bibliotecas compartilhadas. Consulte [TEMPLATE.md](ms-limpo-api/TEMPLATE.md) e a documentação de identidade antes de preparar o banco e o root inicial. Credenciais são configuradas por ambiente; não há senha root padrão.

O [briefing inicial](BRIEFING.md) preserva a proposta de negócio e as decisões ainda em aberto. A [renomeação para Olivaw](docs/renomeacao-olivaw.md) registra os novos identificadores, a preparação dos bancos existentes e a troca de domínio pendente.

O padrão de branches e rastreabilidade está em [Git Flow](docs/gitflow.md). Para ativar os hooks neste clone, execute `python3 scripts/gitflow/setup.py`.

O par `ms-limpo-ng` e `ms-limpo-api` é a base de frontend e backend dos clientes. Para iniciar o frontend, use `cd ms-limpo-ng`, `npm ci` e `npm start`. Frontend e API são serviços separados. O settings público do frontend aponta diretamente para a URL da API (`http://localhost:5137` no desenvolvimento), com CORS configurado no backend. Veja o [guia do frontend](ms-limpo-ng/README.md).
