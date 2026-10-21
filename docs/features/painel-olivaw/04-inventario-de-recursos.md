# 04 — Projetos, ambientes e inventário de recursos

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Permitir entender quais sistemas e dependências pertencem a cada cliente, antes de tentar medir sua saúde e consumo.

## Dependências

[Cadastro de clientes](03-cadastro-de-clientes.md).

## Escopo da entrega

- Modelar Cliente → Projeto → Ambiente → Sistema → Recursos e dependências.
- Inventariar frontend, API, banco, armazenamento, domínio, certificado, backup e tarefa agendada quando conhecidos.
- Registrar finalidade, proprietário, identificador no provedor, ambiente, responsável, referências e cobertura de monitoramento.
- Representar recursos compartilhados e seus vínculos sem duplicá-los como recursos exclusivos.
- Aba Sistemas e recursos da ficha e indicação de sistemas ainda sem coleta.

## Fora desta entrega

- Provisionamento, descoberta remota automática, execução de comandos e armazenamento de credenciais abertas.
- Métricas reais apenas por cadastrar um recurso.

## Critérios de aceite

- Operador identifica os recursos conhecidos de um sistema e o impacto potencial de uma dependência compartilhada.
- Ambiente e propriedade do recurso são explícitos; alterações têm responsável e histórico.
- Um recurso sem coleta aparece como inventariado, sem saúde ou consumo confirmados.
- URLs, filtros e vínculos são validados no servidor; uma referência de credencial não expõe seu valor.

## Decisões antes de implementar

- Tipos mínimos de recursos, cardinalidades, recursos iniciais confirmados e responsáveis.
- Política de destinos autorizados para verificações futuras e de referências a credenciais operacionais.
