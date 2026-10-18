# Organização e desenvolvimento do monorepo

## Visão geral

O Olivaw Hub mantém o template, as bibliotecas compartilhadas e as APIs dos clientes no mesmo repositório Git. Cada cliente tem aplicação, configuração, banco e implantação próprios. Compartilhar código não compartilha automaticamente dados nem obriga todos os clientes a serem publicados juntos.

```text
olivaw-hub/
  OlivawHub.sln              # Agrupamento para desenvolvimento conjunto
  ms-olivaw-core-api/        # Bibliotecas genéricas e solução OlivawCore.sln
  olivaw-identity/           # Bibliotecas de identidade e seus testes
  ms-limpo-api/              # Template e solução Limpo.sln
  ms-limpo-ng/               # Template Angular e collection Schematics
  ms-olivaw-hub-ng/          # Site institucional derivado do template
  docs/                     # Arquitetura, decisões e fluxo de trabalho
  ms-pastelaria-joao-api/    # Exemplo futuro; gerado quando necessário
```

O exemplo da pastelaria não representa um cliente já criado. As pastas de clientes devem ficar diretamente na raiz, ao lado dos módulos compartilhados, porque os caminhos de `ProjectReference` são relativos.

## Pasta, projeto, referência e solução

| Elemento | Função no repositório |
| --- | --- |
| Pasta | Organiza arquivos e pode agrupar vários projetos |
| `.csproj` | Define uma unidade de compilação: biblioteca, API executável ou testes |
| `ProjectReference` | Declara que um projeto depende de outro projeto do monorepo |
| `PackageReference` | Declara uma dependência de pacote NuGet, usada para bibliotecas externas |
| `.sln` | Lista os projetos que queremos abrir, compilar ou testar em conjunto |
| DLL | Resultado da compilação de um projeto; não é copiada manualmente entre clientes |

`olivaw-identity` agrupa domínio, Dados e Api em três `.csproj`, mais um projeto de testes. Mesmo com o sufixo `.Api`, essa biblioteca não tem host próprio: seus controllers e handlers executam dentro da API consumidora. O [guia de identidade](../olivaw-identity/README.md) detalha essa integração.

## Por que existe OlivawHub.sln

A solução da raiz é uma conveniência de desenvolvimento. Atualmente inclui os projetos do core, identidade e template, com seus testes. Ela permite abrir esse conjunto na IDE ou validá-lo por um único comando. Não possui endpoints, banco, processo de execução ou implantação próprios.

As dependências reais continuam declaradas nos `.csproj`. Compilar diretamente o projeto de uma API também compila suas dependências, mesmo sem uma solução da raiz. Uma `.sln` pode ser removida ou reorganizada sem eliminar essas referências.

| Necessidade | Entrada recomendada |
| --- | --- |
| Trabalhar nos componentes compartilhados e no template | `OlivawHub.sln` |
| Trabalhar somente no core | `ms-olivaw-core-api/OlivawCore.sln` |
| Trabalhar no template | `ms-limpo-api/Limpo.sln` |
| Trabalhar em um cliente | Solução ou `.csproj` da API desse cliente |
| Trabalhar somente na identidade | Abrir seus `.csproj` ou usar a solução da raiz |

Não existe atualmente uma solução exclusiva em `olivaw-identity`. Novos clientes não entram automaticamente em `OlivawHub.sln`: inclua-os quando fizer sentido para o conjunto de trabalho. Conforme o repositório crescer, podemos manter a solução principal concentrada nos módulos compartilhados e no template, validando os demais consumidores por comandos próprios.

## Onde implementar cada mudança

- **Core:** contratos e infraestrutura genéricos, sem regras particulares de cliente ou identidade.
- **Identidade:** usuários, autenticação, perfis, permissões e vínculos reutilizados pelas APIs.
- **Template:** estrutura inicial, composição e exemplos para gerar novos clientes.
- **API do cliente:** funcionalidades do negócio, integrações específicas, contexto e configuração próprios.

Uma correção compartilhada deve ser feita na biblioteca correspondente. Alterações no template afetam futuras gerações; não atualizam automaticamente o código já copiado para clientes existentes. Esses clientes precisam receber manualmente ajustes de composição ou configuração quando aplicáveis.

## Fluxo diário

Os comandos abaixo partem da raiz do monorepo e exigem SDK .NET 10. O restore precisa acessar NuGet para dependências externas; não precisamos publicar nossos módulos em um feed privado.

```bash
dotnet build OlivawHub.sln -c Release
dotnet test OlivawHub.sln -c Release --no-build
```

Use `--no-build` somente depois de compilar a mesma configuração. Para uma alteração localizada, compile e teste os projetos afetados. Por exemplo:

```bash
dotnet build olivaw-identity/src/Olivaw.Identity.Api/Olivaw.Identity.Api.csproj -c Release
dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c Release
```

Mudanças no core ou na identidade exigem validar também o template e os clientes afetados, inclusive os que não estiverem na solução da raiz. O build de uma biblioteca não executa testes nem comprova compatibilidade com todos os consumidores. Registre os testes executados e limitações do ambiente; as alternativas usadas no sandbox estão no [registro de identidade](identidade.md#verificações).

## Gerar e desenvolver um cliente

```bash
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
```

O resultado será `ms-pastelaria-joao-api`, com `PastelariaJoao.sln` e projetos `Owh.PastelariaJoao.*`. O destino não pode existir. O script gera arquivos; não cria banco, migrations, segredos ou implantação.

Se desejar incluir a API na solução principal, adicione seu `.csproj` explicitamente. Isso não adiciona automaticamente o projeto de testes à solução:

```bash
dotnet sln OlivawHub.sln add ms-pastelaria-joao-api/src/Owh.PastelariaJoao.Api/Owh.PastelariaJoao.Api.csproj
dotnet sln OlivawHub.sln add ms-pastelaria-joao-api/tests/Owh.PastelariaJoao.Tests/Owh.PastelariaJoao.Tests.csproj
```

Configure a conexão e os segredos exclusivos, prepare o schema no projeto do provedor e execute o bootstrap de root conforme o [guia de identidade](../olivaw-identity/README.md#configuração-e-usuário-inicial). Depois:

```bash
dotnet build ms-pastelaria-joao-api/PastelariaJoao.sln -c Release
dotnet test ms-pastelaria-joao-api/tests/Owh.PastelariaJoao.Tests/Owh.PastelariaJoao.Tests.csproj -c Release --no-build
dotnet run --project ms-pastelaria-joao-api/src/Owh.PastelariaJoao.Api/Owh.PastelariaJoao.Api.csproj -c Release --no-launch-profile
```

Esses caminhos são exemplos válidos após a geração. A documentação do [gerador](../ms-limpo-api/TEMPLATE.md) complementa esse fluxo.

## Banco, publicação e evolução

As entidades compartilhadas são mapeadas no `DbContext` de cada cliente. O módulo de identidade não fornece conexão própria. Migrations pertencem ao consumidor e ao respectivo provedor; sua geração e aplicação exigem escopo explícito. O bootstrap de root depende de tabelas já preparadas e não substitui migrations.

O build Docker deve receber a raiz do monorepo como contexto para alcançar as bibliotecas referenciadas. Consulte os comandos no [README do template](../ms-limpo-api/README.md#docker). Segredos ficam no ambiente ou secret store.

Com `ProjectReference`, um build usa o código compartilhado presente naquele checkout. Clientes compilados do mesmo estado do repositório não selecionam versões independentes dessas bibliotecas. Registrar o commit implantado permite reproduzir cada entrega; publicar uma API não altera automaticamente os outros clientes em produção.

Se precisarmos manter linhas de versões diferentes por cliente ou consumir as bibliotecas fora do monorepo, podemos passar a distribuí-las por NuGet. Por enquanto, `ProjectReference` reduz as etapas de publicação e facilita corrigir e validar os consumidores juntos.

## Git e configuração do editor

O `.gitignore` da raiz contém as regras comuns do monorepo: `bin`, `obj`, `dist`, artefatos, relatórios, logs, estado local das IDEs e certificados privados. Os arquivos `.env` de todos os projetos são versionados no Git privado nesta etapa; Key Vault fica planejado para o futuro. Os padrões genéricos alcançam qualquer profundidade, incluindo futuros clientes. Arquivos `.env.example` e `.env.<ambiente>.example` permanecem disponíveis para versionamento.

O core e a identidade usam esse ignore central. O template mantém também seu próprio `.gitignore`, com as mesmas categorias e sem nomes de projetos, para que os clientes gerados recebam regras úteis mesmo se forem extraídos depois. Ao mudar as regras comuns, mantenha a cópia do template alinhada. Dentro deste monorepo, não é necessário criar outro ignore por cliente sem uma necessidade específica.

A configuração compartilhada do VS Code está em `.vscode/settings.json`, na raiz, e deve permanecer versionada. Abra `olivaw-hub` como pasta de trabalho para usar essa configuração em todo o repositório. Foram removidas as cópias internas idênticas do core e do template. Se abrir somente uma subpasta como workspace, a configuração `.vscode` da raiz não será aplicada automaticamente; nesse caso, use as configurações pessoais do editor ou crie ajustes locais justificados. Os `.editorconfig` existentes continuam definindo as convenções dos arquivos. Veja a [documentação de configurações do VS Code](https://code.visualstudio.com/docs/configure/settings).

Excluir um diretório no Explorer ou na busca do VS Code não o ignora no Git. Da mesma forma, o `.dockerignore` controla o contexto do Docker e não substitui o `.gitignore`.

Confira uma regra sem criar arquivos de teste:

```bash
git check-ignore -v --no-index olivaw-identity/src/Olivaw.Identity/bin/Release/exemplo.dll
git check-ignore -v --no-index ms-limpo-api/src/Owh.Limpo.Api/bin/Release/exemplo.dll
```

Arquivos já rastreados continuam no índice mesmo quando passam a corresponder ao ignore. O ignore não remove arquivos do histórico nem deixa de rastreá-los automaticamente; trate essa limpeza separadamente e preserve os arquivos locais necessários. Consulte a [documentação do Git](https://git-scm.com/docs/gitignore).

## Instruções para agentes e contribuidores

Leia o `AGENTS.md` da raiz e os guias aplicáveis ao diretório alterado. Os arquivos locais detalham responsabilidades, convenções e validação. Não é necessário criar um guia em toda pasta: use-os quando houver regras próprias, como segurança, persistência, testes ou geração de projetos.

O guia da raiz ainda descreve a fase inicial de planejamento; os caminhos e fluxos implementados estão documentados aqui e nos READMEs dos módulos. Os `AGENTS.md` existentes foram preservados nesta atualização.

## Inglês como padrão para novas features de APIs

A partir desta decisão, novas features das APIs e dos projetos gerados pelo template usam inglês nos nomes de entidades, propriedades, contratos, casos de uso, testes e recursos HTTP. Para CRUD, use `Create`, `Read`, `Update` e `Delete`, por exemplo `CreateCustomerCommand`, `ReadCustomerByIdQuery` e `ReadCustomersByFilterQuery`. Preserve CQS: escrita em Commands, leitura em Queries; operações específicas do domínio também devem ter nomes em inglês.

O código existente em português será refatorado em uma tarefa futura, com revisão de compatibilidade dos contratos e rotas. Não renomeie esse legado automaticamente ao desenvolver funcionalidades. Nesta alteração, `CreateDeployment`, `/deployments` e `devops.deploy` permanecem; o payload solicitado passa a usar `folder`, `branch` e `environment`. Documentação e mensagens de erro podem continuar em português.

## Frontend e backend do cliente

`ms-limpo-ng` e `ms-limpo-api` formam o par de templates. O frontend tem ciclo de instalação, testes e build próprio com npm e não pertence à solução .NET. Frontend e API são serviços separados, com builds, imagens e publicações independentes. A configuração pública `ms-limpo-ng/public/config.json` aponta diretamente para `http://localhost:5137` no desenvolvimento e deve receber a URL pública HTTPS da API em produção. O navegador chama essa URL, sem proxy Angular/Nginx e sem rede Docker compartilhada obrigatória. A API deve permitir a origem do frontend via CORS. Outra URL pode ser fornecida em runtime sem novo build do frontend.

A página pública inicial é prerenderizada; login e administração são renderizados no navegador. O estado `autenticacao` usa NgRx SignalStore e os contratos existentes de login, acesso e logout. Não há refresh token nem persistência de sessão nesta etapa. Funcionalidades de eventos, fotos e agenda serão adicionadas nos projetos consumidores; A geração do frontend usa a collection local do template; o comando está no README de `ms-limpo-ng`. Consulte o [guia do frontend](../ms-limpo-ng/README.md) para comandos, tema, testes e limites da publicação de conteúdo com prerenderização.


### Acesso administrativo dos clientes

Root de cada cliente permanece exclusivo da operação Olivaw Hub. Contas entregues ao cliente recebem um perfil com permissões de negócio específicas; acesso ao portfólio não concede administração de identidade. A identidade fica na API e na base do próprio cliente. Chave de assinatura JWT, emissor e destinatário são exclusivos do serviço e ambiente; a senha de um usuário não é a chave de assinatura.


### Nomes de tabelas de domínio

Tabelas usam somente letras minúsculas, sem hífens ou underscores. Exemplos: `portfolioevent`, `portfoliophoto`, `accessprofile`. Não usar snake_case nem kebab-case em tabelas novas. Mudanças em tabelas já implantadas exigem migration que preserve os dados.


### Inicialização de banco dos clientes

Na LTMJ e no template limpo, migrations pendentes e bootstrap do root são obrigatórios no startup, antes de atender requisições. Não adicionar flags para habilitar essa etapa. O bootstrap é idempotente e preserva cadastro e senha do root existente. Arquivos de migrations são gerados no desenvolvimento; a API apenas aplica os que acompanham a versão publicada.
