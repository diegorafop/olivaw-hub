# Owh.Olivaw.Client

Projeto base para contratos públicos e cliente REST do microservice. Adicione as operações reais quando a API e seus contratos estiverem definidos.

## Clientes administrativos

`Features/CustomerFeature` contém payloads planos para criação, atualização, consulta por ID, listagem paginada e resumo por situação. Os endpoints `/customers` são exclusivos do root da identidade administrativa Olivaw. Cliente é empresa e não cria `UserProfile` ou credenciais. Atualização inclui revisão esperada para concorrência; criação pode informar a origem de um lead. Consulte os [contratos e comandos da API](../../README.md#cadastro-administrativo-de-clientes). Esta biblioteca fornece os contratos; não implementa obtenção de JWT nem cria convites de usuários.

## Acompanhamento de leads

`Features/LeadFeature` contém os contratos de envio público, consulta administrativa e acompanhamento. `UpdateLeadFollowUpCommand` possui revisão esperada, etapa e nota; autor e data não fazem parte do payload. `ReadLeadHistoryQuery` consulta blocos de 20 alterações pelo cursor `BeforeRevision`. Lista e resumo acrescentam etapa e contagens reais. Root ou `leads.manage` são exigidos pela API para leitura e manutenção internas. Consulte o [contrato completo](../../README.md#acompanhamento-comercial-de-leads).
