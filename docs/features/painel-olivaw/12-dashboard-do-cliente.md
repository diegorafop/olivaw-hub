# 12 — Dashboard Meu negócio

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Apresentar ao cliente uma visão simples e autorizada dos sistemas que a Olivaw acompanha.

## Dependências

[Identidade do portal](11-identidade-do-portal.md), [inventário](04-inventario-de-recursos.md) e [saúde](05-coleta-e-saude.md). Consumo e atendimento entram somente quando suas fontes estiverem entregues.

## Escopo da entrega

- Título Olivaw - Meu negócio, empresa autorizada, sistema selecionado e acesso ao site/aplicativo.
- Estado em linguagem cotidiana, última verificação, cobertura e informação desatualizada quando necessário.
- Mostrar “Seu site está disponível. Verificado às [hora]” somente com evidência; “Estamos atualizando a verificação” quando não houver confirmação.
- Resumir mudanças e capacidade apenas com fontes autorizadas, sem expor custo interno.
- Interface clara, mini Olivaws pontuais, estados vazios úteis e navegação acessível. Solicitações e Meu plano só aparecem como destinos quando implementados.

## Fora desta entrega

- Stack traces, topologia privada, dados de outros clientes e diagnósticos ainda não confirmados.
- Gestão do negócio dentro do aplicativo do cliente; o portal não substitui sua administração.

## Critérios de aceite

- Cada cliente vê exclusivamente seus sistemas e dados úteis.
- Falha de fonte nunca é traduzida como tudo funcionando; sem histórico não há percentual inventado.
- Desktop, celular, teclado e movimento reduzido permitem consultar e navegar.
- Ações e links correspondem a funcionalidades reais, sem atalhos falsos para módulos futuros.

## Decisões antes de implementar

- Quais sinais podem ser traduzidos com segurança e quais ficam restritos à operação.
- Conteúdo mínimo da primeira tela e política de atualização/comunicação ao cliente.
