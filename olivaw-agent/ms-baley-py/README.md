# Baley — Segurança da Olivaw

Projeto `ms-baley-py`, pacote `ms_baley`, gerado pelo scaffold do `ms-limpo-py` e adaptado para operação sem banco. Não contém driver SQL, DSN ou comando de migration. Sua skill é [safety-docker](../skills/safety-docker/SKILL.md).

## Comandos

Na raiz do monorepo, sem instalar dependências:

```bash
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m ms_baley start
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m ms_baley status
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m ms_baley stop
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m ms_baley update
```

Ou use o [agrupador](../README.md) para todos os agentes. Para instalação local opcional, crie `.venv` e execute `pip install -e .`; o comando `baley` ficará disponível na venv. A configuração padrão é resolvida pela pasta do projeto no checkout/editable; instalação em wheel fora do checkout exige `--settings /caminho/setting.json` explícito e skill disponível no caminho configurado.

`start` é idempotente, usa lock e inicia supervisor destacado. `stop` solicita parada por identidade de instância, sem matar um PID lido de arquivo. `status` verifica o lock real, sem confundir arquivo antigo com processo ativo. Estado privado em `.state/`, ignorado pelo Git; não há log contínuo crescente. Suporte inicial de lifecycle Linux/POSIX.

O supervisor está em `waiting-configuration`: mantém heartbeat e aguarda o contrato de análise, sem acessar SSH/modelo/API. `execution.enabled=false` é obrigatório nesta versão; habilitar antecipadamente falha antes de iniciar. `schedule.intervalHours=6` fica preparado, mas **não há coleta periódica implementada ainda**. `schedule.enabled` não habilita análise enquanto esse contrato estiver pendente. Nenhuma conta/token/modelo é inventado.

`update` consulta fontes aprovadas, busca fontes candidatas e lista source/discovery/learn para decisão do operador. Não executa IA, não grava arquivos nem altera o agente ou qualquer cópia da skill. Use `--format text` para os blocos com #; JSON indentado é o padrão. Consulte o [contrato do agrupador](../README.md#update--pesquisa-somente-leitura).

## Organização

- `configuration.py`: carregador/validador de `setting.json` vindo do template.
- `core/runtime.py`: contrato do lifecycle.
- `features/lifecycle/commands` e `queries`: casos de start, stop e status.
- `features/skill_update/commands`: consulta e apresentação de source/discovery/learn.
- `data`: processos locais, HTTP público, fontes e descoberta; não é banco.
- `worker.py`: supervisor local, sem execução remota.
- `bootstrap.py`: composição explícita por construtor.

O envelope de relatórios e armazenamento RawData pela API está na [feature em discovery](../../docs/features/discovery/agentes-python-baley-e-relatorios.md). Nenhum endpoint/tabela/migration de relatórios foi criado nesta entrega.
