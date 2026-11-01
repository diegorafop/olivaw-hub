# ms-trevize-py

Esqueleto gerado do ms-limpo-py: template mínimo para agentes Python locais em Linux e Windows, com Command–Query Separation (CQS), composição explícita e dependências pequenas. Python 3.10 ou superior.

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
.venv/bin/python -m ms_trevize example --name exemplo
.venv/bin/python -m unittest discover -s tests -v
```

Windows PowerShell:

```powershell
py -3 -m venv .venv
.\.venv\Scripts\python.exe -m pip install -e .
.\.venv\Scripts\python.exe -m ms_trevize example --name exemplo
.\.venv\Scripts\python.exe -m unittest discover -s tests -v
```

O exemplo cria e consulta uma execução em memória no mesmo processo. Não representa persistência de produção. Após instalação, `trevize example` também está disponível no ambiente virtual.

## Organização e dependências

```text
src/ms_trevize/
  domain/       Entidades e invariantes; somente biblioteca padrão
  core/         Contratos necessários aos casos de uso; depende de domain
  features/     Casos de uso agrupados por feature, com commands e queries
  data/         Adaptadores de persistência e migrações SQL
  bootstrap.py  Composição dos adaptadores e handlers por construtor
  cli.py        Entrada e apresentação dos resultados
```

Features dependem de core/domain. Data implementa os contratos de core; domínio não importa infraestrutura. Commands alteram estado; queries apenas consultam. Sem mediator, service locator, ORM obrigatório ou servidor HTTP. Integrações futuras ficam em adaptadores próprios, adicionados quando necessários.

## Configuração e migrações

O exemplo não requer configuração. O arquivo real `setting.json` acompanha o scaffold, com agendamento/execução desabilitados, identidade ajustada ao agente e modelo/skill ainda não selecionados. `configuration.load_settings(path)` valida o JSON sem abrir rede; comandos que usarem configuração devem receber caminho explícito e resolver caminhos relativos ao arquivo. O JSON não contém senhas. Agentes derivados devem receber segredos externamente, sem gravá-los no repositório, argumentos ou logs. Não há carregamento automático de `.env`.

Para habilitar o comando PostgreSQL:

```bash
python -m pip install -e '.[postgres]'
python -m ms_trevize.data.apply_migrations --schema agent_trevize
```

A esteira deve fornecer `AGENT_MIGRATION_DSN` por seu mecanismo de segredos. `trevize-migrate --schema agent_trevize` é a entrada equivalente. Use identidade de migração própria; a credencial de execução do agente não precisa alterar estruturas.

O provisionamento de banco/schema/usuários pertence à futura rota do `ms-devops-api`, acionada pelo `ms-holos-ng` para o ambiente selecionado. O executor exige schema existente e aplica arquivos de `data/migrations/` em ordem, com checksum, lock e transação por arquivo. Há timeout de espera de 30 segundos e de instrução de 5 minutos. Não há rollback automático de versões já aplicadas: correções são novas migrações. Migrações não rodam ao iniciar ou executar o agente.

O template não contém tabelas de negócio nem implementação de provisionamento. `AGENT_MIGRATION_DSN` não deve ser repassado ao subprocesso de análise de IA.

## Validação

Na implementação inicial, em Linux/Python 3.14, passaram 13 testes no template (incluindo seis do scaffold) e sete no Giskard gerado. O teste de independência remove a cópia temporária do template antes de executar o projeto gerado. Foram verificados geração, nomes inválidos, proteção de destinos existentes, rejeição de links e exclusão de arquivos fora da lista permitida.

```bash
PYTHONPATH=src python3 -m unittest discover -s tests -v
```

A instalação e os comandos de console não foram validados nessa execução inicial, pois o Python disponível não tinha `pip`/`ensurepip`. Nenhum serviço real foi acessado. A instalação do suporte a `.venv` é descrita acima; isso não indica que os pacotes já foram instalados nesta máquina.

Testes usam `unittest`, isolados de serviços externos. A suíte cobre o exemplo CQS e a integridade do histórico de migrações. Integração PostgreSQL (permissões, lock, transações e retomada) precisa ser validada com banco descartável antes de usar a esteira. Windows precisa de validação própria; não foi executado neste ambiente Linux.
