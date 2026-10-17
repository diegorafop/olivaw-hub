# Diretrizes — Olivaw Identity

## Organização e dependências

Este diretório agrupa bibliotecas compartilhadas, não um microservice. `Olivaw.Identity` contém domínio e abstrações; Dados implementa persistência no contexto do consumidor; Api contém CQS, controllers e segurança. O domínio referencia apenas o core genérico, sem ASP.NET Core ou EF Core. Não introduza host, conexão de banco, migrations ou integrações corporativas neste módulo.

## Contratos e segurança

- Perfil representa role; autorização representa uma chave de permissão. Concessões passam pelos dois vínculos existentes, sem autorização direta ao usuário.
- Preserve GUIDs públicos nas rotas e payloads; chaves numéricas permanecem internas.
- Senhas preservam todos os caracteres e usam `IPasswordHasher<Usuario>`. Não aplique `Trim()` em senha nem exponha hashes, stamps ou campos internos nas respostas.
- Auditoria vem de `CurrentActor`, nunca do body HTTP. Root só pode nascer pelo bootstrap; não exponha sua atribuição pelo CRUD.
- JWT é validado pelo esquema do módulo e pelo estado atual no banco. Preserve revogação por stamp, perfis ativos e remoção lógica.
- Mudanças de autorização devem valer para tokens existentes. Não substitua a verificação por claims antigas sem definir e testar a revogação.

## Desenvolvimento e validação

Use quatro espaços, UTF-8 e CRLF em C#. Mantenha commands e queries concretos e independentes, respostas próprias e controllers finos. Propague `CancellationToken`; valide entradas e limite listagens. Evite consultas adicionais por item: carregue os relacionamentos necessários na consulta paginada.

Execute na raiz do monorepo:

```bash
dotnet build OlivawHub.sln -c Release
dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release --no-build
```

Teste regras em isolamento e use HTTP em memória/SQLite para verificar integração, índices, políticas e isolamento entre contextos. SQLite não comprova comportamento específico dos bancos de produção. Mudanças compartilhadas exigem também os testes do core e do template. Não publique pacotes nem altere versões como efeito colateral.
