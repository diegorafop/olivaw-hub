# Comentários e reações em plataforma existente

## user-history

Como visitante da plataforma,
quero publicar comentários, votar nos comentários e reagir aos conteúdos,
para compartilhar minhas impressões e acompanhar as interações da comunidade.

Como moderador,
quero revisar e gerenciar comentários com apoio de filtros,
para manter um ambiente de interação adequado.

## acceptance-criteria

- Publicar comentários vinculados ao conteúdo correto e visualizar somente os liberados pela política de moderação.
- Exibir votos positivos e negativos de cada comentário com quantidades identificáveis.
- Exibir reações visuais por conteúdo e total por opção; conjunto final sujeito à confirmação, inicialmente Gostei, Não gostei, Triste e Bravo.
- Permitir uma reação ativa por identidade e conteúdo e um voto ativo por identidade e comentário; troca e remoção são proposta a confirmar no discovery.
- Reenvios e cliques repetidos não incrementam contagens indevidamente, conforme a identidade acordada. Limites para visitantes anônimos devem ser explícitos.
- Moderador autorizado aprova, oculta e exclui comentários; visitantes não acessam essas operações.
- Filtros configurados sinalizam spam e linguagem inadequada com amostras acordadas, permitindo revisão humana de falsos positivos.
- Interface utilizável no computador, celular e teclado, com rótulos acessíveis e indicação de falhas no envio.
- Controles contra bots, abuso e conteúdo executável verificados em homologação, com testes de duplicação e concorrência.
- Entrega inclui orientação administrativa, evidências dos testes acordados e procedimento de reversão.

## ai-prompt

Estado: discovery. Responsável pela análise documental: Codex, em 31/10/2026. Sem contratação, implementação ou testes de aplicação. Não mover para ready antes de resolver as decisões abaixo.

### Escopo e dependências

Implementar integração econômica de comentários, votos, moderação e reações em plataforma existente, após aprovação de proposta. Ver [análise](../analise.md), [estimativa](../estimativa.md) e [abordagem](../abordagem.md).

Confirmar tecnologia, versões, acessos, volume, identidade, política de moderação, reações finais e viabilidade dos componentes. Registrar decisão sobre visitantes anônimos, mudança/remoção de votos, custos externos e critérios de aceite com o cliente. Login existente é preferível quando necessária unicidade por conta; cookies e IP não garantem unicidade por pessoa.

Priorizar recursos nativos e componentes mantidos. Desenvolver somente lacunas justificadas e orçadas. Migração, anexos, notificações, novo login e redesign estão fora da estimativa inicial. Filtros não substituem moderação humana.

### Execução futura

Antes de desenvolver, definir responsável e branch Git Flow próprios. Branch observada na análise: feature/issue026-admin-client, sem vínculo comprovado com este orçamento. Pasta 795293 é identificador do orçamento, não issue. Worktree inicial: orcamentos/795293, com destino solicitado docs/99freelas/795293.

Esta tarefa autoriza documentação e organização local. Não autoriza contato com lead, contratação de ferramentas, acesso a banco real, migrations, finish, push ou implantação. Após contratação, validar em homologação os cenários descritos na análise e registrar evidências antes de review. Não há evidência de conclusão técnica para done.

## user-feedback

Texto original do anúncio fornecido pelo mantenedor, preservado sem correções:

```text
Projeto exclusivo
Implementação de comentários e reações em plataforma web
ontem às 22:39
Descrição do Projeto:
Procuro um profissional com experiência em desenvolvimento web para implementar dois recursos de interação em uma plataforma digital já existente.

O projeto envolve a integração de funcionalidades que permitam aos usuários interagir com conteúdos publicados, de maneira simples e intuitiva.

1. Sistema de comentários

Implementação de uma seção de comentários vinculada às páginas de conteúdo, permitindo que os visitantes:

- Escrevam e publiquem comentários.
- Visualizem os comentários de outros usuários.
- Curtam ou marquem como não gostei nos comentários publicados.
- Visualizem a quantidade de interações recebidas por cada comentário.

Também precisamos de recursos de moderação, preferencialmente com filtros automáticos para identificar spam, mensagens ofensivas, preconceituosas e outros conteúdos inadequados.

Deve existir alguma ferramenta de administração que permita gerenciar os comentários, incluindo aprovação, ocultação e exclusão.

2. Sistema de reações aos conteúdos

Implementação de um recurso que permita aos visitantes expressar suas impressões sobre cada conteúdo publicado.

A ideia é utilizar opções visuais e minimalistas, como emojis ou ícones. Algumas possibilidades:

- Gostei.
- Não gostei.
- Triste.
- Bravo.

O usuário deverá conseguir selecionar uma reação, e o sistema deverá contabilizar e exibir a quantidade de cada opção recebida pelo conteúdo.

Estamos abertos a sugestões de apresentação visual, desde que a experiência seja simples e funcional.

Requisitos técnicos

- Integração dos dois recursos à estrutura já existente.
- Funcionamento adequado em computadores e dispositivos móveis.
- Interface responsiva e visualmente organizada.
- Proteção contra spam, bots e interações duplicadas.
- Ferramentas de administração e moderação.
- Testes para garantir o funcionamento correto dos recursos.

Informações adicionais

Estamos abertos à utilização de plugins, extensões ou soluções já disponíveis no mercado, desde que sejam confiáveis e atendam às funcionalidades descritas.

A prioridade é encontrar uma implementação prática, funcional e de custo acessível. Não existe necessidade de desenvolver um sistema personalizado do zero caso haja uma alternativa mais simples e econômica.

Pedimos que os profissionais interessados informem:

- Qual solução técnica recomendam para implementar os recursos.
- Se pretendem utilizar plugins ou desenvolver as funcionalidades.
- Se existem custos adicionais com ferramentas ou serviços externos.
- Qual o prazo estimado para a execução.

Será um diferencial ter experiência com integração de funcionalidades em plataformas já existentes e apresentar exemplos de trabalhos semelhantes.
```
