# Diretrizes — Baley

- Baley é o responsável pela segurança no agrupador Olivaw. Projeto gerado pelo scaffold `ms-limpo-py`, pacote `ms_baley`, Python 3.10+, indentação de dois espaços.
- Preserve domain/core/features/data e composição explícita por construtor. Biblioteca padrão; dependências adicionais exigem necessidade demonstrada.
- Sem banco, drivers SQL, migrations ou DSNs. Persistência exclusivamente pela API autenticada `/security-reports` da Olivaw.
- `setting.json` é o arquivo real. Segredos são referências externas, nunca valores no JSON ou logs.
- O worker executa ciclos de auditoria somente quando habilitados no setting, sem sobreposição. Distinga coleta, interpretação, fila e confirmação de entrega; não invente execução ou persistência.
- Worker usa lock, estado privado e parada cooperativa; não matar um PID apenas porque consta num arquivo.
- `update` consulta fontes aprovadas e busca fontes candidatas, com limites, somente para listar source/discovery/learn. Não cria propostas em disco, não altera Baley/skill/cópia do Codex e não executa conteúdo externo. Só o operador decide o que incorporar.
- Skill canônica está em `olivaw-agent/skills/safety-docker`; o setting do Baley usa `../skills/safety-docker/SKILL.md`, relativo à pasta do projeto, não ao cwd.
- Testes sem SSH/IA/API reais, em diretórios temporários; encerrar workers criados em testes. Primeiro ciclo Linux; Windows não comprovado.

- Correção exige ticket root na API, reserva exclusiva por host, procedimento fixo permitido e saúde confirmada antes/depois. Sem geração de shell pela IA. Qualquer container parado/instável bloqueia manutenção; limpeza nunca remove containers, volumes, redes ou dados de rollback. O catálogo inicial corrige apenas permissões delimitadas; limpeza não catalogada exige intervenção.
