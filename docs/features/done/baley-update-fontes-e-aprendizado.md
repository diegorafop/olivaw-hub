# Baley — Consulta de fontes, descoberta e pontos de aprendizado

## user-history

Como operador do Baley,
quero consultar fontes de segurança e descobrir novas referências com pontos de aprendizado,
para decidir manualmente o que incorporar à skill, sem o update alterar arquivos ou aplicar mudanças.

## acceptance-criteria

- Update consulta fontes atuais e retorna source/discovery/learn em JSON indentado.
- Formato textual opcional apresenta #source, #discovery e #learn, com pontos numerados.
- Novas fontes são pesquisadas na web e marcadas como candidatas não verificadas; nenhuma é aprovada ou acessada automaticamente.
- Sugestões têm fundamento e URLs de evidência; um exemplo como NgRx não é transformado em vulnerabilidade inventada.
- Update não cria propostas, não grava estado, não altera Baley, skill versionada ou cópia instalada no Codex.
- Operador consegue revisar catálogo e consultas manualmente; falhas, limites e paginação incompleta ficam explícitos.

## ai-prompt

### Entrega e contrato

Implementada em 24/10/2026 por Codex na branch recebida `feature/issue023-tests-and-adjusts`, worktree atual, sem nova issue ou operação remota de servidor. Substitui somente o comportamento anterior de update com propostas em disco; lifecycle e API futura continuam separados.

A saída tem `schemaVersion=2.0`, `operation`, `agentId`, `status` e:

- `source`: fontes no catálogo, URL, publisher, status da consulta, erro HTTP quando disponível e truncamento. Approved significa registrada pelo operador, não prova universal de confiabilidade.
- `discovery`: URLs/títulos encontrados pela busca, origem da pesquisa, tipo estimado de publisher, `trustStatus=not-verified` e revisão obrigatória. Não segue os links encontrados nem incorpora novas fontes.
- `learn`: ações numeradas, fundamento, evidências por URL, seção sugerida e decisão pendente. Regras de análise de evidências, sem uso de modelo de IA nesta etapa. `skillCoverage` indica apenas menção textual do tema, não qualidade ou completude da regra existente.
- `research`: buscas usadas, método, janela, horário e limitações. Sem conclusão de cobertura integral ou de vulnerabilidade na VM.
- `review`: hash da skill lida, decisão humana necessária e confirmações `changesApplied=false`, `localFilesWritten=false`, `skillChanged=false`, `agentChanged=false`.

### Fontes e busca

O catálogo manual é [sources.json](../../../olivaw-agent/skills/safety-docker/references/sources.json). A lista humana está em [sources.md](../../../olivaw-agent/skills/safety-docker/references/sources.md). O operador pode remover fontes ou desabilitar enabled. Fontes page aceitam somente HTTPS público validado; um feed novo exige adaptador próprio em vez de interpretação arbitrária.

Além de CISA/NVD, as páginas existentes de Docker, Traefik, PostgreSQL, RabbitMQ, Redis e Amazon Linux são consultadas conforme o catálogo. HTTP 404, bloqueio ou excesso de resposta são reportados, sem troca silenciosa da fonte. Busca nova via RSS público do Bing, sem chave de API; não é scraping do Google nem usa automaticamente os buscadores disponíveis nesta conversa. O endpoint RSS é externo e pode mudar ou falhar.

Consultas configuráveis no setting, até três buscas, dez candidatos no padrão e filtro de relevância por título/URL. Resultados são intercalados para a primeira busca não consumir a lista inteira. Mesmo um domínio de fornecedor fica como candidato até revisão. Fóruns e publishers desconhecidos exigem corroboração primária.

### Leitura e limites

HTTP sem redirects, TLS validado para hostname original e conexão fixada em IP público previamente validado. Bloqueia IPs privados, localhost, URLs com credenciais/portas não previstas e DTD/XML nas buscas. Não envia cookies ou autenticação para fontes. Limites: 15 segundos por chamada, 8 MiB por feed, 1 MiB por página/busca, até doze fontes e três leitores de página simultâneos. NVD usa uma página de cem registros; truncamento torna o resultado parcial. Resultado parcial tem código de saída não zero e mantém informações aproveitáveis.

Leitura de HTML descarta script/style/noscript; texto externo é usado como evidência de termos, não como instrução. Avisos CVE são candidatos cuja aplicabilidade ainda precisa ser verificada. Não existem remediação, edição de skill, instalação de pacotes ou chamada paga de IA nesta operação.

Não grava nem limpa arquivos de propostas antigas; elas podem continuar existindo em estado de versões anteriores. A execução do Python do Baley/launcher desabilita criação de bytecode para não transformar uma consulta em alteração de __pycache__. Redirecionar stdout para arquivo é decisão explícita do operador, fora do comando.

### Uso

```bash
./olivaw-agent/olivaw-agent update
./olivaw-agent/olivaw-agent update --format text
```

O formato texto apresenta #source, #discovery e #learn e informa o resultado. O padrão é JSON indentado, com detalhes por fonte/busca. Nenhuma fonte ou aprendizado foi promovido automaticamente à skill instalada; o instalador continua ação manual separada.

### Validação

Dez testes do Baley e três do agrupador/instalador aprovados. Testes verificam as três seções, sugestões ancoradas, rejeição de dados privados/DTD, fonte desabilitada não consultada, ausência de vazamento de exceção e snapshot completo do projeto/skill antes/depois. Lifecycle continua testado em diretórios temporários.

Consulta pública inicial de validação leu CISA/NVD e páginas aprovadas, produziu seis ações de revisão e sinalizou fonte 404 e limites de resposta/paginação. Busca RSS retornou páginas; o refinamento de relevância descarta páginas genéricas sem relação com segurança. Não declarar que fontes encontradas são confiáveis ou novas vulnerabilidades foram confirmadas.

A [feature ampla](../discovery/agentes-python-baley-e-relatorios.md) continua em andamento: coleta periódica, modelo, API/RawData e migrations não foram implementados nesta entrega. Não houve alteração do acesso SSH ou chave AWS.

## user-feedback

```text
massa blz, o command update deveria fazer a analise nessas fontes e ver se encontra melhorias para nossa skill de safety sacou? o update tbm deve googlar na internet por novas fontes confiaveis de informacao sobre falhas de segurança. o command update deve listar no terminal a seguinte saida.

#source
http://blabla bla
outro site
outro site
etc

#discovery
site novo
forum novo
etc

#learn
1 - analisar vulnerabilidade do pacote ngrx
2 - bla bla bla

entao com essa saida eu poderia ajustar a skill com as novas fontes encontradas em discovery, ou remover alguma das fontes atuais, e ainda aplicar na skill os novos pontos a aprender.

o que acha?

faça uma saida json para isso tbm claro blz.
formatado e bem estruturado.

isso, o update em SI não muda nada no baley nem na skill. Ele apenas lista essa informacoes e o operador (que no caso sou eu) vou definir quais usar na skill
```
