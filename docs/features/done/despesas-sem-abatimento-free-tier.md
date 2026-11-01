# Despesas — gastos sem abatimento de benefícios Free Tier

## user-history

Como operador da Olivaw,
quero considerar os gastos da infraestrutura independentemente dos créditos Free Tier,
para conhecer o custo de manter os serviços sem que benefícios temporários escondam esse valor.

## acceptance-criteria

- Consultas de despesas retornam custos por padrão; créditos históricos não reduzem o valor considerado nem aparecem como despesas na lista.
- Preservar créditos existentes para auditoria, sem exclusão física, alteração dos custos ou duplicidade de lançamento.
- Consulta administrativa explícita `includeCredits=true` permite conciliar o histórico completo quando necessário.
- Financeiro e lista de Despesas deixam de apresentar o líquido após créditos como total de gastos.
- Tela vazia informa o período e oferece navegar para o anterior, sem deslocar automaticamente lançamentos para o mês atual.
- Setembro/2026 considera USD 4,49 de gastos, R$ 24,70 na cotação gerencial registrada, mesmo que o trecho da fatura tenha líquido zero após os créditos.

## ai-prompt

Correção local por Codex, branch feature/issue026-admin-client, worktree atual. Sem commit, push, deploy ou alteração direta em banco. A API publicada foi consultada com autenticação administrativa: setembro tinha quatro registros, dois custos e dois créditos, bruto R$24,70; outubro não tinha registros. O dashboard inicia no mês atual, explicando a ausência no recorte de outubro. Não cadastrar nova despesa aproximada de USD5: duplicaria a evidência de setembro.

`ReadExpenseByFilterQuery.IncludeCredits` é false por padrão. O handler filtra por `Kind=cost` antes de somas, séries, grupos e paginação. `includeCredits=true` mantém a reconciliação administrativa histórica. O banco e a evidência JSON preservam os quatro registros originais. A alteração é de consulta/apresentação, sem mudança de modelo e sem migration.

As telas consultam custos explicitamente, retiram os benefícios do resumo principal e deixam de oferecer crédito no formulário de despesa. O histórico de benefício pertence à documentação da fatura, que continua em discovery; sua nova estrutura não foi implementada. O financeiro mostra gastos do período, quantidade de despesas e categorias. Estado vazio tem atalho para período anterior.

Publicação ainda pendente pelo mantenedor. Atualizar a API antes do frontend para disponibilizar a nova regra. O filtro de setembro e os custos já existem na versão publicada; a exclusão dos créditos por padrão é a correção local desta entrega.

Validação local concluída: 416 testes da API, 7 testes E2E de Financeiro/Despesas, build e lint do frontend passaram. Sem nova migration; avisos de orçamento SCSS preexistentes preservados.

## user-feedback

```text
os creditos free tier n entra em despesas, mas os gastos entram pra conta.

lançou no sistema? ainda n apareceu na tela do financeiro
tem que deploy né
subi os 2 front e back mas n aparece despesa, consulta ai man porque n ta aparecendo nada
```
