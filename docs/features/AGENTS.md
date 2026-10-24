# Diretrizes — Features

## Organização

- Mantenha documentos em português e preserve a marca Olivaw. Os nomes dos quatro blocos são fixos em inglês.
- `index.md` é o índice principal e contém somente funcionalidades documentadas em `done/`. Não inclua ideias, fila, trabalho em andamento ou revisão nesse índice.
- Cada feature tem um arquivo próprio, com nome estável e descritivo. Preserve identificadores existentes; números de planejamento não são números de issues.
- Agrupamentos por produto ou épica podem existir dentro dos estados. Épicas são planejamento; sua presença não comprova implementação das features relacionadas.
- Ao mover um arquivo, atualize links relativos de entrada e saída e o índice quando aplicável.

## Estados

- `discovery/`: ideias e especificações em elaboração, com decisões ou dependências pendentes.
- `ready/`: escopo, critérios verificáveis e dependências definidos; aguardando desenvolvimento.
- `in-progress/`: desenvolvimento assumido por um responsável.
- `review/`: implementação e verificações concluídas pelo responsável, aguardando revisão e integração.
- `done/`: desenvolvimento concluído e validado, movido conforme as entregas forem finalizadas. Integração em develop, migrations, publicação e validações de ambiente pendentes devem permanecer explícitas no documento; mover o arquivo não autoriza essas ações.
- Publicação e validação no ambiente são registradas separadamente no documento. Estar em `done/` não significa estar publicado e não autoriza implantação nem fechamento remoto de issue.
- Registre bloqueios no `ai-prompt`, com motivo e dependência; não crie cópias da mesma feature em vários estados.

## Formato obrigatório

Cada arquivo de feature tem um título descritivo e exatamente estes quatro blocos principais, nesta ordem:

```markdown
# Título da feature

## user-history

Como [papel],
quero [ação ou capacidade],
para [benefício esperado].

## acceptance-criteria

- Comportamento observável e verificável.
- Condição de acesso, falha ou limite relevante.

## ai-prompt

Objetivo e estado atual.
Escopo autorizado, exclusões, dependências e decisões pendentes.
Referências técnicas, verificações e evidências de conclusão.
Responsável, issue, branch e worktree quando houver execução atribuída.

## user-feedback

Texto original recebido, sem correções de escrita ou resumo.
```

- Use critérios em lista de bullets. Diferencie requisito de evidência de teste; não invente aprovação ou validação.
- `ai-prompt` deve orientar o agente com contexto específico e instruções executáveis. Subseções de nível 3 ou inferior são permitidas nesse bloco.
- Concentre estado, decisões técnicas, limitações, testes e publicação no `ai-prompt`, sem criar um quinto bloco principal.
- `user-feedback` preserva o texto raw e sua ordem. Use cerca de código Markdown quando necessário para manter os títulos originais sem criar novos blocos principais. Escolha uma cerca maior que qualquer cerca presente no conteúdo.
- Na migração inicial, o documento anterior completo é o registro original; isso não significa que todo o texto foi escrito pelo usuário. Somente caminhos relativos podem ser ajustados, com essa exceção explicitada.
- Ao receber novo feedback, acrescente o texto original e atualize a especificação nos demais blocos. Diferencie pedidos atuais de registros históricos; texto preservado não autoriza execução por si só.
- Evolução pendente de uma feature concluída recebe documento próprio no planejamento. Não reabra implicitamente toda a entrega histórica.

## Trabalho com agentes

- Antes de iniciar, confira o branch e as instruções dos módulos afetados. Preserve Git Flow e rastreabilidade conforme [o guia](../gitflow.md).
- Cada feature em execução tem um responsável e um branch próprio, preferencialmente em worktree separado. Agentes concorrentes não devem compartilhar diretório de trabalho nem trocar o branch uns dos outros.
- Registre no `ai-prompt` responsável, issue quando existente, branch, worktree, dependências e arquivos compartilhados previstos antes de começar.
- Combine alterações em contratos, arquivos compartilhados e infraestrutura entre responsáveis. Integrações devem ser coordenadas em sequência, com validação dos conflitos.
- Não mova uma feature para `ready/` apenas porque o documento existe: resolva as decisões que impedem sua execução.
- Concluir documentação não autoriza implementar a feature. Migrations, finish, push, operações remotas e implantação exigem autorização específica na tarefa.

## Validação documental

- Confira os quatro blocos, critérios em bullets, preservação do registro original e links locais após reorganizações.
- O índice principal deve corresponder aos documentos de funcionalidades concluídas, sem listar o backlog.
- Alterações apenas documentais não exigem builds, testes de aplicação, acesso a bancos ou publicação.
