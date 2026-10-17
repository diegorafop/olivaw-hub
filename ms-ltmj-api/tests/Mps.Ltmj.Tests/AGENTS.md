# Testes — Mps.Ltmj.Tests

Estas regras complementam o `AGENTS.md` da raiz para testes automatizados.

- Target: `net10.0`.
- Use xUnit, Moq, Bogus/Faker Builders, NBuilder e FluentValidation TestHelper conforme os padrões existentes.
- Espelhe features e componentes em `<Feature>Test/Commands`, `<Feature>Test/Queries`, `ExtensionsTest` e `HelperTest`.
- Cada operação nova deve ter testes próprios para handler e validator; cada controller novo deve ter testes para suas ações públicas. Não substitua esse espelhamento por um único teste que percorre a assembly.
- Dados reutilizáveis ficam em `Mocks`/Faker Builders e devem ser determinísticos quando ordem ou valor exato afetarem a asserção.

## Cobertura esperada

- Handler: sucesso, request nulo, cancelamento/propagação do token e cada regra ou ramificação relevante.
- Confira os contratos próprios de entrada e saída da operação; handlers CQS não devem retornar diretamente tipos primitivos ou coleções.
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
- Centralize builders em `Mocks/FakerBuilder`, seguindo o padrão existente da `VersaoFeature`, quando criar novos builders.
- Não dependa de relógio local, rede, banco compartilhado, ordem aleatória ou estado de outro teste.
- Faça assertions específicas sobre tipo, valor, status, mensagem e interação importante; `NotNull` isolado não basta.
- Verifique ausência de chamadas inesperadas quando isso proteger o fluxo do caso de uso.
- Execute ao menos o projeto/classe afetado e, antes da entrega, a suíte completa quando viável.

## Testes de referência

- Teste handler e validator separadamente: chamar `Handle` diretamente não executa o pipeline FluentValidation.
- Para filtros, capture o predicado recebido pelo repositório e confira entidades correspondentes e não correspondentes.
- Confira os campos mapeados, inclusive opcionais nulos e preenchidos. Use datas fixas e seeds locais quando o valor exato importar.
- Use um token cancelável distinto de `CancellationToken.None` para verificar propagação quando a assinatura permitir.
- Em controllers, confira request, response, status HTTP e número de chamadas ao mediator. Testes por chamada direta não executam autenticação nem model binding.
