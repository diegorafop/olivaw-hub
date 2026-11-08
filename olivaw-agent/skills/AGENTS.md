# Diretrizes — Skills dos agentes

- Cada pasta tem `SKILL.md` com frontmatter `name`/`description`, instruções focadas e referências carregadas conforme necessidade.
- Preserve a descrição curta e os limites de autorização. Não transformar autorização de uma execução manual em permissão para mutações recorrentes.
- `safety-docker` audita; correções dependem de tarefa explícita. Nenhuma atualização de fonte externa promove automaticamente nova versão.
- Update apenas apresenta source/discovery/learn e não grava arquivo nem altera agente/skill. Só o operador seleciona fontes e pontos de aprendizado; alterações e instalação são ações manuais separadas.
- Não guardar inventários privados, chaves, tokens, dumps ou resultados reais na pasta da skill.
