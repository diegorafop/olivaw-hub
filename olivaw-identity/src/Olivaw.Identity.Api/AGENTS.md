# Diretrizes — API e segurança de identidade

Estas regras complementam o [guia do módulo](../../AGENTS.md).

## Organização

Esta biblioteca `net10.0` fornece controllers por MVC application parts, handlers MediatR e serviços compartilhados. O host é a API do cliente. Não introduza `Program.cs`, servidor próprio ou configuração de conexão aqui.

Organize operações em `Features/<Recurso>Feature/Commands` e `Queries`, com requests e responses concretos. Controllers delegam ao mediator e propagam `CancellationToken`. Não retorne entidades persistidas diretamente.

## Segurança

- Preserve auditoria obtida de `CurrentActor` e IDs públicos nas rotas. Não aceite privilégio root, hash ou versão de segurança pelo payload administrativo.
- Valide comandos e preserve senhas integralmente; não aplique o pipeline que executa `Trim()` nelas.
- Use o esquema JWT registrado pelo módulo. Estado do usuário, versão de segurança e permissões vigentes devem continuar protegendo tokens já emitidos.
- Mudanças em policies, root, login, bloqueio, senha ou logout exigem testes de concessão e negação, incluindo sessões existentes.
- Respostas e erros não podem expor segredos ou detalhes internos do banco. Preserve os status e contratos documentados.

## Validação

Execute a suíte de identidade e a integração do template quando alterar registro de DI, descoberta de controllers ou segurança. Atualize o README do módulo se mudar rotas, configuração ou comportamento público. Não publique pacotes nem crie migrations nesta camada.

## Limite do domínio

Não crie pasta `Domain` neste projeto `.Api`. Entidades e invariantes de identidade pertencem ao núcleo `Olivaw.Identity`. Modelos internos de casos de uso permanecem junto às features; detalhes de persistência pertencem à camada Dados.
