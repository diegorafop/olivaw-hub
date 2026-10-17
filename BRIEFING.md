# Briefing inicial — Olivaw Hub

Registrado em 16 de outubro de 2026 a partir da conversa inicial com o fundador.

## Visão

Oferecer soluções de TI simples e acessíveis para pequenos negócios que precisam de presença digital e organização, mas não têm orçamento para grandes sistemas. Construir uma operação com muitos clientes e potencial de expansão dos serviços.

## Público inicial

- Padarias.
- Empresas de eventos.
- Empresas de construção.
- Outros pequenos negócios com necessidades semelhantes.

## Oferta inicial

- Websites.
- Pequenos sistemas de gestão de dados.
- Soluções para necessidades hoje atendidas por planilhas.
- Manutenção e alterações desenvolvidas com apoio de inteligência artificial.

## Base técnica pretendida

- Um template de API; linguagem e framework ainda não definidos.
- Um template de projeto Angular.
- Schematics para gerar novos projetos para novos clientes.
- Publicação dos sistemas na AWS.
- Automação de infraestrutura para reduzir o trabalho operacional.
- Reutilização dos templates e uso de IA para reduzir o esforço de desenvolvimento.

A arquitetura de isolamento entre clientes, os serviços AWS e o processo de implantação ainda serão definidos.

## Modelo de negócio

- Ticket médio baixo, compatível com o orçamento do público.
- Desenvolvimento de soluções simples com baixo esforço por projeto.
- Infraestrutura automatizada para reduzir o esforço de operação.
- Buscar rentabilidade por escala e quantidade de clientes.

Valores, planos, cobrança recorrente e limites de manutenção ainda não foram definidos.

## Expansão futura

Conforme a relação com os clientes evoluir, ampliar a oferta para integrações com redes sociais, Google Ads, consultoria financeira e outros serviços relevantes para esse público.

## Marca e posicionamento

- Nome escolhido pelo fundador: **Olivaw Hub**.
- Nome de pasta proposto: `olivaw-hub`.
- Referência: R. Daneel Olivaw, da literatura de Isaac Asimov.
- Preferência por uma marca sólida, tecnológica, com ambição de crescimento e sem tom excessivamente informal.
- O porte dos clientes não deve limitar a percepção de futuro da empresa.
- Disponibilidade de marca, domínios e perfis sociais ainda não foi validada individualmente para Olivaw Hub.

## Estado atual

Nome e direção inicial escolhidos. Implementação ainda não iniciada. Este documento preserva o contexto da conversa e deve ser atualizado conforme novas decisões forem tomadas.

## Evolução técnica — 17 de outubro de 2026

O repositório passa a organizar o core compartilhado, as bibliotecas Olivaw Identity e o template .NET `ms-limpo-api`. Novas APIs de clientes consomem esses módulos por `ProjectReference`, mantendo contexto, migrations, banco e implantação próprios. Perfil de acesso representa a role; autorizações representam permissões. O único usuário inicial é root, criado por bootstrap configurável após preparar o schema. Não há serviço central de identidade nesta etapa.

## Template frontend — 17 de outubro de 2026

O template `ms-limpo-ng` complementa `ms-limpo-api` para produtos com site público e área administrativa. Usa Angular standalone, NgRx SignalStore para `autenticacao`, Angular Material, Tailwind, testes unitários e Playwright. A landing page inicial é prerenderizada, a administração executa no navegador e o runtime é Nginx. Integração à identidade usa login, acesso e logout, sem refresh token nesta etapa. Conteúdo de negócio e geração por Schematics permanecem para projetos e etapas posteriores. Veja o [guia do frontend](ms-limpo-ng/README.md).

## Geração e site institucional — 17 de outubro de 2026

A collection local de Angular Schematics em `ms-limpo-ng` passa a gerar frontends com nome, URL de API e DNS próprios, preservando documentos e recusando conflitos. O primeiro consumidor é `ms-olivaw-hub-ng`, com site previsto em `olivaw-hub.duckdns.org` e API futura em `api.olivaw-hub.duckdns.org`. Contatos comerciais ainda serão confirmados. A geração não publica DNS, hospedagem ou API.
