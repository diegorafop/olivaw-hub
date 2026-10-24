# ms-limpo-py

Template mínimo para agentes Python locais em Linux e Windows, com Command–Query Separation (CQS), composição explícita e dependências pequenas. Python 3.10 ou superior.

## Desenvolvimento

Cada projeto deve ter sua própria `.venv`. No Ubuntu, se `python3 -m venv` falhar por ausência de `ensurepip`, instale o suporte a ambientes virtuais:

```bash
sudo apt update
sudo apt install python3-venv
```

O ambiente virtual inclui seu próprio `pip`; não use `sudo pip install`. Não é necessário instalar PostgreSQL local para desenvolver agentes que acessarão o servidor remoto. O exemplo e os testes usam somente biblioteca padrão; a dependência PostgreSQL é opcional e deve ser instalada quando necessária com `python -m pip install -e '.[postgres]'` usando o Python da `.venv`.

Linux:

```bash
python3 -m venv .venv
.venv/bin/python -m pip install -e .
.venv/bin/python -m ms_limpo example --name exemplo
.venv/bin/python -m unittest discover -s tests -v
```

Windows PowerShell:

```powershell
py -3 -m venv .venv
.\.venv\Scripts\python.exe -m pip install -e .
.\.venv\Scripts\python.exe -m ms_limpo example --name exemplo
.\.venv\Scripts\python.exe -m unittest discover -s tests -v
```

O exemplo cria e consulta uma execução em memória no mesmo processo. Não representa persistência de produção. Após instalação, `ms-limpo example` também está disponível no ambiente virtual.

## Criar um agente

Na raiz deste template, em Linux ou Windows com Python 3.10+:

```bash
python scripts/new_agent.py giskard --output ..
```

O desenvolvedor informa somente o nome curto. O prefixo `ms-` e o sufixo `-py` são fixos: todo novo projeto segue **`ms-<nome>-py`**. Não informe o nome completo ao comando.

| Item | Padrão | Exemplo com `giskard` |
| --- | --- | --- |
| Diretório e distribuição | `ms-<nome>-py` | `ms-giskard-py` |
| Pacote Python | `ms_<nome>` | `ms_giskard` |
| Comando | `<nome>` | `giskard` |
| Comando de migração | `<nome>-migrate` | `giskard-migrate` |
| Schema convencional | `agent_<nome>` | `agent_giskard` |

`--output` é um diretório pai existente, relativo ao diretório de execução; o template é localizado pelo próprio script. Aceita nomes curtos com letras ASCII minúsculas, números e hífens entre segmentos, começando por letra, até 57 caracteres. `limpo` é reservado; argumentos com prefixo `ms-` ou sufixo `-py` são recusados. Hífens tornam-se underscores no pacote e no schema: `daneel-olivaw` gera `ms-daneel-olivaw-py`, pacote `ms_daneel_olivaw`, comando `daneel-olivaw` e schema `agent_daneel_olivaw`.

O scaffold aceita nomes simples ou compostos separados por hífen. O nome do comando é exatamente o nome escolhido pelo desenvolvedor. O scaffold cria novos projetos e não atualiza destinos existentes.

O destino precisa ser inexistente e ficar fora do template. Diretórios existentes, mesmo vazios, arquivos e links são recusados. A lista explícita em `scripts/new_agent.py` inclui somente código demonstrativo, testes, metadados e documentação. Não copia Git, ambientes, caches, builds, credenciais, configurações pessoais nem o próprio gerador. Novos arquivos do template precisam ser incluídos conscientemente nessa lista. Imports e identificadores conhecidos são ajustados por tipo de arquivo; conteúdo arbitrário não é percorrido. Todas as leituras e validações precedem as escritas; uma falha de disco pode deixar um destino parcial, que deve ser inspecionado antes de uma nova tentativa.

Depois, no diretório gerado, siga os comandos de desenvolvimento acima com `ms_giskard` em lugar de `ms_limpo`. O projeto é independente do template e mantém a demonstração em memória até a implementação dos casos de uso reais. As mesmas orientações de criação estão no [AGENTS.md](AGENTS.md).

## Organização e dependências

```text
src/ms_limpo/
  domain/       Entidades e invariantes; somente biblioteca padrão
  core/         Contratos necessários aos casos de uso; depende de domain
  features/     Casos de uso agrupados por feature, com commands e queries
  data/         Adaptadores de persistência e migrações SQL
  bootstrap.py  Composição dos adaptadores e handlers por construtor
  cli.py        Entrada e apresentação dos resultados
```

Features dependem de core/domain. Data implementa os contratos de core; domínio não importa infraestrutura. Commands alteram estado; queries apenas consultam. Sem mediator, service locator, ORM obrigatório ou servidor HTTP. Integrações futuras ficam em adaptadores próprios, adicionados quando necessários.

## Configuração e migrações

O exemplo não requer configuração. Agentes derivados devem receber segredos externamente, sem gravá-los no repositório, argumentos ou logs. Não há carregamento automático de `.env`.

Para habilitar o comando PostgreSQL:

```bash
python -m pip install -e '.[postgres]'
python -m ms_limpo.data.apply_migrations --schema agent_giskard
```

A esteira deve fornecer `AGENT_MIGRATION_DSN` por seu mecanismo de segredos. `ms-limpo-migrate --schema agent_giskard` é a entrada equivalente. Use identidade de migração própria; a credencial de execução do agente não precisa alterar estruturas.

O provisionamento de banco/schema/usuários pertence à futura rota do `ms-devops-api`, acionada pelo `ms-holos-ng` para o ambiente selecionado. O executor exige schema existente e aplica arquivos de `data/migrations/` em ordem, com checksum, lock e transação por arquivo. Há timeout de espera de 30 segundos e de instrução de 5 minutos. Não há rollback automático de versões já aplicadas: correções são novas migrações. Migrações não rodam ao iniciar ou executar o agente.

O template não contém tabelas de negócio nem implementação de provisionamento. `AGENT_MIGRATION_DSN` não deve ser repassado ao subprocesso de análise de IA.

## Agentes derivados

Use o comando de scaffold acima. Remova a feature demonstrativa quando houver um caso de uso real. Não crie bibliotecas compartilhadas antes de existir necessidade comprovada.

O futuro Giskard continua local, acessando diretamente `holos_devops.agent_giskard`, com vários desenvolvedores compartilhando o banco. Preservará `report_only`, `comment` e `submit_review`; persistirá a análise estruturada para consulta via `ms-devops-api`. HTML e dashboard ficam no futuro `ms-devops-ng`. A migração do Giskard não faz parte deste esqueleto.

### Decisões para o Giskard

- Origem da futura migração: `~/projects/agent-codex/code-review`. Destino gerado: `~/projects/ms-giskard-py`; template: `~/projects/ms-limpo-py`.
- Servidor PostgreSQL remoto: `postgres-desenvolvimento.holos.mps.com.br:5432`; banco `holos_devops`; schema `agent_giskard`. Não usar localhost nem o banco da conexão antiga. Para outros agentes, o schema segue `agent_<nome>`.
- Nesta etapa, o usuário criará banco e schema manualmente. Credenciais devem ser fornecidas externamente, sem senha no código, README, argumentos ou logs. A configuração e a persistência funcional ainda serão implementadas.
- Preservar `report_only` como padrão; publicação somente por seleção explícita de `comment` ou `submit_review`. Preservar a skill code-review, análise somente leitura, checkpoints, histórico, recuperação e reconciliação de publicações.
- Persistir a análise completa e estruturada para consumo posterior pela `ms-devops-api`. A geração de relatório local e dashboard sairá do agente. A interface futura ficará no `ms-devops-ng`, consultando a API; o estudo de UI/UX será feito com o usuário, sem HTML, dashboard ou layout nesta etapa.
- O provisionamento futuro será responsabilidade de uma rota na `ms-devops-api`, acionada pelo `ms-holos-ng` para o ambiente selecionado. Migrações serão executadas explicitamente pela futura esteira com identidade própria, nunca na inicialização ou execução normal do agente.
- Esta entrega cobre scaffold e estrutura independente, preservando core, domain, features e data, commands e queries separados, composição explícita e injeção por construtor. Não inclui migração funcional, conexão a serviços reais, provisionamento ou esteiras.

## Validação

Na implementação inicial, em Linux/Python 3.14, passaram 13 testes no template (incluindo seis do scaffold) e sete no Giskard gerado. O teste de independência remove a cópia temporária do template antes de executar o projeto gerado. Foram verificados geração, nomes inválidos, proteção de destinos existentes, rejeição de links e exclusão de arquivos fora da lista permitida.

```bash
PYTHONPATH=src python3 -m unittest discover -s tests -v
```

A instalação e os comandos de console não foram validados nessa execução inicial, pois o Python disponível não tinha `pip`/`ensurepip`. Nenhum serviço real foi acessado. A instalação do suporte a `.venv` é descrita acima; isso não indica que os pacotes já foram instalados nesta máquina.

Testes usam `unittest`, isolados de serviços externos. A suíte cobre o exemplo CQS e a integridade do histórico de migrações. Integração PostgreSQL (permissões, lock, transações e retomada) precisa ser validada com banco descartável antes de usar a esteira. Windows precisa de validação própria; não foi executado neste ambiente Linux.
