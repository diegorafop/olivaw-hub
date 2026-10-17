# Git Flow e rastreabilidade

## Padrão do Olivaw Hub

Usamos Git Flow clássico, conforme a configuração atual do repositório:

| Branch | Origem e integração |
| --- | --- |
| `master` | Código entregue; recebe releases e hotfixes |
| `develop` | Integração do desenvolvimento; recebe features |
| `feature/issue001-descricao` | Nasce de develop; `feature finish` integra em develop |
| `release/1.0.0` | Nasce de develop; `release finish` integra em master e develop e cria a tag |
| `hotfix/1.0.1` | Nasce de master; `hotfix finish` integra em master e develop e cria a tag |

O Git Flow gerencia merges e tags localmente. É necessário publicá-los para que o GitHub registre a entrega. Não exigimos PR para usar `finish`; PRs são opcionais para revisão. Evite squash neste fluxo para preservar o histórico com as referências das issues. Não confunda integração em master com implantação na AWS.

## Preparação por computador

Instale Git Flow AVH e Python 3. Em um clone já configurado com master/develop, execute uma vez:

```bash
python3 scripts/gitflow/setup.py
```

O script ativa `core.hooksPath=.githooks`, sem trocar de branch, criar branches ou substituir outro caminho de hooks existente. Confira primeiro `git flow init` se o clone ainda não estiver configurado. A configuração local do Git não acompanha o clone: a ativação é necessária em cada computador. Os scripts, hooks e regras acompanham o repositório.

## Trabalhar em uma feature

Com a issue criada, inclua o número no nome do branch:

```bash
git flow feature start issue001-devops-api
git add <arquivos-da-alteracao>
git commit -m "feat: configura execução da API"
git flow feature publish issue001-devops-api
```

O hook `commit-msg` acrescenta `Refs #1` automaticamente, se o commit ainda não mencionar essa issue. Não altera commits antigos nem transmite dados pela rede. O hook `pre-push` confere nomes e permite publicar master/develop durante o finish normal. Não use `--no-verify` para o fluxo habitual, pois ele ignora hooks.

No push, a Action registra uma referência ao branch na issue, sem comentários repetidos. Esse registro é um comentário com link, não uma vinculação formal no painel Development. A issue permanece aberta quando a feature entra em develop:

```bash
git flow feature finish issue001-devops-api
git push origin develop
```

Commits devem descrever mudanças concretas, com prefixos como `feat:`, `fix:`, `docs:`, `test:` e `chore:`. A referência à issue é adicionada pelo hook. Para alterações que atendem a várias issues, use linhas separadas `Refs #N`.

## Preparar e concluir uma release

Com develop atualizada e validada:

```bash
git flow release start 1.0.0
```

Faça os ajustes finais, commits e verificações necessários. Para publicar o trabalho em andamento, use `git flow release publish 1.0.0`. A Action identifica as issues nos commits da release e registra a referência ao branch, sem fechá-las ainda.

```bash
git flow release finish 1.0.0
git push origin master develop --tags
```

Confira os merges e a tag produzidos pelo Git Flow antes de publicar. O push acima também envia outras tags locais ainda não publicadas; quando necessário, publique apenas a tag desta entrega explicitamente.

Ao receber o push de master, a Action compara o estado anterior com o novo. Ela coleta as issues dos trailers `Refs #N` e dos nomes de features presentes nos commits de merge, registra o commit da entrega e fecha as issues abertas correspondentes. Releases podem concluir várias issues sem repetir números manualmente.

## Corrigir com hotfix

Para manter a tag com uma versão limpa e associar a issue uma única vez:

```bash
python3 scripts/gitflow/start_hotfix.py 2 1.0.1
```

Esse helper executa `git flow hotfix start 1.0.1` e registra localmente a issue #2 para os hooks. O restante é Git Flow normal:

```bash
git add <arquivos-da-correcao>
git commit -m "fix: corrige falha da API"
git flow hotfix publish 1.0.1
git flow hotfix finish 1.0.1
git push origin master develop --tags
```

Se preferir iniciar diretamente, os comandos equivalentes são:

```bash
git flow hotfix start 1.0.1
git config --local branch.hotfix/1.0.1.olivaw-issue 2
```

Essa associação local precisa ser informada também em outro clone que continue a hotfix; os commits já publicados mantêm suas referências. Uma branch versionada sozinha não permite inferir o número da issue. Se houver release ativa, confira os destinos e opções informados pelo Git Flow instalado antes de concluir a hotfix.

## Automação no GitHub

`.github/workflows/rastreabilidade.yml` executa em pushes de features, bugfixes, hotfixes, releases e master. Usa o `GITHUB_TOKEN` temporário com leitura do código e escrita de issues; não exige token pessoal, criação de PRs, merges automáticos ou acesso à AWS. Actions precisa estar habilitado no repositório, e políticas de organização não podem bloquear essas permissões.

A automação só passa a existir remotamente depois que estes arquivos forem commitados e enviados. Ela não fecha issues no primeiro push de uma master nova nem após um push forçado em master. Nesses casos, revise a entrega manualmente. Erros de permissão ou issue inexistente aparecem na execução da Action.

O fechamento corresponde à integração técnica descrita na issue. Critérios de aceitação que exigirem implantação ou validação manual precisam ser conferidos antes do finish; não use o número de uma issue ainda incompleta em commits entregues. Referências em commits sem hooks precisam ser preenchidas manualmente.

## Instruções para agentes

Ao receber uma tarefa vinculada a issue, derive o número do branch atual ou registre a associação da hotfix. Preserve o fluxo Git Flow e mantenha as referências nos commits. Não crie PR obrigatório, bloqueio de push em master/develop ou fechamento antecipado ao integrar em develop. Não faça finish, push, fechamento remoto ou implantação sem autorização para essas ações na tarefa.

As regras estão em `scripts/gitflow/config.json`. Para validar a automação sem rede:

```bash
python3 -m unittest discover -s scripts/gitflow/tests -v
```

Referências: [Git Flow AVH](https://github.com/petervanderdoes/gitflow-avh), [hooks do Git](https://git-scm.com/docs/githooks), [token de Actions](https://docs.github.com/en/actions/tutorials/authenticate-with-github_token) e [referências nativas de fechamento do GitHub](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue). O fechamento deste projeto usa a Action em master e não depende de palavras de fechamento em PRs.
