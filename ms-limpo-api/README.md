# ms-limpo-api
- **explicação serviço.

## Ambientes
- **Development** 
- **Quality** 
- **Release** 
- **Staging** 
- **Production**

## Arquitetura
 - **Command-Query Separation (CQS)** - [Documentação](https://www.codewithmukesh.com/blog/cqrs-in-aspnet-core-3-1/)
	- **Commands**
		- _Ação_ + _Nome_ + _Command_
			- Exemplo: _InserirMandadoPagamentoCommand_
  	- **Queries**
		- _Ação_ + _Nome_ + _Query_
			- Exemplo: _SelecionarMandadoPagamentoQuery_
 - **Repository Pattern**
	- [Documentação](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core)
 - **Camadas**
	- Domí­nio
	- Api Resful [Documentação](https://docs.microsoft.com/pt-br/azure/architecture/best-practices/api-design)
	- docker-compose [Documentação] (https://docs.docker.com/compose/)
	- Dados
	- Tests

## Tecnologias
 - **Mps.HolosCore** 1.1.0, **Mps.HolosCore.Dados** 1.1.0 e **Mps.HolosCore.Api** 1.2.1, consumidos diretamente por NuGet. O template mantém apenas a composição do host, o contexto/repositório concretos e as regras da feature de exemplo.
 - **Provedores:** PostgreSQL e SQL Server são os caminhos suportados. Oracle permanece reservado; os exemplos de comandos abaixo não comprovam suporte funcional nem substituem testes de integração.

## Configuração do template

As credenciais e senhas de banco, broker e autenticação devem vir de variáveis de ambiente ou de um secret store. Preencha `ConnectionStrings__DbContext`, `authentication__secret`, `masstransit__user`, `masstransit__password`, `services__provider-authentication__login` e `services__provider-authentication__senha` conforme o ambiente. O catálogo de configuração inclui `services:loki:configured=false`; habilite Loki somente com suas opções de transporte preenchidas. Os placeholders `${{...}}` do arquivo de produção são substituídos pela esteira.
 - **DotNetCore**
 - **EntityFrameWorkCore**
 - **FluentValidation**
	- [Documentação pipeline Behavior](https://medium.com/tableless/fail-fast-validations-com-pipeline-behavior-no-mediatr-e-asp-net-core-f3854d3c21fa)
	- [Documentação com MediatR](https://www.codewithmukesh.com/blog/mediatr-pipeline-behaviour/)
 - **MetiatR** [Documentação](https://github.com/jbogard/MediatR)
 - **Swashbuckle Swagger AspNetCore**
 - **Jwt Token Bearer**
 - **Postgresql**
 - **Migrations**
 - **Docker-Compose**

## Tests
 - **Xunit**
 - **Moq**
 - **NBuilder**
 - **100% cobertura de testes unitários**

## Padrões de Projeto
 - **Nomenclatura**
	- **Ações** 
		- Nomes devem ser verbos no infinitivo.
		- Correto = Inserir, Selecionar, Esconder, VincularEndereco, etc.
		- Errado = Insere, Seleciona, EscondeModal, VinculaEndereco, etc.
 - **Variáveis**
	- **Funções, Métodos, Classes e Propriedades**
		- PascalCase
	- **Locais, Parametros de Funções/Métodos**
		- camelCase
 - **Variáveis locais (injeção de dependência)**
	- **private readonly _nomeVariavel**
		- Correto = _private readonly IConfiguration_ __configuration;_
## Postgres
 - **ms-postgres**
	- [Documentação](https://git.mps.com.br/XJP/ms-holos-setup/src/branch/develop/ms-postgres)
 - **Nomenclatura**
	- **Tabelas**
		- **PK, FK, INDEX**
			- O `SetupConstraints` do HolosCore usa sufixos minúsculos `_pk`, `_fk`, `_ix` e limite de 63 caracteres. Confira o impacto antes de gerar migrations sobre um banco existente.
				- Exemplos
					- PK 	: tabela_pk
					- FK 	: tabela1_tabela2_coluna_fk
					- INDEX : tabela_coluna_ix

## Versionamento
 - **GitFlow** [Documentação](https://git-flow.readthedocs.io/en/latest/index.html)
	- master 	= Produção
	- develop	= Desenvolvimento
	- features	= Melhorias
		- Correto = _feature/MH-0001_descricao_branch_
	- hotfix	= Correções
		- Correto = _hotfix/MH-0002_descricao_branch_
	- release	= Versão
	- tag		= Checkpoint
 - **Sugestão de ferramenta para Git**
	- Sourcetree [Documentação](https://www.sourcetreeapp.com/)

## VsCode
 - **Commands**
	- **test**
		- dotnet test
	- **build**
		- dotnet build
	- **run**
		- dotnet run --project .\src\Mps.Limpo.Api\
 - **migrations**
	- update tool
		- dotnet tool update --global dotnet-ef
	- create
		- PostgreSQL: dotnet ef migrations add DbCreate --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.PostgreSql\
		- SQL Server: dotnet ef migrations add DbCreate --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.SqlServer\
		- Oracle: dotnet ef migrations add DbCreate --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.Oracle\
	- update
		- PostgreSQL: dotnet ef database update --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.PostgreSql
		- SQL Server: dotnet ef database update --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.SqlServer
		- Oracle: dotnet ef database update --startup-project .\src\Mps.Limpo.Api\ --project .\src\Mps.Limpo.Dados.Oracle
		- (Obs.: está configurado no startup para atualizar o database automaticamente).

## Docker
 - **Ambientes**
	- **localhost**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d
	- **development**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up --build -d
	- **quality**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.qas.yml up --build -d
	- **release**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.rls.yml up --build -d
	- **staging**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.stg.yml up --build -d
	- **production**
		- docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.prd.yml up --build -d
