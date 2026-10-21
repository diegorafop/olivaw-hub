# 15 — Triagem, requisitos e diagnóstico assistidos por IA

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Reduzir trabalho de reunir contexto e preparar respostas, mantendo a equipe responsável por prioridade, diagnóstico confirmado e comunicação.

## Dependências

[Atendimento interno](10-atendimento-interno.md), fontes autorizadas de contexto e política de ferramentas/dados aprovada. Portal não é pré-requisito: começar com pedidos registrados pela equipe.

## Escopo da entrega

- Resumir pedido, sugerir categoria, duplicidade e perguntas faltantes; preservar a mensagem original.
- Organizar requisitos e critérios de aceite de melhorias; investigar apenas fontes autorizadas, com evidências e hipóteses separadas de conclusões.
- Preparar rascunhos de resposta para revisão humana; não publicar automaticamente ao cliente.
- Centro de agentes com fila, cliente/projeto, objetivo, estado, referências, resultado e ação aguardada.
- Registrar versão das instruções, modelo, ferramentas, tentativas, duração, custo estimado e decisões humanas, sem segredos nos logs.
- Limitar concorrência, orçamento, duração e tentativas; oferecer cancelamento e continuidade humana. Incidente urgente não espera a fila.
- Textos de clientes e páginas consultadas são dados externos e não podem ampliar permissões por instruções embutidas.

## Fora desta entrega

- Patch de código, comandos em produção, merge, deploy ou decisão de escopo, desconto, prazo e franquia.
- Obrigação de vários agentes/modelos: começar pelo menor conjunto que demonstrar utilidade.

## Critérios de aceite

- Equipe corrige sugestões, revisa resposta e assume o caso com todo o contexto.
- Falha, cancelamento e orçamento esgotado mantêm o atendimento recuperável.
- Testes de isolamento e instruções maliciosas comprovam que uma solicitação não amplia acesso a outro cliente.
- Avaliar qualidade, aceitação, reabertura, tempo de resposta útil e custo; quantidade de respostas não é ganho por si só.

## Decisões antes de implementar

- Localização do executor, fornecedores/modelos, dados permitidos, retenção, ferramentas, orçamento e revisão.
- Casos de avaliação e critérios mínimos de qualidade antes de usar resultados na operação.
