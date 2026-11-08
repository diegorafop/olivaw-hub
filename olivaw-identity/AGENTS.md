# Diretrizes — Olivaw Identity

## Organização e dependências

Este diretório agrupa bibliotecas compartilhadas, não um microservice. `Olivaw.Identity` contém domínio e abstrações; Dados implementa persistência no contexto do consumidor; Api contém CQS, controllers e segurança. O domínio referencia apenas o core genérico, sem ASP.NET Core ou EF Core. Não introduza host, conexão de banco, migrations ou integrações corporativas neste módulo.

## Contratos e segurança

- Perfil representa role; autorização representa uma chave de permissão. Concessões passam pelos dois vínculos existentes, sem autorização direta ao usuário.
- Preserve GUIDs públicos nas rotas e payloads; chaves numéricas permanecem internas.
- Senhas preservam todos os caracteres e usam `IPasswordHasher<UserProfile>`. Não aplique `Trim()` em senha nem exponha hashes, stamps ou campos internos nas respostas.
- Auditoria vem de `CurrentActor`, nunca do body HTTP. Root só pode nascer pelo bootstrap; não exponha sua atribuição pelo CRUD.
- Na autoridade local, JWT é validado pelo esquema do módulo e pelo estado atual no banco. Preserve validação por stamp, perfis ativos e remoção lógica.
- Nos serviços consumidores internos, valide JWT localmente com o mesmo secret, emissor e audiência do Olivaw. Use os claims de root, conta técnica e permissões até a expiração. Não consulte o Olivaw nem seu banco por requisição; mudanças de acesso exigem novo token.
- Contas técnicas de clientes vivem no Olivaw e são independentes das contas humanas dos clientes. Consumidores não registram CRUD, banco de identidade ou bootstrap.

## Desenvolvimento e validação

Use quatro espaços, UTF-8 e CRLF em C#. Mantenha commands e queries concretos e independentes, respostas próprias e controllers finos. Propague `CancellationToken`; valide entradas e limite listagens. Evite consultas adicionais por item: carregue os relacionamentos necessários na consulta paginada.

Execute na raiz do monorepo:

```bash
dotnet build Olivaw.sln -c Release
dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release --no-build
```

Teste regras em isolamento e use HTTP em memória/SQLite para verificar integração, índices, políticas e isolamento entre contextos. SQLite não comprova comportamento específico dos bancos de produção. Mudanças compartilhadas exigem também os testes do core e do template. Não publique pacotes nem altere versões como efeito colateral.

## Limites de versões por licenciamento

Por decisão explícita do mantenedor, manter **MassTransit e seus transportes em 8.5.x** e **MediatR em 12.5.x**, preservando as linhas escolhidas para uso gratuito. Não atualizar essas famílias para outra minor/major, nem adicionar pacote que exija versões fora desses intervalos, sem nova autorização explícita.

- Usar versões estáveis explícitas nos projetos; alinhar todos os pacotes `MassTransit.*` na mesma versão 8.5.x.
- Conferir dependências transitivas após restore, além das referências diretas. Não usar atualização automática irrestrita para contornar os limites.
- A regra inclui Core, identidade, APIs, testes, template e serviços gerados. Ao atualizar dependências, verificar também compatibilidade entre EF Core, provedores e frameworks.
