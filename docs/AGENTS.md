# Diretrizes — Documentação

## Escopo e organização

Mantenha os documentos em português, com títulos descritivos, exemplos concretos e links relativos. `README.md` é o índice desta pasta; `desenvolvimento.md` explica o fluxo atual; `identidade.md` registra a extração e suas verificações. Atualize o índice ao acrescentar um guia relevante.

## Precisão

- Features com desenvolvimento concluído ficam em `features/done/`, com índice e referências atualizados. Preserve pendências de publicação e validação do ambiente no documento; evolução não implementada deve permanecer no planejamento, em arquivo próprio quando apenas um recorte foi concluído.

- Confira nomes, caminhos e comandos na implementação antes de documentá-los.
- Identifique exemplos futuros, como a API da pastelaria, para não apresentá-los como componentes existentes.
- Distinga pasta, `.csproj`, `ProjectReference` e `.sln`. A solução da raiz agrupa projetos; não define dependências nem executa uma aplicação.
- Registre decisões operacionais no guia adequado e contratos específicos nos READMEs dos módulos, evitando duplicações extensas.
- Separe resultados históricos de testes e limitações do ambiente das instruções gerais de desenvolvimento.
- Use placeholders para segredos e conexões; não copie valores reais do ambiente.

## Validação

Confira links locais, exemplos de caminhos e a estrutura do Markdown. Mudanças apenas documentais não exigem executar builds, testes, geração de clientes, migrations ou publicações. Não execute comandos com efeitos externos apenas para conferir um exemplo.
