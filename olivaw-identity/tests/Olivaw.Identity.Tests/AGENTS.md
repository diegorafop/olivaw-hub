# Diretrizes — Testes de identidade

Estas regras complementam o [guia do módulo](../../AGENTS.md).

## Ferramentas e organização

Use xUnit v3, Moq, ASP.NET Core TestHost e SQLite conforme o projeto existente. Separe testes de validators, serviços e delegação dos controllers dos fluxos HTTP e relacionais. `IdentityHost` prepara um banco em memória por host, sem serviços externos.

## Cenários relevantes

- Verifique sucesso e negação de acesso, isolamento entre bancos e efeitos de alterações sobre tokens existentes.
- Cubra bootstrap idempotente, proteção do root, senhas preservadas, bloqueio por falhas, revogação e vínculos removidos/restaurados quando afetados.
- Use assertions específicas sobre conteúdo, status e chamadas; não substitua testes de comportamento por reflexão sobre nomes de classes.
- Nos testes de delegação, confira o token enviado ao mediator. Use `Xunit.TestContext.Current.CancellationToken` nas demais chamadas assíncronas apropriadas.
- Não dependa de relógio exato, ordem de execução ou recursos compartilhados. Credenciais de teste devem ser claramente fictícias.

## Execução

Da raiz: `dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release`. Para o runner direto, consulte o README do módulo. Não reporte SQLite ou chamadas diretas de controller como validação completa do banco de produção ou dos middlewares HTTP.
