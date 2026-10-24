# Identidade compartilhada no Olivaw

## Decisão

A identidade foi extraída para três bibliotecas em `olivaw-identity`, separadas do core genérico. Elas são referenciadas pelo template por `ProjectReference`, incluídas em `Olivaw.sln` e executadas dentro de cada API consumidora. Não há microservice central nem banco compartilhado de identidade.

Cada cliente mantém seu host, conexão, contexto, migrations e implantação. Os novos projetos precisam ficar ao lado das bibliotecas no monorepo; o script do template verifica o diretório de destino. O build Docker recebe a raiz do monorepo como contexto.

## Escopo extraído

O módulo implementa usuário, perfil de acesso, autorização e os dois vínculos, com CRUD, consulta paginada, validação, auditoria, índices únicos e remoção lógica. Perfil representa role; autorização representa permissão. Os contratos foram simplificados intencionalmente e não preservam as rotas/payloads da aplicação anterior.

A autenticação local usa hash de senha do ASP.NET Core, JWT com emissor/destinatário/chave do consumidor, bloqueio temporário por falhas e limite de requisições no login. Alteração de senha e logout revogam tokens por versão de segurança; logout revoga todas as sessões. Estado do usuário e permissões são verificados no banco, permitindo revogação durante a validade do JWT. Policies exigidas pelos endpoints do consumidor devem ser registradas pelo host.

O único usuário inicial é root, criado por bootstrap opcional com senha externa depois de preparar o schema. Root existente não tem a senha alterada pelo bootstrap. Nenhum perfil, permissão ou usuário corporativo é criado automaticamente.

A aplicação `ms-auth-api` e suas migrations antigas foram removidas após validar o substituto. Integrações de RH, AD, governo, notificações, concessões diretas ao usuário e seeds corporativos não foram transportados. Nenhuma migration nova foi gerada; nenhum banco real foi acessado ou alterado.

## Configuração e dependências

As configurações corporativas do template foram substituídas por configuração local sem senhas ou conexões versionadas. O cliente HTTP externo de autenticação deixou de ser necessário para inicializar o host. Certificados privados não são incluídos no template gerado nem no build Docker.

A integração também fixa `System.Security.Cryptography.Pkcs` em `9.0.21` no projeto Dados do template. A versão transitiva anterior, trazida pelo driver SQL Server, gerava os avisos NuGet NU1902/NU1903. A versão corrigida consta no [aviso da Microsoft](https://github.com/dotnet/announcements/issues/468) e no [advisory correspondente](https://github.com/advisories/GHSA-846f-f764-76mx). Não foi feita uma atualização geral das dependências.

## Verificações

- Solução `Olivaw.sln`: build Release sem erros ou warnings de compilação.
- Identidade: 85 testes aprovados, incluindo unitários e HTTP/SQLite em memória.
- Template: 59 testes aprovados, incluindo composição real do host, descoberta de rotas, segurança e registro das cinco entidades no contexto.
- Core: 226 testes existentes aprovados.
- Template gerado em uma pasta de validação: compilação da solução e referências compartilhadas conferidas; a pasta temporária é removida após a verificação.
- Bash do gerador e configuração Docker Compose validados.

O sandbox bloqueou a comunicação por socket do VSTest. As suítes xUnit v3 foram executadas pelo runner direto (`dotnet run --project ... -- -noLogo`). Para a suíte xUnit v2 existente do template, foi usado um runner temporário em `/tmp`, com `xunit.runner.utility` 2.9.3, sem alterar seu framework de testes.

O restore foi realizado com um feed temporário em `/tmp`, alimentado por pacotes NuGet oficiais e pelo cache disponível, porque o processo de restore não conseguia acessar a rede diretamente. A auditoria NuGet completa não foi executada nessa validação offline; o build não desabilita auditoria na configuração versionada.

O build/execução Docker não pôde ser realizado: o usuário do ambiente não tem acesso ao daemon, e a elevação é bloqueada por `no new privileges`. Não foram executados testes contra PostgreSQL ou SQL Server reais; os testes relacionais usam SQLite e o teste do template inspeciona os metadados do provedor sem conectar ao banco.

## Próximo uso

Consulte [Olivaw Identity](../olivaw-identity/README.md) e [geração do cliente](../ms-limpo-api/TEMPLATE.md). Antes da primeira inicialização real, defina o banco do cliente, crie/revise suas migrations, prepare o schema e configure segredos exclusivos. A publicação em produção permanece uma etapa separada.
