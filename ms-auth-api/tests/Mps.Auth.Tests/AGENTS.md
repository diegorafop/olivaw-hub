# Testes — Mps.Auth.Tests

Estas regras complementam o `AGENTS.md` da raiz para testes automatizados.

- Target: `net10.0`.
- Use xUnit, Moq, Bogus/Faker Builders, NBuilder e FluentValidation TestHelper conforme os padrões existentes.
- Espelhe features e componentes em `<Feature>Test/Commands`, `<Feature>Test/Queries`, `ExtensionsTest` e `HelperTest`.
- Cada arquivo de command/query em `src/Mps.Auth.Api/Features` deve ter uma classe de teste com o mesmo nome da operação, removendo o sufixo `Command`/`Query` e adicionando `Test` (por exemplo, `SelecionarUsuarioByIdTest`). Cada controller deve ter seu próprio `<Nome>ControllerTest`, com testes para suas ações públicas. Não substitua esse espelhamento por um único teste genérico que percorre a assembly.
- Dados reutilizáveis ficam em `Mocks`/Faker Builders e devem ser determinísticos quando ordem ou valor exato afetarem a asserção.

## Cobertura esperada

- Nos testes de commands e queries, confirme que a operação usa seu request/payload de entrada e sua response/payload de saída próprios, respeitando `Command`/`CommandResponse` ou `Query`/`QueryResponse`. Não aceite handlers que retornem diretamente tipos primitivos ou coleções.
- Handler: sucesso, request nulo, cancelamento/propagação do token e cada regra ou ramificação relevante.
- Validator: request válido, cada campo/regra inválida e mensagens específicas, preferencialmente com `TestValidate`.
- Controller: delegação ao mediator, request correto, token e resultado/status HTTP.
- Repositório: predicados, filtros, ordenação, persistência, ausência e comportamento específico de provedor.
- Integração externa: sucesso, timeout/cancelamento, payload inválido e mapeamento de códigos de erro.
- Bugfix: inclua teste que falhe sem a correção.
- O objetivo é 100% de cobertura unitária com prioridade para domínio e CQS, sem testes artificiais ou `[ExcludeFromCodeCoverage]` sem justificativa de infraestrutura.

## Forma dos testes

- Nomeie como `<Metodo>_should_<resultado>` e deixe o cenário explícito.
- Organize Arrange, Act e Assert, mesmo sem comentários formais.
- Cada teste deve ser independente e observar comportamento público.
- Use Moq nas fronteiras; não faça mock de objetos simples.
- Use Faker Builders para massa válida e sobrescreva explicitamente apenas o dado relevante ao cenário.
- Centralize os builders de features em `Mocks/FakerBuilder`: registre cada builder seguindo `VersaoFeature`, com propriedade de apoio, propriedade pública virtual, inicialização sob demanda com `??=` e setter. Nos testes, instancie `FakerBuilder` e acesse `_fakerBuilder.AutorizacaoFeature` (ou a feature correspondente), sem instanciar diretamente o builder da feature.
- Não dependa de relógio local, rede, banco compartilhado, ordem aleatória ou estado de outro teste.
- Faça assertions específicas sobre tipo, valor, status, mensagem e interação importante; `NotNull` isolado não basta.
- Verifique ausência de chamadas inesperadas quando isso proteger o fluxo do caso de uso.
- Execute ao menos o projeto/classe afetado e, antes da entrega, a suíte completa quando viável.

## Testes de referência para autorização

- Para mudanças de cobertura, mantenha produção e contratos existentes; amplie para outras features somente no escopo solicitado.
- Cubra handler e validator da query separadamente: a chamada direta a `Handle` não executa o pipeline FluentValidation.
- Capture o predicado recebido pelo repositório e execute-o contra entidades com `IdExterno` correspondente e diferente; `It.IsAny` sozinho não comprova o filtro.
- Verifique cada campo mapeado, incluindo opcionais nulos e preenchidos. Use datas fixas e `UseSeed` local nos Faker Builders, sem alterar o seed global do Bogus.
- Use um token cancelável distinto de `CancellationToken.None` quando a assinatura permitir. Simule cancelamento na dependência e confira a propagação; não presuma que o handler verifica o token por conta própria.
- No controller, confira a mesma request e response, status HTTP e uma única chamada ao mediator. `It.IsAny` é matcher de mock, não dado para chamar o método sob teste.
- Para auditoria de `Post`/`Put`, configure `DefaultHttpContext` com JWT sintético no header Authorization e a claim configurada. Não use credenciais reais; esse token é apenas dado de teste, sem validar autenticação.
- Evite ternários e condições booleanas concatenadas também em callbacks e assertions. Prefira asserções separadas por propriedade.

- `UsuarioLogadoId` é interno ao command. Para observar auditoria sem expor membros de produção, use a conversão pública `EntityExtensions.ToDomain<Command, Autorizacao>` no callback do mediator, como no fluxo real.
- Execute o runner xUnit direto conforme os comandos da raiz; a chamada VSTest tradicional não é compatível com a configuração atual no SDK .NET 10.
