# Site institucional do Olivaw Hub

## Decisões

- Projeto gerado pela collection Angular Schematics de `ms-limpo-ng`, preservando o HTML de inspiração. Este documento consolida as decisões atuais e substitui a proposta inicial de marca.
- Padrão técnico herdado: standalone, Signals/SignalStore, Material, Tailwind, tema centralizado, prerender público e administração CSR.
- O HTML de referência não é servido nem importado pela aplicação. Sua marca, sequência de apresentação e proposta de simplicidade inspiram o conteúdo.
- Frontend e API são serviços separados. Domínios previstos: `olivaw-hub.duckdns.org` e `api.olivaw-hub.duckdns.org`.
- A API será criada depois. A página pública e o resumo local não dependem dela; login e administração exigem a API.

## Conteúdo aplicado

A página apresenta benefícios concretos, possibilidades de desenvolvimento e etapas da contratação. Não promete catálogo pronto, integração automática, resultados medidos ou funcionalidades preditivas. A história da marca aparece de forma breve após a oferta. Não há clientes, depoimentos, preços ou contatos fictícios.

O menu mobile é funcional, formulários usam Material com labels e validação e as seções são responsivas. A página tem canonical, metadados sociais, robots e sitemap. A preferência por movimento reduzido é respeitada. Não usa CDN de Tailwind, fontes externas, bibliotecas de ícones externas ou imagens remotas.

## Contato pendente

Telefone, e-mail e redes sociais serão confirmados pelo responsável. Enquanto isso, o formulário cria um resumo copiável localmente. Não envia, armazena ou registra dados, e informa esse comportamento ao visitante. Antes de lançar a captação comercial, substituir esse fluxo pelo canal real e definir o tratamento do contato com base na implementação escolhida.

DNS, TLS, API e implantação não são criados pela geração. O HTML público é atualizado por novo build. Não há CMS nem publicação de conteúdo por painel nesta etapa.

## Avatar do Hub

O cabeçalho e o ícone do navegador passam a usar `public/assets/brand/olivaw-avatar-v1.png`, proposta de guardião robótico que mantém o “O” e o ponto luminoso. O arquivo para o grupo de WhatsApp está em `docs/identidade` na raiz do monorepo. Não altera a identidade própria da LTMJ.
