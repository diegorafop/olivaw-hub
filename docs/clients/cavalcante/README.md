# Cavalcante — descoberta e protótipo

Cliente **Cavalcante Terraplenagem e Locação**, projeto **Olivaw**, issue **#24**, branch `feature/issue024-cavalcante`. Estudo iniciado em 24/10/2026. Responsável pela execução: Codex; Nicolas participa como estagiário em formação.

- [Estudo do site e inventário](estudo-site.md): evidências, conteúdo existente, problemas e proposta de navegação.
- [Cores e identidade](identidade-visual.md): paleta, contraste, alternativas e avatar.
- [Plano de funcionalidades](funcionalidades.md): prioridades e documentos individuais.
- [Guia para o Nicolas](guia-nicolas.md): execução local, arquitetura e exercícios.
- [Publicação](publicacao.md): domínio de produção e condições para disponibilização.
- [Verificações](validacao.md): resultados e limites desta entrega.
- [Frontend](../../../ms-cavalcante-ng/README.md) e [API](../../../ms-cavalcante-api/README.md): fontes e comandos.

Esta etapa entrega as cascas geradas dos templates e um protótipo Angular navegável. Não implementa regras de negócio de API, reservas, contratos ou persistência de orçamentos. A infraestrutura de autenticação e a feature de exemplo são herdadas do template. Não foi iniciado o host da API nem acessado banco real. Nenhuma migration nova foi gerada.

O domínio definido pelo mantenedor é **https://cavalcante.olivaw.com.br**. Frontend publicado como protótipo, com HTTPS validado; a API não foi implantada. Ver [registro da publicação](publicacao.md).

## Prévias

- [Landing desktop](preview-desktop.png).
- [Landing no celular](preview-mobile.png).
- [Inventário de páginas e publicações](inventario-conteudo.json).

## Organização

- `brainstorm/`: ideias e estudos livres do cliente.
- `features/`: melhorias e funcionalidades separadas por estado.
- [Funcionalidades concluídas](features/index.md): somente documentos em `done/`.
- [Abordagem inicial](brainstorm/abordagem-inicial.md): conversa e proposta comercial preservadas.
