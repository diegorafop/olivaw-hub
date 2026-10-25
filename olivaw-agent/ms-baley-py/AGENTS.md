# Diretrizes — Baley

- Baley é o responsável pela segurança no agrupador Olivaw. Projeto gerado pelo scaffold `ms-limpo-py`, pacote `ms_baley`, Python 3.10+, indentação de dois espaços.
- Preserve domain/core/features/data e composição explícita por construtor. Biblioteca padrão; dependências adicionais exigem necessidade demonstrada.
- Sem banco, drivers SQL, migrations ou DSNs. Persistência futura será pela API autenticada da Olivaw; a rota continua em planejamento.
- `setting.json` é o arquivo real. Segredos são referências externas, nunca valores no JSON ou logs.
- Lifecycle local start/stop/status não pode disparar coleta/IA enquanto o contrato de execução estiver pendente. Não fingir que um worker aguardando configuração está auditando.
- Worker usa lock, estado privado e parada cooperativa; não matar um PID apenas porque consta num arquivo.
- `update` consulta fontes aprovadas e busca fontes candidatas, com limites, somente para listar source/discovery/learn. Não cria propostas em disco, não altera Baley/skill/cópia do Codex e não executa conteúdo externo. Só o operador decide o que incorporar.
- Skill canônica está em `olivaw-agent/skills/safety-docker`; o setting do Baley usa `../skills/safety-docker/SKILL.md`, relativo à pasta do projeto, não ao cwd.
- Testes sem SSH/IA/API reais, em diretórios temporários; encerrar workers criados em testes. Primeiro ciclo Linux; Windows não comprovado.
