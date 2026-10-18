# Biblioteca de infraestrutura para APIs — Mps.OlivawCore.Api

Estas regras complementam o [AGENTS.md da raiz](../../AGENTS.md).

## Responsabilidade e organização

- Target: `net10.0`; pacote `Mps.OlivawCore.Api`, usando `Microsoft.NET.Sdk` como biblioteca.
- Depende de `Mps.OlivawCore` e `Mps.OlivawCore.Dados`. Fornece componentes ASP.NET Core e integrações para hosts consumidores.
- Organize funcionalidades nas pastas existentes: `Extensions/Generics`, `Extensions/Entities`, `Extensions/Contracts`, `Helpers`, `Services`, `Settings` e `Validation`.
- Não há `Program.cs`, controllers, `Features` ou `ReadyFeature`. Não introduza uma aplicação executável ou scaffolding de negócio como parte de uma extensão compartilhada.
- O consumidor escolhe registros de DI, ordem do pipeline, endpoints, ambientes e políticas de segurança. `Properties/launchSettings.json` não torna a biblioteca executável; `dotnet run` não é o fluxo de uso deste projeto.

## Extensões e contratos públicos

- Preserve nomes, namespaces, assinaturas e comportamento observável das extensões usadas pelos microservices.
- Conversões genéricas ficam em `EntityExtensions`/`BaseContractExtensions`; regras específicas de negócio ficam no consumidor.
- Alterações no mapeamento por reflexão exigem testes para nulabilidade, tipos compatíveis, coleções, identidade e auditoria. Considere o comportamento existente de `Id`/`ExternalId`, `TraceId` e datas em UTC.
- Helpers e extensões sem estado podem ser estáticos. Dependências externas devem ser injetadas; não crie estado global ou resolva serviços por service locator.
- Métodos `internal` não são acessíveis ao consumidor do pacote. Só amplie a superfície pública quando isso fizer parte do requisito e estiver documentado/testado.

## Validação e respostas

- Reutilize `FluentValidatorBehavior<,>`, `FluentValidationExceptionFilter`, `FluentValidationResponse` e `ServiceResponseValidator` conforme o componente afetado.
- O pipeline MediatR existente valida requests e normaliza strings com `Trim()`. Preserve a ordem e os efeitos observáveis ou documente/teste a mudança.
- Preserve o formato dos erros, status HTTP e mensagens consumidos pelas aplicações. Reutilize `MessageHelper` quando houver mensagem equivalente.
- Propague `CancellationToken` em validação e integrações assíncronas. Diferencie cancelamento de falhas de integração.
- Não exponha stack traces, tokens, credenciais ou conteúdo sensível nas respostas ou logs.

## Configuração, HTTP e mensageria

- Registros compartilhados ficam nas extensões de `IServiceCollection`/`WebApplicationBuilder`; configuração do pipeline nas extensões de `WebApplication`.
- Documente chaves obrigatórias, defaults, lifetimes e efeitos dos registros. Não adicione inicialização automática de recursos que o host não solicitou.
- Leia configuração por `IConfiguration`/Settings e valide valores obrigatórios sem incluir segredos nas mensagens.
- Use `IHttpClientFactory` e os contratos `IServiceHttpClient`/`IProviderAuthentication`; preserve timeout, cancelamento, autenticação, rotas e tratamento de respostas.
- Considere o ciclo de vida de `HttpClient`, respostas, streams e cache. Não descarte recursos fornecidos pelo chamador sem contrato explícito.
- Reutilize a configuração existente do MassTransit para RabbitMQ, ActiveMQ e Azure Service Bus; teste também o comportamento com mensageria desabilitada e `NullPublishEndpoint` quando afetado.
- Alterações em autenticação JWT, CORS, serialização e OpenAPI afetam todos os hosts consumidores. Avalie compatibilidade e segurança; não apresente defaults existentes como garantias de segurança para produção.

## Validação e distribuição

- Teste o comportamento público dos componentes afetados, registros de DI e configuração quando pertinente; não crie controllers de negócio apenas para testar a biblioteca.
- Valide build/testes e, quando mudar distribuição ou dependências, o pacote NuGet e o uso por um host consumidor.
- Não adicione `appsettings`, Docker Compose, portas ou ambientes de implantação de um microservice a esta biblioteca. O Dockerfile da raiz gera pacotes e relatórios.
