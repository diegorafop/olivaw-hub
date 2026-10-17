# Diretrizes — Ferramentas do monorepo

## Escopo

Os scripts da raiz automatizam o fluxo comum do Olivaw Hub. O gerador de clientes continua em `ms-limpo-api/scripts`. Não copie scripts que dependem da estrutura do monorepo para dentro dos clientes gerados.

## Git Flow e rastreabilidade

- Preserve o Git Flow clássico: features em develop; finish de release/hotfix, merges e tags gerenciados pelo Git Flow.
- Hooks locais devem operar sem rede e sem alterar o índice ou commits antigos. Setup não substitui hooks personalizados silenciosamente.
- Número de issue em feature vem do branch. Hotfix versionada precisa da associação explícita registrada pelo helper.
- A automação remota não faz merges, pushes, deploys ou PRs obrigatórios. Fechamento de issue ocorre somente com os commits integrados em master.
- Use APIs oficiais, permissões mínimas e chamadas idempotentes. Não imprima tokens nem copie credenciais para arquivos versionados.

## Validação

Teste os scripts com `python3 -m unittest discover -s scripts/gitflow/tests -v`. Use repositórios temporários para testar hooks, configuração local e ciclos de finish; nunca use o branch de trabalho como ambiente de teste. Simule chamadas GitHub e não feche issues reais para testar o código. Documente separadamente ativação local, publicação e limites do ambiente.
