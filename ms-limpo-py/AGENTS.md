# Repository Guidelines

## Estrutura

O template mantém código em `src/ms_limpo/` e testes em `tests/`. `domain` contém entidades e invariantes; `core`, contratos; `features`, commands e queries organizados por caso de uso; `data`, adaptadores e migrações. `bootstrap.py` compõe dependências e `cli.py` recebe entradas.

## Criar novos agentes

Na raiz de `~/projects/ms-limpo-py`, execute `python scripts/new_agent.py <nome> --output ..` (ou `python3` no Linux). O desenvolvedor escolhe somente `<nome>`; o diretório e o nome de distribuição sempre seguem `ms-<nome>-py`, com prefixo `ms-` e sufixo `-py` fixos. Não passe o nome completo ao comando.

Exemplo: `python scripts/new_agent.py giskard --output ..` gera `../ms-giskard-py`, pacote `ms_giskard`, comandos `giskard` e `giskard-migrate`, schema convencional `agent_giskard`. Hífens no nome tornam-se underscores no pacote e no schema. Use letras ASCII minúsculas e números, começando por letra; hífens somente entre segmentos; máximo de 57 caracteres. `limpo`, prefixo `ms-` e sufixo `-py` no argumento são recusados.

`--output` deve apontar para um diretório pai existente fora do template. Nunca sobrescrever destinos existentes, mesmo vazios, nem copiar links, Git, ambientes virtuais, caches, builds, credenciais ou configurações pessoais. Mantenha a lista explícita de arquivos do scaffold e substituições controladas. Não inicialize Git automaticamente.

No projeto gerado, crie `.venv`, instale com `python -m pip install -e .`, execute `python -m ms_giskard example` e `python -m unittest discover -s tests -v`, adaptando o pacote ao nome escolhido. Valide o scaffold com `PYTHONPATH=src python3 -m unittest discover -s tests -v` no template; testes de geração precisam executar sem depender da cópia de origem.

Nomes simples ou compostos são aceitos: `daneel-olivaw` gera `ms-daneel-olivaw-py`, pacote `ms_daneel_olivaw`, comando `daneel-olivaw` e schema `agent_daneel_olivaw`. O scaffold cria novos projetos; não migra nem atualiza projetos existentes. As orientações desta seção pertencem somente ao template e não são copiadas para o AGENTS.md gerado.

## Arquitetura e estilo

Use Python 3.10+, dois espaços por nível de indentação, UTF-8, `snake_case` para módulos/funções e `PascalCase` para classes. Respeite `.editorconfig` e o YAPF configurado no VS Code. Domínio não depende de dados ou apresentação. Features usam contratos de core; adaptadores implementam esses contratos. Injete dependências por construtor e preserve CQS: commands alteram estado, queries consultam. Prefira composição simples e biblioteca padrão; justifique dependências adicionais. Não crie frameworks, classes base ou abstrações sem necessidade concreta.

## Desenvolvimento e testes

Crie ambiente com `python -m venv .venv` e instale com `python -m pip install -e .`. Execute `python -m ms_limpo example` e `python -m unittest discover -s tests -v` com o Python desse ambiente. Use `unittest` e mocks; nomeie testes como `test_*.py` e métodos como `test_<comportamento>`. Não há meta numérica de cobertura. Testes de unidade não acessam serviços reais; valide integrações separadamente em infraestrutura descartável.

## Banco e segurança

PostgreSQL é opcional. Provisionamento pertence ao `ms-devops-api`; migrações são executadas explicitamente pela esteira com identidade própria. Nunca aplique migrações na inicialização do agente. Não altere SQL já aplicado. Não versione segredos ou registre DSNs; subprocessos recebem somente as credenciais necessárias. Preserve execução local e isolamento dos futuros agentes.

## Commits e pull requests

Sem histórico Git inicial disponível, use assuntos imperativos e concisos. PRs explicam problema, comportamento resultante, alterações de contrato/configuração/schema e validação. Informe testes não executados, inclusive Windows e PostgreSQL quando aplicável. Preserve alterações preexistentes e não faça deploy ou operações remotas como consequência de mudanças locais.
