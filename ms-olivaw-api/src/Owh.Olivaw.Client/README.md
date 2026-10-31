# Owh.Olivaw.Client

Projeto base para contratos públicos e cliente REST do microservice. Adicione as operações reais quando a API e seus contratos estiverem definidos.

## Clientes administrativos

`Features/CustomerFeature` contém payloads planos para criação, atualização, consulta por ID, listagem paginada e resumo por situação. Os endpoints `/customers` são exclusivos do root da identidade administrativa Olivaw. Cliente é empresa e não cria `UserProfile` ou credenciais. Atualização inclui revisão esperada para concorrência; criação pode informar a origem de um lead. Consulte os [contratos e comandos da API](../../README.md#cadastro-administrativo-de-clientes). Esta biblioteca fornece os contratos; não implementa obtenção de JWT nem cria convites de usuários.

## Acompanhamento de leads

`Features/LeadFeature` contém os contratos de envio público, consulta administrativa e acompanhamento. `UpdateLeadFollowUpCommand` possui revisão esperada, etapa e nota; autor e data não fazem parte do payload. `ReadLeadHistoryQuery` consulta blocos de 20 alterações pelo cursor `BeforeRevision`. Lista e resumo acrescentam etapa e contagens reais. Root ou `leads.manage` são exigidos pela API para leitura e manutenção internas. Consulte o [contrato completo](../../README.md#acompanhamento-comercial-de-leads).

## Serviços administrativos

`Features/ManagedServiceFeature` fornece payloads planos de criação/atualização, listagem e ficha de serviços, com vínculos públicos por GUID. Atualização recebe revisão esperada e não permite substituir o identificador técnico. Períodos e nomes dos autores de abertura/encerramento são dados de saída, definidos pelo servidor. Consulte os [contratos da API](../../README.md#serviços-e-vínculos-com-clientes). O projeto continua sem transporte HTTP próprio ou obtenção de JWT.

## Relatórios de segurança

`Features/SecurityReportFeature` contém payloads planos para envio e consulta dos relatórios Baley. `CreateSecurityReportCommand.RawData` preserva o JSON evolutivo; os campos internos de auditoria não pertencem ao payload. Envio exige `security.reports.write`; listagem e detalhes exigem root. O projeto fornece contratos, sem transporte HTTP ou aquisição de JWT. Consulte o [guia da API](../../README.md#relatórios-de-segurança-do-baley).

## Correções de segurança

`Features/SecurityRemediationFeature` contém contratos planos de aprovação individual/em lote, consulta por relatório e fila técnica, e atualização de execução. A aprovação é root; execução depende de permissão própria, autor do relatório, reserva, estado e saúde. Não há comandos livres nos payloads. Consulte o [contrato da API](../../README.md#correções-aprovadas-do-baley).


## Contratos de Planos da issue026

Payloads em `Features/PlanFeature`: `CreatePlanVersionCommand`, `CreatePlanAssignmentCommand`, `CreatePlanCostSheetCommand` e `ReadPlanVersionByFilterQuery`, com responses próprias. Endpoints root: POST/GET `/plans/versions`, POST `/plans/assignments` e POST `/plans/cost-sheets`. Não incluem obtenção de JWT nem publicam catálogo automaticamente. `ReadPlanStudyByFilterQuery.ExternalId` permite recuperar um estudo pelo ID público.

`CommercialPercent` é opcional por compatibilidade (padrão zero) nos estudos/estimativas antigos. `CostSheetData` é JSON string opcional na consulta de estudos novos, preservando entradas originais sem alterar estudos legados. Capacidades de versão são GiB para armazenamento, MiB para memória e vCPU para CPU; estudos antigos mantêm StorageLimitGB decimal, sem reinterpretar a unidade.

Contratos de faturas ficam em `AwsInvoiceFeature`, controle gerencial em `FinancialFeature` e estado da coleta em `HealthReadingFeature`. São contratos planos independentes de domínio e MediatR; o projeto não implementa um SDK HTTP para estas rotas.
