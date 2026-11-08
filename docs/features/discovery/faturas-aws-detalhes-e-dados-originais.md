# Faturas AWS — resumo, detalhes e JSON original

## user-history

Como operador da Olivaw,
quero guardar os dados principais da fatura AWS e seu detalhamento original em JSON,
para consultar a origem das despesas e acompanhar custos e créditos sem perder informações quando o formato da AWS evoluir.

## acceptance-criteria

- Manter uma entidade de fatura herdada de `Entity`, com PK/FK internas e `ExternalId` público conforme o padrão do hub.
- Colunas próprias para fornecedor, identificação da conta, número do documento quando disponível, competência, moeda, situação documental, custo bruto, créditos, impostos e líquido; valores ausentes permanecem nulos.
- `RawData` é uma string contendo JSON válido com o documento/detalhamento recebido e sua versão de formato. Preservar campos desconhecidos sem exigir nova coluna para cada detalhe da AWS.
- Identificar claramente se o registro é fatura completa, estimativa, seção de um emissor ou captura parcial. Um resumo de serviços não comprova pagamento, competência ou fechamento.
- Detalhes de serviços, regiões, unidades, tarifas, quantidades, descontos e créditos ficam no JSON, com leitura tolerante a campos novos ou ausentes.
- Relacionar a fatura às despesas por vínculo explícito, sem duplicar valores entre fatura, subtotais de serviços e linhas de uso.
- Criar lançamentos de custo e crédito separados, com idempotência e reconciliação do líquido. Serviços com valor exibido zero são preservados nos detalhes, sem gerar lançamentos financeiros vazios.
- Manter USD original, taxa de conversão, origem e natureza da taxa: efetiva de pagamento ou gerencial estimada. Não apresentar cotação de estudo como valor efetivamente debitado.
- UI administrativa usa links para lista e detalhe da fatura, com breadcrumb e acesso a despesas relacionadas. Não usar modal para detalhes.
- Endpoints administrativos exigem root; faturas e custos internos não são expostos pelo Portal do cliente.
- Não importar novamente a mesma evidência, registrar competência arbitrária ou marcar uma fatura como paga a partir de custos estimados.

## ai-prompt

### Estado e escopo autorizado

Implementação local iniciada com a autorização de concluir a codificação do backlog em 30/10/2026. Entidade `AwsInvoice`, vínculos `AwsInvoiceExpense`, endpoints root `/aws-invoices`, formulário, lista e detalhes com JSON extensível. Nenhum lançamento financeiro é criado ao registrar uma fatura. Referências às despesas existentes são explícitas, limitadas a cem, à mesma competência e a uma evidência por despesa. A versão documental não substitui ou altera uma despesa silenciosamente.

Responsável: Codex, issue026, branch `feature/issue026-admin-client`, workspace atual. Sem commit, push ou deploy. Migration nova pendente ao estabilizar o pacote, preservando `Issue026` já publicada. Nenhum documento foi importado pela API nova em produção, pois ela ainda não foi publicada. Os lançamentos operacionais abaixo pertencem à API antiga de despesas.

Paginação por cursor de cinquenta documentos; JSON original somente no detalhe. Tamanho limitado, objeto JSON válido e totais reconciliados quando todos informados. Conta e competência obrigatórias; número e totais desconhecidos permanecem nulos. Pago exige documento completo identificado; não inferir pagamento do líquido zero. A evidência de pagamento deve estar na fonte/JSON e ser informada pelo operador. Digest protege repetição do mesmo conjunto de metadados, JSON e vínculos; nova evidência fica separada, sem reimportação de despesas.

Créditos permanecem na evidência documental; o filtro padrão de despesas considera somente custos, conforme decisão posterior do mantenedor. A proposta original de lançar créditos abaixo é histórica e não deve ser repetida em novos cadastros.

### Evidência recebida

- [JSON estruturado](aws-invoices/aws-bill-services-received-2026-10-26.json).
- [Transcrição do trecho recebido](aws-invoices/aws-bill-services-received-2026-10-26.txt). Separadores de layout e linhas vazias normalizados; descrições, quantidades e valores exibidos preservados. Não se trata de PDF fiscal ou exportação original da AWS.
- Fonte: seção **Amazon AWS Serviços Brasil Ltda. — charges by service**, recebida do mantenedor. O próprio trecho informa 9 serviços ativos, USD 0,00 após créditos e USD 0,00 de imposto. Não informa competência, número da fatura ou cotação.

| Componente do trecho | Custo USD | Crédito USD | Líquido USD |
| --- | ---: | ---: | ---: |
| EC2 — Linux t3.micro | 2,49 | — | — |
| EBS — gp3 | 0,80 | — | — |
| EC2 — subtotal incluindo EBS | 3,29 | 3,29 | 0,00 |
| VPC — IPv4 público | 1,20 | 1,20 | 0,00 |
| Outros 7 serviços | 0,00 | 0,00 | 0,00 |
| **Total do trecho** | **4,49** | **4,49** | **0,00** |

As duas primeiras linhas são componentes do subtotal EC2 e **não** devem ser novamente somadas ao subtotal. Quantidades: 239,093 h de EC2, 10 GB-Mo de gp3 e 239,098 h de IPv4. `GB-Mo` é armazenamento provisionado proporcional ao tempo; não representa espaço ocupado na VM. Os 239,796 h de capacidade EBS com tarifa zero são outra linha de uso, não mais horas de EC2 a cobrar.

A diferença entre USD 4,49 deste trecho e USD 4,05 do resumo anterior precisa de conciliação de período/escopo; não corrigir silenciosamente o documento recebido para fazer os números coincidirem. Não extrapolar a seção deste emissor para outros emissores da mesma conta, se houver.

### Estrutura sugerida

Nome de domínio proposto: `AwsInvoice`. Além de `Entity`: `Provider`, `Issuer`, `AccountReference`, `InvoiceNumber` opcional, `BillingYear/Month` opcionais enquanto incompleto, `PeriodStartsAt/EndsAt`, `Currency`, `DocumentStatus`, `Completeness`, `GrossAmount`, `CreditAmount`, `TaxAmount`, `NetAmount`, `IssuedAt/PaidAt` opcionais, `Source`, `SourceDigest`, `RawData` string. Campos públicos e regras permanecem em inglês; UI/documentação em português. Valores monetários usam decimal com precisão suficiente para evidências de custo; despesas mantêm os arredondamentos definidos no lançamento.

Definir antes da implementação: identidade de fatura sem número, substituição por documento final, versionamento de capturas parciais e vínculo entre fatura e créditos/faturas de outros emissores. Não basta uma chave conta+mês quando podem existir vários documentos. Digest identifica repetição da mesma evidência; nova versão não deve duplicar despesas já importadas.

O arquivo atual guarda `schemaVersion`, metadados, totais reconciliados, serviços/linhas estruturados e `rawData` string com o texto de origem. O mantenedor confirmou setembro/2026. O arquivo registra a referência de fechamento em 30/09/2026 e câmbio gerencial estimado de R$5,50/USD, explicitado na execução; não são data de pagamento nem cotação efetivamente cobrada. `invoiceTotalUsd` e `paidAmountUsd` são nulos: o líquido zero é confirmado apenas para a seção fornecida.

### Lançamentos operacionais previstos

Utilizar `POST /expenses`, com `EntryKey` determinística e consulta prévia paginada, sem conexão direta ao banco. Dois custos (EC2+EBS USD 3,29; IPv4 USD 1,20) e dois créditos Free Tier dos mesmos valores. Agrupar pelo subtotal do serviço evita diferenças de arredondamento desnecessárias entre linhas componentes. Não registrar adicionalmente subtotal geral ou imposto zero. Classificação: infraestrutura compartilhada sem rateio; clientes beneficiários não significam que o custo integral deva ser repetido para cada um.

Registro operacional concluído: quatro lançamentos gravados e consultados pela API; custos USD 4,49 e créditos USD 4,49, ambos R$ 24,70 na conversão gerencial, líquido zero. IDs, chaves de idempotência e conciliação estão no JSON. A competência setembro foi confirmada pelo mantenedor. Não foi cadastrado um novo custo aproximado de USD 5, pois duplicaria a mesma evidência.

Regra para próximos imports: confirmar competência/data de referência e taxa USD→BRL exigidas pela API atual. A cotação de R$ 5,50 do estudo comercial era apenas uma hipótese; só pode ser usada como cotação gerencial se identificada e confirmada para este cadastro. Não inferir esses dados somente das horas de uso. Registrar IDs e reconciliação no JSON após confirmar a gravação. Diante de erro/incerteza, consultar antes de repetir o POST; nunca trocar a chave para forçar duplicidade.

### Validação nesta etapa

JSON parseável, nove serviços, soma dos custos USD 4,49, créditos USD 4,49, líquido zero, preservação das linhas de custo zero e `rawData` também parseável. Não exige build ou testes de aplicação, pois nenhum código de produto foi alterado. Geração de migration da futura entidade exige pedido próprio.

## user-feedback

```text
faz assim, cria uma estrutura de fatura aws pra gente, nela vamos gravar dados principais tabelados e um campo raw como fizemos em outra estrutura que n lembro agora.

ai voce salva esses dados aqiu formato JSON< porque na usabilidade de despesas podemos mostrar esses detalhes depois blz.

DEIXE A FEATURE E ESSE JSON ANOTADOS EM docs como nova feature.

por hora apenas use os dados para cadastrar as despesas via API para mim por favor.
```

O detalhamento de serviços enviado junto com este pedido está preservado nos arquivos de evidência vinculados acima.
