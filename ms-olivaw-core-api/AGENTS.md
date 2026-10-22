# Diretrizes de desenvolvimento — Owh.OlivawCore

Este arquivo contém as regras globais do repositório. Os `AGENTS.md` dos projetos complementam estas regras no respectivo diretório. Em caso de conflito, siga a instrução mais específica e registre no PR decisões arquiteturais que extrapolem as diretrizes.

## Objetivo e arquitetura

O `Owh.OlivawCore` reúne bibliotecas reutilizáveis do ecossistema OlivawCore, distribuídas como pacotes NuGet com suas DLLs para consumo pelos microservices. O objetivo é compartilhar contratos, extensões e infraestrutura sem replicar código entre serviços.

| Projeto / pacote | Target | Responsabilidade |
| --- | --- | --- |
| `Owh.OlivawCore` | `netstandard2.1`, C# 10 | Entidade base, contratos, requests e interfaces de repositório, serviços e configuração. |
| `Owh.OlivawCore.Dados` | `net10.0` | Helpers e convenções de EF Core e `EntityValidator<T>`. |
| `Owh.OlivawCore.Api` | `net10.0` | Extensões ASP.NET Core, validação, mapeamento, clientes HTTP, autenticação e mensageria. |
| `Owh.OlivawCore.Tests` | `net10.0` | Testes das bibliotecas; não gera pacote. |

```text
Owh.OlivawCore.Api   -> Owh.OlivawCore.Dados -> Owh.OlivawCore
Owh.OlivawCore.Api   -> Owh.OlivawCore
Owh.OlivawCore.Tests -> Owh.OlivawCore.Api
```

- Os três projetos de produção usam `Microsoft.NET.Sdk` e são bibliotecas. O sufixo `.Api` identifica utilitários para aplicações ASP.NET Core; o host pertence ao microservice consumidor.
- O núcleo não referencia os outros projetos, ASP.NET Core ou EF Core. Dados não referencia Api.
- `IRepository<T>` pertence ao núcleo. Atualmente não existem `Repository<T>`, `Service<T>`, `DbContext` concreto, projetos de provedores ou migrations nesta solução. Não documente componentes planejados como implementados.
- Regras de negócio, entidades específicas, endpoints, composição da aplicação, configuração do banco e migrations pertencem aos microservices consumidores.
- Os helpers reconhecem nomes de provedores; isso não comprova suporte completo a PostgreSQL, SQL Server ou Oracle. Não prometa integração sem implementação e testes correspondentes.

## Escopo de novas funcionalidades

- Implemente capacidades compartilhadas que possam ser usadas por diferentes microservices, com contratos pequenos e explícitos.
- Antes de alterar, inspecione a implementação mais próxima, seus consumidores internos, testes, referências e metadados de pacote.
- Solicitações como `new feature` devem ser interpretadas no contexto destas bibliotecas. Não aplique automaticamente scaffolding de CRUD, controllers, handlers CQS, facade de tabelas ou migrations copiado de outro repositório.
- CQS e MediatR podem ser usados pelos consumidores. Aqui, mantenha a infraestrutura transversal correspondente, como o pipeline de validação.
- Quando faltar informação que altere materialmente um contrato público, esclareça o requisito; para decisões locais, siga os padrões existentes e informe suposições relevantes.

## Compatibilidade e empacotamento

- Tipos públicos, namespaces, assinaturas, constraints genéricas, serialização, mensagens de validação e chaves de configuração fazem parte do contrato consumido por outros projetos.
- Preserve compatibilidade de fonte e binária. Delete/renomear membros, adicionar membros obrigatórios a interfaces, alterar defaults ou mover tipos entre assemblies exige avaliação do impacto nos consumidores.
- Prefira evolução aditiva quando possível. Não transforme membros `internal` em `public` sem necessidade de consumo externo e contrato definido.
- Preserve `PackageId`, targets, referências entre projetos e `IsPackable`. Não eleve o target do núcleo para acomodar uma dependência de infraestrutura.
- As versões estão nos `.csproj`; mantenha coerência entre os pacotes de uma entrega. Use versionamento semântico: correção compatível em patch, capacidade compatível em minor e quebra de contrato em major.
- Não altere versões nem publique pacotes como efeito colateral de uma tarefa sem esse escopo. Versões publicadas devem ser tratadas como imutáveis.
- Prefira consumo via NuGet, que transporta DLLs e metadados de dependências. Copiar uma DLL isolada não substitui a resolução das suas dependências.

## Engenharia e manutenção

- Aplique responsabilidade única e mantenha coesão por componente. Prefira composição e abstrações pequenas a hierarquias profundas.
- Use injeção por construtor para dependências externas. Extensões estáticas sem estado são adequadas; evite service locator e estado global mutável.
- Use métodos curtos, nomes descritivos, guard clauses e `early return` quando aplicáveis.
- Não use o operador de supressão de nulabilidade (`!`, como em `valor!` ou `null!`). Trate a possibilidade de `null` explicitamente com verificações simples e `early return`, mantendo o fluxo claro para quem lê o código.
- Não concatene condições booleanas com `&&` ou `||`, como em `if (condicao1 && condicao2 || condicao3)`. Extraia as decisões para métodos pequenos com nomes intuitivos, usando `if`s simples e `early return`; não apenas transfira a expressão composta para outro método.
- Não adicione comentários ao código, incluindo comentários de bloco e documentação XML (`///`). As únicas formas permitidas são `//TODO -` para trabalho futuro ou pendente e `//NOTE -` para explicar um workaround necessário. Expresse a intenção com métodos pequenos e nomes intuitivos para métodos, parâmetros e variáveis.
- Evite duplicação sem antecipar abstrações sem um padrão repetido e estável.
- Não misture refatorações, formatação global ou atualizações de dependências não relacionadas.
- Use `async` de ponta a ponta para I/O e propague `CancellationToken`; não use `.Result`, `.Wait()` ou `Task.Run` para simular assincronicidade.
- Use `DateTime.UtcNow` para dados persistidos e integrações, salvo requisito explícito de fuso.
- Trate warnings novos como defeitos. Preserve alterações preexistentes no worktree.

## Convenções e segurança

- Os `.editorconfig` são a fonte de verdade. Use quatro espaços, CRLF em C# e respeite a configuração mais próxima: `src` usa UTF-8 sem BOM; os testes herdam UTF-8 com BOM da raiz.
- Preserve o estilo local de namespaces e organização dos arquivos; não reformate código não relacionado.
- Use `PascalCase` para tipos e membros, `camelCase` para parâmetros e variáveis, `_camelCase` para campos privados e prefixo `I` para interfaces.
- Métodos assíncronos novos usam sufixo `Async`, exceto contratos de framework como `Handle`. Não renomeie contratos existentes incidentalmente.
- Mantenha `using` fora do namespace; use chaves quando melhorarem a segurança.
- Em lambdas curtas, use `item` ou o nome singular do conceito; evite identificadores de uma letra.
- Prefira LINQ quando continuar legível e iteração imperativa quando representar melhor fluxo, desempenho ou efeitos.
- Valide entradas obrigatórias cedo. Não esconda estados inválidos em cadeias longas de operadores null-conditional.
- Evite parâmetros opcionais que escondam informação necessária. `CancellationToken = default` é aceitável em abstrações quando não muda a semântica.
- Um arquivo representa uma unidade coesa. Execute formatter somente nos arquivos alterados.
- Nunca inclua segredos, certificados privados, tokens ou credenciais em código, testes, logs, pacotes ou commits. Não introduza caracteres invisíveis ou de controle.
- Não concatene entrada em SQL nem registre dados sensíveis.

## Dependências de terceiros

- Antes de adicionar pacote, verifique se a plataforma ou dependências atuais já resolvem o problema.
- Avalie benefício, manutenção, licença, segurança, compatibilidade com o target e impacto transitivo nos consumidores.
- Não adicione bibliotecas para substituir poucas linhas triviais.
- Mantenha ferramentas de desenvolvimento/testes fora das dependências públicas do pacote, usando `PrivateAssets` quando apropriado.

## Validação e entrega

Execute conforme o escopo, a partir da raiz:

```bash
dotnet restore OlivawCore.sln
dotnet build OlivawCore.sln -c Release --no-restore
dotnet test tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c Release --no-build
dotnet pack OlivawCore.sln -c Release --no-build -o ./dist/packages
```

Para mudanças de comportamento, cubra os cenários relevantes com testes isolados. Quando necessário, colete cobertura com `dotnet test tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c Release --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory ./dist/test-results`. Informe separadamente cobertura de linhas e ramificações, respeitando as exclusões documentadas no README. Alterações somente em documentação pedem conferência de comandos, caminhos e diff; não exigem execução da suíte.

O fluxo Docker oficial de geração está no [README](README.md#gerar-pacotes-com-docker). Ele usa o SDK .NET 10, gera os pacotes em `dist/packages` e exporta os resultados de testes em `dist/results`. O target `final` exporta arquivos; não é uma imagem para executar um serviço e não envia pacotes ao Gitea.

- Ao alterar empacotamento, valide também o comando Docker, os três `.nupkg` e seus metadados/dependências. Não inclua testes, segredos ou artefatos acidentais nos pacotes.
- O build Docker depende do acesso à CA corporativa e às fontes de pacotes. Não contorne TLS nem altere certificados reais para fazer a validação passar.
- Publicação no registro NuGet do Gitea é uma etapa separada, com destino e credenciais fornecidos pelo ambiente. Não invente organização, endpoint ou token.
- Antes de concluir, revise o diff, confirme ausência de erros/warnings novos nas verificações executadas e informe comandos rodados e validações não realizadas.
- Atualize o README quando contratos, configuração, consumo ou empacotamento mudarem. Não versione `dist`, logs ou relatórios gerados.

## Versionamento e revisão

- Siga o Git Flow clássico do [monorepo](../docs/gitflow.md): `feature/issue001-descricao`, releases versionadas e hotfixes versionadas com issue associada. Preserve o uso de `finish`; PRs são opcionais.
- Commits devem ser pequenos, coerentes e descrever a intenção.
- PRs devem explicar motivação, solução, pacotes e contratos afetados, compatibilidade, testes e riscos para os consumidores.
- Prefira PRs com até 500 linhas adicionadas de lógica de produção e não ultrapasse 700. Testes não entram nessa contagem. Divida mudanças maiores ou justifique por que isso não é seguro.

## Padronização de migrations

- Gere migrations somente mediante pedido explícito; `new feature` não inclui essa etapa.
- Confira a branch atual. Use a issue em PascalCase como nome: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se já existir uma migration com esse nome, preserve o histórico e acrescente um sufixo descritivo, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome com o mantenedor.
- Gere somente para os provedores solicitados, no projeto do provedor da API consumidora. O Core não contém migrations. Não copie históricos entre serviços.
- Use a factory de design time quando disponível, sem iniciar o host nem acessar bancos reais. Compile com `-m:1`; use `--no-build` no EF somente após compilar o modelo atual. `--no-restore` pressupõe dependências restauradas.
- Revise migration, designer, snapshot e SQL entre a migration anterior e a nova. Preserve BOM UTF-8 e CRLF dos arquivos C#. Não altere migrations antigas já publicadas.
- Gerar e revisar não autoriza `database update`, conexão a bancos reais ou redeploy. O startup das APIs aplica as migrations pendentes; registre essa consequência antes da publicação.
- Documente comandos reproduzíveis no README da API e consulte os [comandos do Core](README.md#migrations-dos-microservices-consumidores). Informe as validações executadas e os provedores não contemplados.

## Limites de versões por licenciamento

Por decisão explícita do mantenedor, manter **MassTransit e seus transportes em 8.5.x** e **MediatR em 12.5.x**, preservando as linhas escolhidas para uso gratuito. Não atualizar essas famílias para outra minor/major, nem adicionar pacote que exija versões fora desses intervalos, sem nova autorização explícita.

- Usar versões estáveis explícitas nos projetos; alinhar todos os pacotes `MassTransit.*` na mesma versão 8.5.x.
- Conferir dependências transitivas após restore, além das referências diretas. Não usar atualização automática irrestrita para contornar os limites.
- A regra inclui Core, identidade, APIs, testes, template e serviços gerados. Ao atualizar dependências, verificar também compatibilidade entre EF Core, provedores e frameworks.
