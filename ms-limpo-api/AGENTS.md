# Diretrizes de desenvolvimento — Owh.Limpo

Este arquivo contém as regras globais do repositório. Os arquivos `AGENTS.md` das camadas complementam estas regras no respectivo diretório. Em caso de conflito, siga a instrução mais específica, preserve o comportamento existente e registre no PR decisões arquiteturais que extrapolem as diretrizes.

## Objetivo e arquitetura

O `Owh.Limpo` é uma API ASP.NET Core organizada por Command–Query Separation (CQS), MediatR, FluentValidation, Repository Pattern e Entity Framework Core. PostgreSQL e SQL Server são suportados; Oracle permanece reservado e não deve ser presumido funcional sem implementação e testes equivalentes.

```text
Owh.Limpo.Api ───────> Owh.Limpo.Dados.<Provedor>
       │                         │
       ├─────────────> Owh.Limpo.Dados
       │                         │
       └─────────────────────────┴──> Owh.Limpo

Owh.Limpo.Tests ─────> Owh.Limpo.Api
```

Os projetos consomem `Owh.OlivawCore` (núcleo), `Owh.OlivawCore.Dados` (EF Core) e `Owh.OlivawCore.Api` (infraestrutura ASP.NET Core) por `ProjectReference` no monorepo. Não recrie tipos compartilhados, helpers ou extensões locais que apenas dupliquem esses pacotes. O `Owh.Limpo` fica disponível para regras próprias do novo microservice.

No projeto `src/Owh.Limpo`, mantenha sempre as pastas `Repositories/`, `Services/` e `Requests/`, cada uma com seu próprio `AGENTS.md`, inclusive quando ainda não houver código nelas. Elas são parte do esqueleto entregue aos microservices gerados por este template; siga as instruções locais antes de adicionar contratos.

- O núcleo não referencia infraestrutura nem apresentação.
- Dados não referencia a API.
- Código de ASP.NET Core não deve vazar para o núcleo.
- Código específico de banco pertence ao projeto do provedor.
- Use injeção por construtor; não use service locator ou dependências estáticas.
- Migrations pertencem ao projeto do provedor correspondente e não podem ser misturadas. Oracle não deve receber suporte presumido por similaridade.
- Para CRUD e consultas comuns, use primeiro `IRepository<T>` com expressões de filtro e os métodos básicos disponíveis. Não crie repository/reader específico sem necessidade concreta; consultas especializadas podem ter implementação própria baseada em `IRepository<T>` ou no `Repository<T>` existente.

## Prioridades globais

1. Manter regras de negócio independentes de transporte, framework web e banco.
2. Preservar a separação entre leitura e escrita.
3. Fazer mudanças pequenas, coesas, testáveis e compatíveis com contratos existentes.
4. Não inverter a direção das dependências.
5. Nunca incluir segredos, certificados privados, tokens ou credenciais em código, testes, logs ou commits.

## Comando Codex `new feature`

Quando o usuário escrever `new feature <nome-do-objeto-de-dominio>` ou `codex new feature <nome-do-objeto-de-dominio>`, trate isso como solicitação para implementar uma feature CQS completa para o objeto informado. Exemplo: `codex new feature usuario` cria a feature `UserProfile`.

O comando pode conter um bloco opcional de propriedades no formato `{ campo: tipo, modificadores }`, por exemplo `codex new feature usuario { nome: string, obrigatório, max: 150; email: string, obrigatório, único, email, max: 254 }`. Interprete `obrigatório`, `opcional`, `único`, `email`, `max`, `min`, `filtro`, `sensível`, valores default e relacionamentos como requisitos do domínio, validator, mapping, queries, responses e testes, conforme aplicável. O bloco descreve apenas propriedades específicas do objeto; não duplique `Id`, `ExternalId`, `ActorUserId`, `CreatedAt`, `UpdatedAt` ou `DeletedAt`, pois são herdados de `Entity`.

Antes de implementar, inspecione a entidade e a feature completa mais próxima e preserve os padrões atuais. Se o objeto ainda não existir e suas propriedades não forem informadas, procure contratos e linguagem já presentes no repositório; faça somente suposições conservadoras, informe-as durante o trabalho e peça definição quando a escolha alterar materialmente o contrato ou o schema.

Por padrão, o comando inclui:

1. Entidade ou ajustes de domínio no núcleo.
2. Mapeamento Fluent API e inclusão na facade de tabelas.
3. Extensions/mapeadores específicos da entidade.
4. Controller com rotas de recurso.
5. Commands `Create`, `Update`, `Delete` e `UpdateVisibility`, com requests, responses, handlers e validators próprios.
6. Queries `Read<Nome>ById` e `Read<Nome>ByFilter`, com requests, responses, handlers e validators próprios.
7. Propagação de `CancellationToken`, auditoria e remoção lógica conforme `Entity`.
8. Faker Builders e testes unitários de controller, handlers, validators, predicados e ramificações relevantes.
9. Build, testes proporcionais ao escopo e revisão final do diff.

O comando `new feature` nunca inclui criação, edição ou execução de migrations. As migrations são responsabilidade do mantenedor e só podem ser geradas mediante pedido explícito separado.

## Engenharia e manutenção

- Aplique responsabilidade única e mantenha coesão por feature.
- Prefira composição e abstrações pequenas a hierarquias profundas.
- Não crie classes base para commands ou queries apenas porque suas operações são parecidas. Mantenha cada caso de uso concreto e independente; herança só cabe quando representar uma relação real do domínio.
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
- Arquivos C# devem seguir o `charset` e o fim de linha definidos no `.editorconfig` mais próximo; o da raiz define UTF-8 com BOM e CRLF.
- Não introduza caracteres Unicode invisíveis ou de controle. Unicode textual intencional é permitido.
- Use nomes descritivos: `PascalCase` para tipos e membros; `camelCase` para parâmetros e variáveis; `_camelCase` para campos privados; prefixo `I` para interfaces.
- Métodos assíncronos usam sufixo `Async`, exceto contratos de framework como `Handle`.
- Mantenha `using` fora do namespace, use chaves quando melhorarem a segurança e preserve a organização do arquivo tocado.
- Use corpo com chaves em métodos que tenham várias instruções ou lógica que exija leitura detalhada; reserve `=>` para membros simples.
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
dotnet restore Limpo.sln
dotnet build Limpo.sln --no-restore
dotnet test tests/Owh.Limpo.Tests/Owh.Limpo.Tests.csproj --no-build
dotnet test tests/Owh.Limpo.Tests/Owh.Limpo.Tests.csproj --collect:"XPlat Code Coverage"
```

Mantenha a capitalização dos caminhos dos projetos na solução igual à dos diretórios reais; em sistemas case-sensitive, uma divergência impede o restore e o build. Não renomeie diretórios nem regrave a solução como efeito colateral de outras tarefas.

Antes de concluir:

1. Confirme que não há erros nem warnings novos.
2. Revise o diff e remova artefatos, logs, credenciais e alterações acidentais.
3. Garanta testes isolados para o comportamento novo.
4. Atualize documentação e configuração quando contratos ou procedimentos mudarem.
5. Informe comandos executados e validações não realizadas.

## Versionamento e revisão

- Siga o Git Flow clássico do [monorepo](../docs/gitflow.md): `feature/issue001-descricao`, releases versionadas e hotfixes versionadas com issue associada. Preserve o uso de `finish`; PRs são opcionais.
- Commits devem ser pequenos, coerentes e descrever a intenção.
- PRs devem explicar motivação, solução, contratos/schema/configuração afetados, testes e riscos.
- Prefira PRs com até 500 linhas adicionadas de lógica de produção e não ultrapasse 700. Testes não entram nessa contagem. Divida mudanças maiores ou justifique na descrição por que isso não é seguro.

## Legibilidade de decisões

- Evite ternários e condições booleanas concatenadas no código novo ou alterado. Prefira guard clauses e métodos com nomes que expressem a regra.
- Ao simplificar condições, preserve precedência, curto-circuito e efeitos colaterais sem aumentar o aninhamento.
- Em predicados EF, componha expressões pequenas e traduzíveis; não introduza métodos C# arbitrários dentro de consultas SQL.

## Guia para um futuro cliente NuGet

Quando este template receber um cliente REST completo, siga também `src/Owh.Limpo.Client/AGENTS.md` e confira os contratos reais da API antes de implementá-lo. A presença dessas diretrizes não cria o projeto nem autoriza migração de contratos existentes.

1. Inventarie operações, controllers, requests, responses, validators, handlers, mapeadores e testes. Preserve verbos, rotas, status, JSON, enums e significado dos IDs.
2. Defina cada contrato público uma única vez no projeto cliente, em `Features/<Recurso>Feature/Commands` ou `Queries`. Payloads e responses são classes simples, sem MediatR, ASP.NET Core, herança de entidades ou auditoria interna.
3. Na API, componha o payload público em adaptadores concretos `*MediatorCommand` ou `*MediatorQuery`. Apenas o adaptador implementa `IRequest<TResponse>`; auditoria obtida do contexto autenticado fica nele.
4. Faça handlers lerem `request.Payload`, traduzirem enums públicos para o domínio, aplicarem auditoria explicitamente e propagarem `CancellationToken`.
5. Valide o payload público com FluentValidation e delegue a validação no adaptador, preservando nomes de campos sem o prefixo `Payload.`. Verificações dependentes de banco permanecem na API.
6. Controllers recebem payloads HTTP planos e enviam os adaptadores ao MediatR. A composição interna não altera JSON, rotas ou status.
7. Migre mapeadores, chamadas internas e testes junto aos contratos. Evite duplicar requests e responses entre API e cliente ou esconder duplicação com aliases.
8. Mantenha as rotas base em um catálogo JSON interno embutido, com caminhos relativos. O cliente constrói IDs e filtros, omite nulos, preserva `false` e listas, escapa valores e usa cultura invariável.
9. Exponha operações tipadas e reutilize o transporte e a autenticação existentes. Não implemente obtenção ou armazenamento de JWT no pacote.
10. Teste HTTP em memória, autenticação, serialização, cancelamento, compatibilidade de payloads e correspondência das rotas com os controllers. Execute build, testes e `dotnet pack`; não publique sem pedido explícito.

## Identidade compartilhada

- Root de cada cliente é exclusivo da operação Olivaw; clientes usam contas sem root e perfis com somente as permissões necessárias. Cada serviço e ambiente possui chave JWT própria, sem reutilizar a chave do template, Olivaw ou DevOps.

- Usuários, autenticação, perfis e permissões vêm de `../olivaw-identity`, por `ProjectReference`; não copie essas features para cada cliente.
- O contexto local aplica `AddOlivawIdentity()`. Banco e migrations continuam no consumidor.
- O módulo registra o esquema JWT local e controllers por application part. Não reintroduza o cliente externo de autenticação como requisito de inicialização.
- Senhas não podem passar pelo pipeline que aplica `Trim()`. Preserve a delegação de `TemplateValidationBehavior`.
- Bootstrap cria somente root, com segredo externo, depois de preparar o schema. Não transporte migrations ou seeds corporativos.

## Configuração versionada nesta etapa

Por decisão explícita do mantenedor, os dotenv de todos os projetos acompanham o Git privado até a futura migração para Key Vault. Essa exceção às diretrizes gerais de credenciais aplica-se aos arquivos de configuração de serviço; não autoriza registrar seus valores em logs, documentação ou respostas, nem versionar chaves SSH, credenciais AWS, tokens pessoais ou dados de clientes.

## Padronização de migrations

- Gere migrations somente mediante pedido explícito; `new feature` não inclui essa etapa.
- Confira a branch atual. Use a issue em PascalCase como nome: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se já existir uma migration com esse nome, preserve o histórico e acrescente um sufixo descritivo, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome com o mantenedor.
- Gere somente para os provedores solicitados, no projeto do provedor da API consumidora. O Core não contém migrations. Não copie históricos entre serviços.
- Use a factory de design time quando disponível, sem iniciar o host nem acessar bancos reais. Compile com `-m:1`; use `--no-build` no EF somente após compilar o modelo atual. `--no-restore` pressupõe dependências restauradas.
- Revise migration, designer, snapshot e SQL entre a migration anterior e a nova. Preserve BOM UTF-8 e CRLF dos arquivos C#. Não altere migrations antigas já publicadas.
- Gerar e revisar não autoriza `database update`, conexão a bancos reais ou redeploy. O startup das APIs aplica as migrations pendentes; registre essa consequência antes da publicação.
- Documente comandos reproduzíveis no README da API e consulte os [comandos do Core](../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores). Informe as validações executadas e os provedores não contemplados.

## Limites de versões por licenciamento

Por decisão explícita do mantenedor, manter **MassTransit e seus transportes em 8.5.x** e **MediatR em 12.5.x**, preservando as linhas escolhidas para uso gratuito. Não atualizar essas famílias para outra minor/major, nem adicionar pacote que exija versões fora desses intervalos, sem nova autorização explícita.

- Usar versões estáveis explícitas nos projetos; alinhar todos os pacotes `MassTransit.*` na mesma versão 8.5.x.
- Conferir dependências transitivas após restore, além das referências diretas. Não usar atualização automática irrestrita para contornar os limites.
- A regra inclui Core, identidade, APIs, testes, template e serviços gerados. Ao atualizar dependências, verificar também compatibilidade entre EF Core, provedores e frameworks.
