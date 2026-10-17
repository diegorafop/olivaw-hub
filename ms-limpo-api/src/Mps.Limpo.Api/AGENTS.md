# API e casos de uso — Mps.Limpo.Api

Estas regras complementam o `AGENTS.md` da raiz para apresentação, CQS e integrações.

- Target: `net10.0`, ASP.NET Core.
- A API é o composition root e pode depender do núcleo, Dados e provedores suportados.
- `Program.cs` deve permanecer mínimo; configuração transversal pertence às extensions/facades existentes.
- Use extensões compartilhadas de `Mps.HolosCore.Api` diretamente. Configuração própria do host fica em `Extensions/Hostings`; não recrie uma pasta `Extensions/Generics`.
- Organize endpoints por feature em `Features/<Nome>Feature`.

## Features e CQS

```text
Features/<Nome>Feature/
├── <Nome>Controller.cs
├── Commands/
│   └── <Acao><Nome>.cs
├── Queries/
│   └── <Acao><Nome>.cs
├── Events/
│   └── <NomeDoEvento>.cs
└── Internals/
    └── <ComponenteInterno>.cs
```

`Events` e `Internals` são opcionais. Use `Events` para eventos publicados/consumidos e `Internals` para componentes exclusivos da implementação da feature. Abstrações compartilhadas ou de domínio não pertencem a `Internals`.

- Command altera estado; Query apenas lê e não causa efeito colateral observável.
- Nomeie operações em português com verbo no infinitivo: `InserirUsuarioCommand`, `SelecionarUsuarioQuery`.
- Para leitura CRUD, use `Selecionar`; não alterne entre `Buscar`, `Consultar` e `Obter` sem razão de domínio.
- Use classes próprias para entrada e saída de cada operação, inclusive quando vazias. Não retorne diretamente tipos primitivos ou coleções de handlers CQS.
- Não crie hierarquias entre commands ou queries para compartilhar implementação.
- Cada operação concreta declara request, response, handler e validator próprios, normalmente no mesmo arquivo quando coesos.
- Query não persiste, publica comandos implícitos nem modifica estado externo.
- Command retorna apenas o necessário para confirmar ou identificar o resultado.

## Controllers e rotas

- Controllers são finos: recebem HTTP, enviam pelo `IMediator` e traduzem a resposta.
- Não injete repositórios no controller para executar casos de uso.
- Propague o `CancellationToken` em `_mediator.Send(request, cancellationToken)`.
- Modele rotas com substantivos e recursos do domínio; evite verbos, nomes de métodos e operações técnicas.
- Mantenha rotas rasas. Evite cadeias como `/usuario/{usuarioId}/endereco/{enderecoId}/cidade`; promova o recurso relacionado a uma rota própria.
- A API abstrai casos de uso, não tabelas. Não exponha automaticamente uma rota por tabela ou relacionamento do schema.
- Preserve autenticação negada por padrão; use `[AllowAnonymous]` somente para endpoints deliberadamente públicos.

## Handlers, validação e respostas

- Handlers coordenam o caso de uso e dependem de abstrações injetadas.
- Propague o token até repositórios, EF Core e clientes externos.
- Mantenha `ValidationHelper.ThrowIfNull(request)` como proteção defensiva.
- Entrada pertence ao validator; invariantes ao domínio; verificações dependentes de persistência ao caso de uso/repositório.
- Prefira FluentValidation e o fluxo de `ValidationException`/`ValidationProblemDetails` para falhas esperadas.
- Diferencie validação, não encontrado, não autorizado, conflito e falha inesperada sem expor stack traces ou detalhes internos.
- Use `MessageHelper`; não espalhe mensagens hard-coded quando houver helper ou necessidade de padronização.
- Não use `throw` no controller/handler para substituir validação esperada. Exceções são adequadas para argumentos nulos defensivos, invariantes não representáveis e falhas inesperadas.
- Nunca devolva entidades EF nem `null` como response CQS. Use responses próprias e coleções vazias.
- O handler sinaliza ausência/falha pelo mecanismo centralizado; controller não infere `NotFound` de response nula.
- O pipeline `FluentValidatorBehavior<,>` valida e normaliza strings com `Trim()`.

## Mapeamento e integrações

- Conversões reutilizáveis ficam em extensions/mapeadores específicos; regras de negócio não viram extensions genéricas.
- Configuração pertence a `Settings` e deve ser registrada em DI.
- Leia configuração por `IConfiguration`/Settings, valide valores obrigatórios no startup ou no limite de uso e nunca revele o valor secreto na mensagem.
- Segredos vêm de variáveis de ambiente, secret stores ou user secrets; `appsettings.*` versionado contém somente valores não sensíveis ou placeholders.
- Use `IHttpClientFactory` e reutilize `IServiceHttpClient`/`ServiceHttpClient`, com timeout, cancelamento e validação de resposta.
- Integrações externas ficam em Services dedicados atrás de interfaces do núcleo.
- Publicação usa abstrações do MassTransit e a configuração existente do broker.
- Logs devem ser estruturados, úteis e sem dados pessoais desnecessários ou segredos.
- Health/readiness deve refletir dependências reais e preservar o formato da `ReadyFeature`.

## Execução e ambientes

- Execute localmente com `dotnet run --project src/Mps.Limpo.Api/Mps.Limpo.Api.csproj`.
- Ambientes reconhecidos: Development, Quality, Release, Staging e Production.
- Em Docker, mantenha valores comuns no compose base e somente diferenças nos overrides de ambiente.
- Ao alterar Docker, valide build, health/readiness, variáveis, portas e execução sem privilégios quando suportada.
- Não edite ou substitua certificados reais durante tarefas comuns.

## Evolução para cliente público

- Se um cliente REST for criado, mantenha payloads públicos planos e independentes de MediatR. A API pode recebê-los por composição em adaptadores internos, mantendo auditoria fora do body HTTP.
- Migre controllers, handlers, validators, mapeadores e testes junto aos contratos expostos. Preserve rotas, status, JSON e nomes públicos dos erros de validação.
- Não aplique regras específicas do `Mps.Auth.Client` a este template sem conferir a implementação existente e o escopo solicitado.
