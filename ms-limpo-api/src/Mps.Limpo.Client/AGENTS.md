# Cliente REST — Mps.Limpo.Client

Estas diretrizes vieram do `Mps.Auth.Client` e orientam a implementação futura do cliente deste template. O projeto `Mps.Limpo.Client` existe apenas como biblioteca base; não presuma que seus contratos ou serviços estejam implementados.

- Quando solicitado, crie uma biblioteca NuGet com payloads públicos e cliente HTTP, separada da execução da API.
- Organize payloads em `Features/<Nome>Feature/Commands` e `Queries`, com nomes `*Command`, `*Query` e respectivas responses. Não acrescente sufixo `Contract`.
- Payloads são classes simples, sem `IRequest`, herança de entidades, regras de negócio, auditoria interna ou atributos ASP.NET Core.
- O cliente não referencia API, núcleo, Dados ou provedores. Handlers, validators de negócio, pipelines e persistência ficam no servidor.
- Enums públicos ficam em `Enums`, com valores numéricos explícitos e estáveis. A API traduz esses valores para os enums do domínio.
- Exponha métodos tipados por operação com payload próprio e `CancellationToken`. Reutilize o transporte HTTP existente quando aplicável; não duplique infraestrutura genérica.
- Configure endereço, autenticação e timeout no consumidor. Não embuta credenciais nem políticas de retry que possam repetir escritas.
- Preserve rotas, verbos, JSON, status e semântica das operações publicadas. Não introduza wrappers de resposta que alterem o contrato HTTP.
- Defina contratos públicos em um único projeto. A API pode recebê-los por adaptadores concretos `*MediatorCommand` e `*MediatorQuery`, compostos com o payload; apenas os adaptadores implementam `IRequest`.
- Validators da API devem alcançar o payload composto sem acrescentar `Payload.` aos nomes públicos dos erros. Não compense a composição com `Trim()` manual nos handlers.
- Leia rotas de um catálogo JSON embutido e interno, com caminhos relativos. O consumidor configura transporte, não endpoints individuais.
- Construa IDs, sufixos e query strings a partir do payload. Omita filtros nulos, preserve `false` e listas, escape nomes e valores e use cultura invariável.
- Preserve a abstração de autenticação do transporte existente; não implemente obtenção ou armazenamento de JWT no cliente.
- Dentro da solução, a API deve referenciar o projeto cliente por `ProjectReference`; o núcleo permanece independente dele.
- Use quatro espaços, chaves em linhas próprias e o estilo local. Formate somente arquivos alterados.
- Ao ampliar a superfície pública, atualize README e testes. Teste HTTP em memória, incluindo verbo, caminho, filtros, body plano, autenticação, desserialização e prefixo da URL.
- Valide build, testes e `dotnet pack`. Não publique sem solicitação explícita.

Siga também o guia da raiz em `../../AGENTS.md` e confira a implementação real da API antes de criar o cliente.
