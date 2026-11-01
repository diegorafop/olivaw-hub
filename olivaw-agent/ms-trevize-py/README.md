# Trevize — Prospecção de empresas

Agente local gerado pelo scaffold `ms-limpo-py`, com CQS e persistência somente pela API Olivaw. Busca empresas de eventos de Curitiba, rola o painel público do Maps e reúne até 20 candidatos novos por ciclo. Faz leitura passiva de sites e prepara qualificação e abordagem em rascunho para revisão humana. Não envia mensagens, ligações ou formulários.

## Executar

Na raiz do monorepo:

```bash
python3 -m venv olivaw-agent/ms-trevize-py/.venv
olivaw-agent/ms-trevize-py/.venv/bin/python -m pip install -e ./olivaw-agent/ms-trevize-py
olivaw-agent/ms-trevize-py/.venv/bin/python -m playwright install chromium
olivaw-agent/olivaw-agent status --agent trevize
olivaw-agent/olivaw-agent start --agent trevize
olivaw-agent/olivaw-agent stop --agent trevize
```

O ambiente local foi preparado. Se o Python não incluir `ensurepip`, preparar o suporte a venv/pip da distribuição ou usar o pip oficial apenas no ambiente virtual; não instalar pacotes no Python global.

`olivaw-agent start` inicia todos os agentes descobertos. Também é possível executar `.venv/bin/trevize start` dentro desta pasta. Importar o pacote, instalar, consultar status ou abrir o frontend não inicia o worker.

**Sem autostart:** somente `start` inicia. Primeiro ciclo imediato; próximo ciclo 24 horas após terminar o anterior, sem sobreposição. O supervisor fica aberto enquanto espera. `stop` solicita parada cooperativa; uma leitura/IA em andamento pode precisar terminar dentro do timeout. Desligar o PC encerra os processos; não há retomada, serviço systemd, cron ou inicialização com o shell. Após ligar o PC, executar start novamente.

## Configuração

O arquivo real é `setting.json`, com paths relativos a esta pasta:

- `search`: região, localização da busca, segmento, limite, exclusões, candidatos e rolagens máximos. Região é recorte de busca, não comprovação de endereço de cada empresa.
- `schedule.intervalHours`: 24. `schedule.enabled=false` permite um ciclo por start. `execution.enabled=false` deixa o supervisor aguardando configuração, sem rede/IA.
- `execution.timeoutSeconds`: orçamento por lote; leituras e IA também têm limites próprios. Pode produzir lote parcial se a fonte, tempo ou entrega impedir completar 20.
- `ai`: Codex CLI autenticado localmente, modelo configurável (null usa padrão do CLI), até 20 chamadas por ciclo. Nenhuma chamada paga foi usada na validação. IA roda em modo read-only, sem ferramentas, com JSON Schema e skill fornecida no prompt.
- `skill.path`: fonte versionada `../skills/trevize-prospect/SKILL.md`. Não há update autônomo da skill.
- `leadApi`: conta técnica em `../../_secrets/admin.json`, seção `trevize`, arquivo 0600. JWT somente em memória. Exige `leads.manage` e rejeita root. A conta técnica foi criada em 01/11/2026 com essa permissão; não dar acesso a deploy, banco ou outros módulos.
- `lifecycle.autoStart`: false obrigatório. `localState`: diretório privado e limites. Apenas último resumo é mantido (limpo após retenção na próxima preparação); índice de deduplicação contém até 5.000 hashes e datas, limitado em bytes. Sem HTML, contatos, tokens ou inventário bruto persistidos localmente.

Antes de coletar, autentica e confere disponibilidade de `PUT /leads/{id}/qualification`. Se o backend ainda não tiver essa operação publicada, interrompe antes de Maps e IA. Em 01/11/2026, a disponibilidade dessa operação foi confirmada após a publicação. Preparar o agente não autoriza deploy.

## Persistência e revisão

Usa `POST /leads`, `GET /leads/{id}` e `PUT /leads/{id}/qualification`, com UUID determinístico por identidade do resultado, revisão atual e releitura para confirmação. Repetições não criam outro UUID. Se já há qualificação, não sobrescreve trabalho humano. Nome idêntico em outro Lead gera indicação de possível duplicidade, sem fusão automática; isso pode deixar homônimos aguardando revisão.

A ficha em `/admin/leads/<id>/details` separa fatos, positivos, observações, perguntas e rascunho. Confiança permanece pendente e abordagem em draft, independentemente da análise automática. Telefone do Maps fica como telefone; WhatsApp exige link encontrado no site. Falha de leitura é inconclusiva. Não mede desempenho ou qualidade visual nem efetua auditoria de segurança. Links de contato não são enviados.

A POC validou pesquisa e rolagem com 20 candidatos; o coletor desta versão foi testado com fixtures locais, sem nova extração remota, sem IA paga e sem cadastro de lote real. Interface do Maps pode mudar ou exigir intervenção; não contornar CAPTCHA, consentimento ou bloqueios. Permissões de uso e armazenamento da fonte precisam ser consideradas para operação recorrente: [políticas](https://developers.google.com/maps/documentation/places/web-service/policies) e [termos](https://cloud.google.com/maps-platform/terms). Acesso técnico não equivale a licença irrestrita.

## Validação

```bash
PYTHONPATH=olivaw-agent/ms-trevize-py/src python3 -m unittest discover -s olivaw-agent/ms-trevize-py/tests -v
python3 -m unittest discover -s olivaw-agent/tests -v
```

Testes sem API, banco, Maps ou IA reais: configurações, rolagem/deduplicação, fontes, URLs privadas, leitura passiva, saída IA, revisão da API e lifecycle em diretório temporário. Linux/POSIX; Windows não implementado.

Referências: [ficha de Leads](../../docs/features/done/leads-ficha-qualificacao-e-abordagem.md), [discovery](../../docs/features/discovery/trevize-prospeccao-e-qualificacao-de-leads.md), [Codex não interativo](https://developers.openai.com/codex/noninteractive), [Playwright Python](https://playwright.dev/python/docs/library).

## Diagnóstico de execução

O status informa `lastCollection.phase` quando a autenticação/verificação da API (`api-preflight`) ou a leitura do Maps (`maps-collection`) falha. Outras falhas usam `batch-processing`. Mensagens brutas de dependências não são gravadas, pois podem conter credenciais ou dados de contatos. O intervalo continua contado após o término do ciclo, inclusive se falhar; não há repetição automática imediata.

Em 01/11/2026, a tentativa anterior tinha apenas erro genérico e sua causa não pôde ser recuperada. A composição local, autenticação do Codex, verificação da API e uma amostra de leitura do Maps funcionaram no diagnóstico. Isso não valida ainda um ciclo completo com IA e entrega de Leads. Nenhum lote ou chamada de IA foi executado nesse diagnóstico; os agentes não foram reiniciados. Os 19 testes Python passaram.
