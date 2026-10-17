# Testes das bibliotecas — Mps.OlivawCore.Tests

Estas regras complementam o [AGENTS.md da raiz](../../AGENTS.md).

## Estrutura e ferramentas

- Target: `net10.0`; `IsTestProject=true`, `IsPackable=false`.
- O projeto referencia `Mps.OlivawCore.Api`, com acesso transitivo a Dados e ao núcleo.
- Use xUnit v3, Moq e as ferramentas já declaradas no `.csproj`: Microsoft.NET.Test.Sdk, runner Visual Studio, Coverlet collector e logger XML xUnit.
- Organize os testes por componente nas pastas `ExtensionsTest`, `HelperTest`, `ValidationTest`, `ServicesTest`, `RepositoriesTest` e `SettingsTest`; os auxiliares ficam em `TestSupport`.
- Não há Bogus, NBuilder, Faker Builders, testes de controllers ou features CQS na estrutura atual. Não os presuma disponíveis nem adicione bibliotecas sem necessidade.
- Dados e tipos auxiliares exclusivos de teste devem ficar neste projeto; prefira construção explícita ou builders locais simples quando houver repetição.
- Os arquivos C# deste diretório herdam UTF-8 com BOM e CRLF do `.editorconfig` da raiz.

## Cobertura esperada conforme a alteração

- Extensões/helpers: comportamento público, valores nulos/vazios, limites, serialização, reflexão e composição de expressões relevantes.
- Entidades/contratos: identidade, auditoria, conversões e compatibilidade dos dados serializados.
- Validação: entrada válida, regras inválidas, mensagens, normalização e propagação de cancelamento. Use o TestHelper do FluentValidation quando aplicável.
- Serviços HTTP: sucesso, erro remoto, resposta inválida, autenticação, timeout/cancelamento e descarte de recursos. Simule a fronteira HTTP sem acessar serviços reais.
- Configuração/DI/mensageria: chaves necessárias, registros, lifetimes e integrações habilitadas/desabilitadas, sem depender de broker externo em teste unitário.
- Dados: convenções de nomes, parsing de configuração, validação de entidades e operações de repositório quando houver implementação.
- Bugfix: inclua um teste de regressão que falhe sem a correção.
- Priorize cenários que protegem os consumidores do pacote. Não afirme 100% de cobertura sem relatório e diferencie linhas de ramificações. A coleta inclui os três pacotes e exclui arquivos gerados pelo framework; o Dockerfile mantém threshold zero.

## Forma dos testes

- Nomeie como `<Metodo>_should_<resultado>` e explicite o cenário.
- Organize Arrange, Act e Assert. Observe comportamento público, com assertions específicas sobre valores, mensagens e interações relevantes.
- Use Moq nas fronteiras; não faça mock de objetos simples nem de detalhes internos para repetir a implementação.
- Cada teste deve ser independente e determinístico. Não dependa de relógio local, rede, banco compartilhado, ordem de execução ou valores aleatórios para decidir o resultado.
- Para datas geradas pelo código, use uma estratégia de verificação estável; não exija igualdade com uma nova leitura do relógio.
- Testes de integração devem declarar seus requisitos e usar recursos isolados; não conecte a ambientes corporativos por padrão.
- Não adicione `[ExcludeFromCodeCoverage]` apenas para melhorar o percentual.

## Execução e relatórios

Execute da raiz, conforme o escopo:

```bash
dotnet test tests/Mps.OlivawCore.Tests/Mps.OlivawCore.Tests.csproj -c Release
dotnet test tests/Mps.OlivawCore.Tests/Mps.OlivawCore.Tests.csproj -c Release --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory ./dist/test-results
```

- Para uma checagem direcionada, use `--filter FullyQualifiedName~NomeDaClasse`; execute a suíte completa antes de entregar mudanças de comportamento quando viável.
- Use `--no-build` somente depois de compilar os mesmos target/configuração. Não reporte binários antigos como validação de alterações novas.
- O fluxo Docker do README, com `SKIP_UNITARY_TESTS=false`, executa testes com cobertura, uma execução adicional para XML e exporta relatórios para `dist/results`.
- O estágio de testes atual do Docker não recebe `BUILD_CONFIG` no comando `dotnet test`; não afirme que a suíte Docker roda em Release só porque o empacotamento usa essa configuração.
- Ao alterar o pipeline, confira falha do build quando testes falham e exportação dos resultados. Não versione pacotes, resultados ou relatórios gerados.
