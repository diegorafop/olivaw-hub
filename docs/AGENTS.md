# Diretrizes — Documentação

## Escopo e organização

Mantenha os documentos em português, com títulos descritivos, exemplos concretos e links relativos. `README.md` é o índice desta pasta; `desenvolvimento.md` explica o fluxo atual; `identidade.md` registra a extração e suas verificações. Atualize o índice ao acrescentar um guia relevante.

## Precisão

- Siga o formato e os estados definidos em [features/AGENTS.md](features/AGENTS.md). O índice principal é [features/index.md](features/index.md) e lista somente funcionalidades concluídas. Features com desenvolvimento concluído ficam em `features/done/`, com índice e referências atualizados. Preserve pendências de publicação e validação do ambiente no documento; evolução não implementada deve permanecer no planejamento, em arquivo próprio quando apenas um recorte foi concluído.

- Confira nomes, caminhos e comandos na implementação antes de documentá-los.
- Identifique exemplos futuros, como a API da pastelaria, para não apresentá-los como componentes existentes.
- Distinga pasta, `.csproj`, `ProjectReference` e `.sln`. A solução da raiz agrupa projetos; não define dependências nem executa uma aplicação.
- Registre decisões operacionais no guia adequado e contratos específicos nos READMEs dos módulos, evitando duplicações extensas.
- Separe resultados históricos de testes e limitações do ambiente das instruções gerais de desenvolvimento.
- Use placeholders para segredos e conexões; não copie valores reais do ambiente.

## Validação

Confira links locais, exemplos de caminhos e a estrutura do Markdown. Mudanças apenas documentais não exigem executar builds, testes, geração de clientes, migrations ou publicações. Não execute comandos com efeitos externos apenas para conferir um exemplo.

## Documentação de clientes

- Centralize briefings, brainstorms, acervo, decisões e melhorias específicas em `clients/<cliente>/`.
- `features/` contém apenas funcionalidades do hub: infraestrutura, site e API Olivaw, agents, identidade e componentes compartilhados. Uma referência a um cliente como exemplo não transforma uma feature do hub em feature do cliente.
- Cada cliente tem `README.md`, `brainstorm/` para estudos livres e `features/` com os mesmos estados e quatro blocos das features do hub. Preserve os estados ao reorganizar documentos.
- READMEs de execução e instruções junto ao código permanecem nos módulos e apontam para a documentação central do cliente. Não duplique especificações entre os dois locais.
