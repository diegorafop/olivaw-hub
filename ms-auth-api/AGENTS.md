# Diretrizes de desenvolvimento — Mps.Auth

Este arquivo contém as regras globais do repositório. Os arquivos `AGENTS.md` das camadas complementam estas regras no respectivo diretório. Em caso de conflito, siga a instrução mais específica, preserve o comportamento existente e registre no PR decisões arquiteturais que extrapolem as diretrizes.

## Objetivo e arquitetura

O `Mps.Auth` é uma API ASP.NET Core organizada por Command–Query Separation (CQS), MediatR, FluentValidation, Repository Pattern e Entity Framework Core. PostgreSQL e SQL Server são suportados; Oracle permanece reservado e não deve ser presumido funcional sem implementação e testes equivalentes.

```text
Mps.Auth.Api ───────> Mps.Auth.Dados.<Provedor>
       │                         │
       ├─────────────> Mps.Auth.Dados
       │                         │
       └─────────────────────────┴──> Mps.Auth

Mps.Auth.Tests ─────> Mps.Auth.Api
```

- O núcleo não referencia infraestrutura nem apresentação.
- Dados não referencia a API.
- Código de ASP.NET Core não deve vazar para o núcleo.
- Código específico de banco pertence ao projeto do provedor.
- Use injeção por construtor; não use service locator ou dependências estáticas.
- Migrations pertencem ao projeto do provedor correspondente e não podem ser misturadas. Oracle não deve receber suporte presumido por similaridade.

## Prioridades globais

1. Manter regras de negócio independentes de transporte, framework web e banco.
2. Preservar a separação entre leitura e escrita.
3. Fazer mudanças pequenas, coesas, testáveis e compatíveis com contratos existentes.
4. Não inverter a direção das dependências.
5. Nunca incluir segredos, certificados privados, tokens ou credenciais em código, testes, logs ou commits.

## Comando Codex `new feature`

Quando o usuário escrever `new feature <nome-do-objeto-de-dominio>` ou `codex new feature <nome-do-objeto-de-dominio>`, trate isso como solicitação para implementar uma feature CQS completa para o objeto informado. Exemplo: `codex new feature usuario` cria a feature `Usuario`.

O comando pode conter um bloco opcional de propriedades no formato `{ campo: tipo, modificadores }`, por exemplo `codex new feature usuario { nome: string, obrigatório, max: 150; email: string, obrigatório, único, email, max: 254 }`. Interprete `obrigatório`, `opcional`, `único`, `email`, `max`, `min`, `filtro`, `sensível`, valores default e relacionamentos como requisitos do domínio, validator, mapping, queries, responses e testes, conforme aplicável. O bloco descreve apenas propriedades específicas do objeto; não duplique `Id`, `IdExterno`, `UsuarioLogadoId`, `DataCadastro`, `DataAtualizacao` ou `DataRemocao`, pois são herdados de `Entity`.

Antes de implementar, inspecione a entidade e a feature completa mais próxima — como `AutorizacaoFeature` ou `PerfilAcessoFeature`, conforme a operação — e preserve os padrões atuais. Se o objeto ainda não existir e suas propriedades não forem informadas, procure contratos e linguagem já presentes no repositório; faça somente suposições conservadoras, informe-as durante o trabalho e peça definição quando a escolha alterar materialmente o contrato ou o schema.

Por padrão, o comando inclui:

1. Entidade ou ajustes de domínio no núcleo.
2. Mapeamento Fluent API e inclusão na facade de tabelas.
3. Extensions/mapeadores específicos da entidade.
4. Controller com rotas de recurso.
5. Commands `Inserir`, `Atualizar`, `Remover` e, quando o domínio admitir ativação/inativação, `Alternar`, com requests, responses, handlers e validators próprios.
6. Queries `Selecionar<Nome>ById` e `Selecionar<Nome>ByFilter`, com requests, responses, handlers e validators próprios.
7. Propagação de `CancellationToken`, auditoria e remoção lógica conforme `Entity`.
8. Faker Builders e testes unitários de controller, handlers, validators, predicados e ramificações relevantes.
9. Build, testes proporcionais ao escopo e revisão final do diff.

O comando `new feature` nunca inclui criação, edição ou execução de migrations. As migrations são responsabilidade do mantenedor e só podem ser geradas mediante pedido explícito separado.

## Engenharia e manutenção

- Aplique responsabilidade única e mantenha coesão por feature.
- Prefira composição e abstrações pequenas a hierarquias profundas.
- Não crie classes base para commands ou queries apenas porque suas operações são parecidas. Commands e queries devem permanecer concretos, independentes e coesos; herança entre eles aumenta o acoplamento e reduz a clareza da feature.
- No máximo, um command pode herdar de uma classe de modelo do domínio quando essa herança representar uma relação real do domínio. Não crie modelos base genéricos para compartilhar implementação entre commands ou queries.
- Use métodos curtos, condições com nomes que revelem intenção, `early return` e guard clauses quando aplicáveis.
- Evite duplicação, mas não antecipe abstrações sem um padrão repetido e estável.
- Não misture refatorações, formatação global ou atualizações de dependências não relacionadas.
- Preserve contratos HTTP por padrão; mudanças de rota, status ou payload devem ser intencionais e testadas.
- Use `async` de ponta a ponta para I/O; não use `.Result`, `.Wait()` ou `Task.Run` para simular assincronicidade.
- Use `DateTime.UtcNow` para dados persistidos e integrações, salvo requisito explícito de fuso.
- Trate warnings novos como defeitos.
- Preserve alterações preexistentes no worktree e nunca descarte trabalho de terceiros.

## Convenções e segurança

- Os `.editorconfig` são a fonte de verdade.
- Use quatro espaços, sem tabs, e preserve o estilo local.
- Arquivos C# devem usar UTF-8 com BOM conforme os `.editorconfig` atuais e CRLF conforme a configuração mais próxima.
- Não introduza caracteres Unicode invisíveis ou de controle. Unicode textual intencional é permitido.
- Use nomes descritivos: `PascalCase` para tipos e membros; `camelCase` para parâmetros e variáveis; `_camelCase` para campos privados; prefixo `I` para interfaces.
- Métodos assíncronos usam sufixo `Async`, exceto contratos de framework como `Handle`.
- Mantenha `using` fora do namespace, use chaves quando melhorarem a segurança e preserve a organização do arquivo tocado.
- Não use expression-bodied methods (`=>`) quando o corpo tiver várias instruções, quebras de linha ou lógica que exija leitura detalhada. Nesses casos, use o bloco tradicional com tipo de retorno, nome, parâmetros e chaves. Getters e setters simples de propriedades podem continuar usando a sintaxe normal do .NET.
- Em lambdas curtas, use `item` ou o nome singular do conceito; evite identificadores de uma letra.
- Prefira LINQ quando a operação declarativa continuar legível e iteração imperativa quando representar melhor fluxo, desempenho ou efeitos.
- Não esconda estados inválidos em cadeias longas de operadores null-conditional; valide entradas obrigatórias cedo.
- Não introduza parâmetros opcionais/default que escondam informação necessária. `CancellationToken = default` é aceitável em abstrações quando não muda a semântica.
- Um arquivo representa uma unidade coesa. Execute formatter somente nos arquivos alterados para evitar diffs massivos.
- Nunca concatene entrada em SQL, registre dados sensíveis ou codifique segredos em configuração versionada.

## Dependências de terceiros

- Antes de adicionar pacote, verifique se a plataforma ou dependências atuais já resolvem o problema.
- Exija benefício claro, manutenção ativa, licença compatível e postura de segurança aceitável.
- Não adicione bibliotecas para substituir poucas linhas triviais.
- Encapsule dependências externas quando houver risco de vazamento para o domínio.

## Validação e entrega

Execute conforme o escopo:

```bash
dotnet restore Auth.sln
dotnet build Auth.sln --no-restore
dotnet run --project tests/Mps.Auth.Tests/Mps.Auth.Tests.csproj --no-build --no-restore
dotnet run --project tests/Mps.Auth.Tests/Mps.Auth.Tests.csproj --no-build --no-restore -- -class "Mps.Auth.Tests.AutorizacaoFeatureTest.*"
```

O projeto usa xUnit v3 (pacote 4.x) e Microsoft.Testing.Platform. Com o SDK .NET 10, o `dotnet test` tradicional pode falhar por tentar usar VSTest; use o runner direto acima. No WSL com SDK instalado somente no Windows, use `dotnet.exe`. Cobertura exige um runner/coletor compatível com a configuração atual; não presuma que `--collect` funcione no runner direto.

A solução contém caminhos de provedores cuja capitalização pode divergir dos diretórios reais. Em sistemas case-sensitive, confirme essa causa antes de atribuir falhas ao código; não renomeie diretórios nem regrave a solução como efeito colateral.

Antes de concluir:

1. Confirme que não há erros nem warnings novos.
2. Revise o diff e remova artefatos, logs, credenciais e alterações acidentais.
3. Garanta testes isolados para o comportamento novo.
4. Atualize documentação e configuração quando contratos ou procedimentos mudarem.
5. Informe comandos executados e validações não realizadas.

## Versionamento e revisão

- Use o GitFlow documentado: `master`, `develop`, `feature/MH-0001_descricao_branch`, `hotfix/MH-0002_descricao_branch`, releases e tags.
- Commits devem ser pequenos, coerentes e descrever a intenção.
- PRs devem explicar motivação, solução, contratos/schema/configuração afetados, testes e riscos.
- Prefira PRs com até 500 linhas adicionadas de lógica de produção e não ultrapasse 700. Testes não entram nessa contagem. Divida mudanças maiores ou justifique na descrição por que isso não é seguro.

## Legibilidade de decisões

- Evite operadores ternários (`condicao ? valorA : valorB`), inclusive nos testes. Prefira `if`, guard clauses e retornos explícitos.
- Evite condições booleanas concatenadas, como `if (a || b || c && d)`. Divida decisões em verificações simples e métodos com nomes que expressem a regra de negócio. Não apenas transfira a mesma expressão complexa para uma variável.
- Preserve a precedência, o curto-circuito e os efeitos colaterais ao simplificar condições; não aumente o aninhamento para eliminar uma expressão.
- Em predicados EF, componha expressões pequenas e traduzíveis usando as extensions existentes; não introduza métodos C# arbitrários dentro de consultas SQL.
- Aplique essas orientações ao código novo ou alterado no escopo solicitado, sem refatoração global.
