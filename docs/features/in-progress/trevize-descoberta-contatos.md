# Trevize — descoberta de contatos públicos

## Objetivo

Encontrar e-mails públicos nos sites dos candidatos e registrar páginas com formulário de contato para uso futuro, sem iniciar comunicação ou alterar o acompanhamento comercial.

## Implementação

- Leitura passiva da página inicial e até três páginas de contato do mesmo site, com limites de bytes, timeout, proteção de rede e respeito a `robots.txt`.
- Até cinco e-mails distintos, provenientes de `mailto:` ou texto visível, acompanhados da página de origem.
- Página de contato, endereço do formulário, data UTC e resultado: encontrado, não detectado, inconclusivo ou não verificado.
- Identificação de formulário por campos de contato e mensagem; busca, newsletter simples e login não contam. Formulários dinâmicos podem não ser detectados.
- Persistência pelo contrato de qualificação no snapshot JSON, sem migration. E-mail principal preenchido na criação de novos leads; preservação de qualificações existentes.
- Ficha administrativa exibe os canais e preserva os campos ao salvar outras alterações.

## Validação

Testes isolados cobrem extração, normalização, origem, limites, links externos, robots, falhas parciais, falsos formulários, contrato e persistência. Não usam sites, contas ou bancos reais. Procedimentos e limites no [README do Trevize](../../../olivaw-agent/ms-trevize-py/README.md#descoberta-de-canais-de-contato).

## Publicação e limites

Implementação local; publicar primeiro a API compatível e depois atualizar o agente. Nenhuma publicação, migration, coleta real ou contato comercial foi executado. A releitura verifica a persistência das evidências. Não inclui reprocessar os leads existentes, descobrir e-mails no Maps, enviar formulários ou campanhas Gmail. A integração Gmail permanece um piloto separado, pendente de validação OAuth real.
