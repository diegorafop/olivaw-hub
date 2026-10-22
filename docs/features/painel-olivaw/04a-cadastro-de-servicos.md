# 04a — Cadastro de serviços e vínculo com clientes

## Estado e prioridade

Planejada; sem implementação. Primeiro recorte do [inventário](04-inventario-de-recursos.md), vinculado à [épica administrativa](../painel-administrativo-e-portal-do-cliente.md).

Priorizar esta entrega depois do cadastro de clientes. O [acompanhamento comercial de leads](../done/02-acompanhamento-comercial-de-leads.md) foi antecipado e concluído localmente a pedido do mantenedor; não depende tecnicamente deste cadastro.

## Objetivo e significado de serviço

Registrar quais serviços a Olivaw mantém e quais clientes são atendidos por eles. Neste planejamento, serviço é uma unidade técnica identificável: frontend, API, worker ou outra aplicação. Exemplo de cadastro a confirmar com a operação: cliente LTMJ vinculado a `ms-ltmj-ng` e `ms-ltmj-api`.

Um sistema/projeto pode agrupar vários serviços. Domínio, banco de dados, armazenamento e servidor são recursos vinculáveis a esses serviços na evolução do inventário. Oferta comercial, plano contratado e cobrança pertencem às features específicas; não usar um único cadastro para representar todos esses conceitos.

O [detalhamento da feature 04](04-inventario-de-recursos.md) incorpora o exemplo informado pelo mantenedor: aplicações LTMJ utilizando PostgreSQL, RabbitMQ, Redis, domínio Olivaw, IP público e disco, além de outros recursos que precisem ser confirmados. Este primeiro cadastro fornece a identidade das aplicações; o inventário registra as instâncias/dependências, os ambientes e seus consumidores. Um recurso compartilhado é cadastrado uma vez, e não uma cópia por cliente ou serviço.

## Dependência

[Cadastro de clientes concluído](../done/03-cadastro-de-clientes.md). Não depende de coleta, agentes de IA, portal, cobrança ou descoberta automática na nuvem.

## Escopo inicial

- Nome de apresentação, identificador técnico estável e único, tipo, descrição breve, responsável interno e situação cadastral.
- Situações propostas: planejado, em implantação, ativo e encerrado; confirmar transições antes de implementar. Situação cadastral não comprova disponibilidade.
- Vincular um ou mais clientes ao serviço, com vínculo explícito e rastreável. Um cliente pode ter vários serviços; um serviço compartilhado pode atender vários clientes sem duplicar o cadastro técnico.
- Identificar serviços internos da Olivaw sem cadastrar uma empresa fictícia para permitir sua existência. Confirmar a representação desse escopo antes de codar.
- Registrar criação, alterações e encerramento de vínculos, com operador e momento. Remover um vínculo não deve apagar o histórico do atendimento àquele cliente.
- Busca por nome/identificador, filtro por cliente, tipo e situação e listagem paginada.
- Ficha do serviço mostrando contexto e clientes vinculados. A ficha do cliente mostra os serviços que o atendem.

Não presumir que dois serviços com nomes parecidos são a mesma aplicação. Tampouco duplicar um serviço por ambiente: desenvolvimento, homologação e produção terão registros próprios vinculados ao mesmo serviço na próxima entrega.

## Interface proposta

- Menu **Serviços** na operação, com lista e ação **Cadastrar serviço**.
- Na ficha do cliente, seção **Serviços**, com ações **Vincular serviço existente** e **Cadastrar serviço para este cliente**. A segunda prepara o vínculo, que só é persistido após confirmação.
- Lista com nome, identificador técnico, tipo, clientes e situação. Servidor, custos, consumo e indicadores de saúde entram quando houver dados reais das respectivas features.
- Ficha do serviço com dados principais e vínculos; ambientes e recursos serão acrescentados pelo inventário, sem apresentar abas vazias como funcionalidades disponíveis.

## Fora desta entrega

- Criar repositório, conta técnica, usuário, `UserProfile`, domínio, banco, container ou servidor automaticamente.
- Executar deploy, verificar URLs, suspender serviços ou alterar cobranças ao cadastrar/encerrar um vínculo.
- Descoberta de serviços na AWS, leitura de credenciais e importação automática de Compose.
- Cadastro completo de ambientes, infraestrutura, métricas, alertas ou custo.

## Critérios de aceite

- Operador autorizado identifica os serviços de um cliente e os clientes atendidos por um serviço compartilhado.
- Identificador técnico duplicado e vínculo já existente têm tratamento compreensível; vínculo usa o cliente cadastrado, não um nome copiado em texto livre.
- Consultas e alterações são autorizadas no servidor. Definir o acesso interno inicial seguindo a política da operação; o portal futuro só pode consultar o próprio vínculo, sem revelar outros clientes do serviço compartilhado.
- Paginação e filtros são executados no banco, com estados de carregamento, vazio e falha distintos na interface.
- Encerrar um serviço/vínculo preserva histórico e não modifica recursos remotos ou condições comerciais.
- Cadastro de serviço não significa serviço monitorado ou provisionado. Dados da LTMJ só entram após confirmação, sem seeds ou chamadas a produção nos testes.

## Próxima entrega e decisões

O [inventário](04-inventario-de-recursos.md) reutilizará estes serviços para relacionar sistemas/projetos, ambientes, recursos e dependências compartilhadas. Essa separação permite começar com cadastro e vínculos úteis antes de modelar toda a infraestrutura.

Antes da implementação, confirmar tipos e situações iniciais, regras de encerramento/reativação, formato do identificador técnico, responsáveis e representação de serviços internos/compartilhados. Seguir os padrões do domínio e de chaves internas do monorepo; não criar schema ou gerar migrations como consequência deste documento.
