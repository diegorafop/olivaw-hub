# Diretrizes — Automação GitHub

## Rastreabilidade

O workflow de rastreabilidade acompanha o Git Flow clássico documentado em `../docs/gitflow.md`. Executa scripts em `../scripts/gitflow`, registra branches nas issues e conclui issues referenciadas quando o código chega a master. Não transforme feature finish em PR obrigatório nem feche issues em develop.

## Permissões e execução

Use `GITHUB_TOKEN` com as permissões mínimas necessárias. Não execute código de forks não confiáveis com permissões de escrita por `pull_request_target`. Mantenha ações externas fixadas por commit e não inclua segredos em logs ou no código do workflow.

Mudanças no workflow não significam que ele já foi publicado ou executado no GitHub. Declare claramente o que foi validado localmente. Não habilite merge, deploy ou alterações de configuração do repositório como efeito colateral desta automação.

## Validação

Confira o YAML, os eventos, permissões e contratos dos scripts. Rode testes locais com API simulada e repositórios descartáveis, sem modificar issues reais. Evite comentários repetidos e alterações de estado indevidas em retries, exclusões de branches, pushes iniciais ou pushes forçados.
