# Agentes da Olivaw

`olivaw-agent/ms-<nome>-py` reúne agentes independentes criados pelo scaffold de [ms-limpo-py](../ms-limpo-py/README.md). O primeiro é [Baley](ms-baley-py/README.md), responsável pela segurança, com a skill versionada [safety-docker](skills/safety-docker/SKILL.md).

## Operar todos os agentes

Na raiz do monorepo:

```bash
./olivaw-agent/olivaw-agent start
./olivaw-agent/olivaw-agent status
./olivaw-agent/olivaw-agent stop
./olivaw-agent/olivaw-agent update
```

Dentro da pasta `olivaw-agent`, use `./olivaw-agent start`. Para usar literalmente `olivaw-agent start`, coloque a pasta do agrupador no PATH da sua sessão. Não há instalação global de comandos ou alteração automática de arquivos do shell.

O agrupador descobre os projetos `ms-*-py` presentes no checkout para versionamento. Confere `pyproject.toml`, identidade de `setting.json` e pacote `src/ms_<nome>/__main__.py`; rejeita links e identidades incoerentes. Não depende de os arquivos novos já estarem commitados e não executa qualquer pasta apenas por seu nome.

Operações são sequenciais e retornam JSON indentado com dois espaços por agente, com código de falha agregado. Uma falha não é omitida e não impede verificar os outros. Por padrão usa `.venv/bin/python` de cada projeto (ou `.venv/Scripts/python.exe` no Windows); sem venv usa o Python atual, suficiente para o Baley inicial sem dependências externas. O lifecycle inicial do Baley foi validado em Linux/POSIX; Windows permanece pendente.

Para escolher só um agente:

```bash
./olivaw-agent/olivaw-agent status --agent baley
```

O Baley atual inicia um **supervisor aguardando configuração**, sem SSH, IA ou envio de relatório. O contrato de análise e a API permanecem no planejamento. Não interpretar `running=true` como auditoria em andamento; `state=waiting-configuration` e `analysisEnabled=false` explicitam esse limite.

## Adicionar um projeto

```bash
python3 ms-limpo-py/scripts/new_agent.py outro --output olivaw-agent
```

O template copia o `setting.json` e a identidade ajustada; adicionar as operações de lifecycle do novo agente antes de incluí-lo na rotina. O scaffold ainda contém sua demonstração CQS, não um runtime completo. Projetos API-only removem dependências/comandos de banco, como foi feito no Baley. O agrupador não altera automaticamente agentes existentes.

## Skill no Codex

A fonte é `skills/safety-docker`. A cópia de execução solicitada fica em `~/.agents/skills/safety-docker`. O [padrão oficial](https://learn.chatgpt.com/docs/build-skills) usa `SKILL.md` com `name`/`description`, referências opcionais e descoberta por escopo; não exige framework próprio.

```bash
python3 olivaw-agent/scripts/install_skill.py
```

Invocar no Codex como **`$safety-docker`**. Se não aparecer na sessão já aberta, reinicie o Codex. A instalação é cópia, não symlink; alterar a fonte não altera automaticamente o que já está instalado. Depois de revisar mudanças e confirmar a substituição:

```bash
python3 olivaw-agent/scripts/install_skill.py --replace
```

O instalador recusa substituir uma versão diferente sem `--replace`, valida a cópia por hashes e preserva as demais skills. Não instala skill na VM nem copia chaves SSH.

## Update — pesquisa somente leitura

```bash
./olivaw-agent/olivaw-agent update
./olivaw-agent/olivaw-agent update --format text
```

O padrão é JSON indentado. O formato text exibe `#source`, `#discovery` e `#learn`, além de status/limitações. `update` **não grava arquivos, não cria propostas, não edita o agente, não altera a skill e não sincroniza a cópia do Codex**. Estado de supervisor já existente permanece intacto. O operador decide se quer usar cada informação.

- `source`: fontes atuais, URLs, situação da consulta e indicação de truncamento/erro. O catálogo editável é [sources.json](skills/safety-docker/references/sources.json); para retirar uma fonte, removê-la ou definir enabled=false. As referências humanas estão em [sources.md](skills/safety-docker/references/sources.md).
- `discovery`: links de páginas/fontes/fóruns encontrados em busca web. Todos exigem revisão, inclusive um domínio de fornecedor conhecido. Nenhum link encontrado é acessado automaticamente ou promovido a fonte aprovada. Títulos e snippets não provam confiabilidade.
- `learn`: pontos numerados para revisar/incorporar à skill, com motivo, evidência por URL, seção sugerida e decisão pendente. Não são achados confirmados na VM. NgRx citado como tema de interesse não vira vulnerabilidade inventada.
- `research`: consultas usadas, método, período e limitações. A análise inicial usa regras de evidência, sem IA paga; o modelo automático continua não configurado.
- `review`: hash da skill lida, decisão humana necessária e confirmação de nenhuma alteração.

O código lê os feeds CISA/NVD e as páginas aprovadas do catálogo. Pesquisa fontes novas via RSS público do Bing, sem chave de API e sem scraping do Google. A interface RSS pode mudar ou ser bloqueada: nesse caso, a busca falha explicitamente, sem resultados fictícios. Provedor e consultas ficam em `setting.json`; atualmente somente `bing-rss` está implementado.

São até três consultas, dez links candidatos no padrão, páginas até 1 MiB, feeds até 8 MiB e timeout de até 15 segundos por chamada. DNS é validado e fixado em IP público para TLS; destinos privados, credenciais em URL, redirecionamentos e DTD/XML de busca são rejeitados. Essas verificações não transformam uma fonte descoberta em confiável. CISA KEV não é catálogo completo, e a NVD está limitada a uma página. Resultado parcial retorna código não zero, com dados aproveitáveis e limitações explícitas.

Exemplo **de estrutura**, sem representar uma nova consulta:

```json
{
  "schemaVersion": "2.0",
  "operation": "update",
  "agentId": "baley",
  "status": "completed",
  "source": [],
  "discovery": [],
  "learn": [],
  "research": {},
  "review": {
    "operatorDecisionRequired": true,
    "changesApplied": false,
    "localFilesWritten": false,
    "skillChanged": false,
    "agentChanged": false
  }
}
```

Adicionar/remover fontes, alterar instruções da skill e executar o instalador após revisão são ações manuais separadas. Arquivos de propostas de versões anteriores, se existirem em `.state/updates`, não são usados nem apagados pelo update novo.

## Validação

```bash
PYTHONPATH=ms-limpo-py/src python3 -m unittest discover -s ms-limpo-py/tests -v
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m unittest discover -s olivaw-agent/ms-baley-py/tests -v
python3 -m unittest discover -s olivaw-agent/tests -v
```

As verificações de processos usam projetos/estados temporários e encerram os workers de teste. Fontes de vulnerabilidade são simuladas nos testes, sem produção ou chamadas pagas. A [feature principal](../docs/features/in-progress/agentes-python-baley-e-relatorios.md) mantém pendências de coleta, modelo, API, RawData e migrations.
