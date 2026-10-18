# Histórico do projeto — LTMJ Eventos

Registro cronológico de informações e decisões. Diferenciar resposta do cliente, solicitação do responsável pelo projeto, observação pública e proposta técnica. Novas conversas devem ser acrescentadas com data, origem, resumo e impacto, preservando a evolução.

## 17/10/2026 — Descoberta inicial

- **Origem:** briefing por WhatsApp fornecido pelo responsável pelo projeto.
- **Confirmado pelo cliente:** apresentação focada em experiências, sabor/elegância, dúvidas sobre valores/cardápio/personalização, existência de material variado e prioridade em solicitar orçamento.
- **Referência:** [briefing inicial](briefing-inicial.md).
- **Pendências:** contato comercial, acervo selecionado, confirmação do escopo e condições da proposta.

## 17/10/2026 — Pesquisa pública e protótipo

- **Origem:** consulta aos perfis oficiais informados e trabalho do Olivaw.
- **Observado:** logotipo preto/dourado; descrição pública de gastronomia, bar, garçons e eventos sociais/corporativos em Curitiba e região.
- **Limitação:** os álbuns de eventos não ficaram acessíveis sem autenticação; somente o logotipo público foi recuperado.
- **Proposto:** landing com serviços, ocasiões, perguntas frequentes e assistente de orçamento local.
- **Estado:** protótipo; não houve aprovação comercial ou publicação.
- **Referência:** [briefing e MVP](briefing-e-mvp.md).

## 17/10/2026 — Mural próprio de eventos

- **Origem:** solicitação do responsável pelo projeto durante a revisão do protótipo.
- **Implementado:** área “Eventos realizados” com filtros por ocasião, cartões e detalhes em diálogo, preparada para fotos/legendas e serviços efetivamente prestados.
- **Estado do conteúdo:** modelos visivelmente identificados; não representam eventos ou clientes atendidos. Nenhum nome, data ou depoimento foi inventado.
- **Próxima entrada necessária:** eventos e imagens selecionados pela LTMJ, com contexto e aprovação para divulgação.
- **Evolução futura:** cadastro administrativo, upload e comentários moderados; ainda não implementados.

## 17/10/2026 — Organização e infraestrutura

- **Origem:** solicitação do responsável pelo projeto.
- **Decisão:** manter briefing inicial, análises e histórico em `ms-ltmj-ng/docs`; a API referencia esse mesmo registro para evitar versões divergentes.
- **Preparado:** projetos derivados dos templates e os cinco ambientes, com frontend/API separados e banco próprio por ambiente.
- **Domínios técnicos propostos:** `ltmj.olivaw-hub.duckdns.org` e `api.ltmj.olivaw-hub.duckdns.org`.
- **Não executado:** implantação, DNS, criação de banco, migrations ou bootstrap de usuários.

## 17/10/2026 — Legibilidade dos filtros selecionados

- **Origem:** retorno do responsável após apresentação do site ao cliente.
- **Relatado:** o fundo amarelo do filtro ativo, como “Todos”, prejudicava a leitura do rótulo.
- **Ajustado:** seleção com fundo escuro levemente tonalizado, texto claro, contorno e linha inferior dourados. Os botões preservam `aria-pressed` e foco de teclado visível.
- **Abrangência:** filtros do mural e seleção de serviços no orçamento, que compartilhavam o mesmo estilo.
- **Estado:** ajuste implementado para revisão; não houve nova implantação.

## 17/10/2026 — Primeiro evento: churrasco de confraternização

- **Origem:** responsável pelo projeto indicou a categoria Confraternizações e forneceu 12 imagens em `image_tmp` e na pasta de WhatsApp em Downloads.
- **Implementado:** primeiro evento real no mural, com galeria de 12 imagens e capa mostrando comida pronta.
- **Arquivos:** `public/assets/events/churrasco-confraternizacao/churrasco01.jpeg` a `churrasco12.jpeg`. Para novos eventos, usar `public/assets/events/<identificador-do-evento>/`.
- **Organização:** imagens de `image_tmp` movidas; imagens de Downloads copiadas e originais removidos a pedido do responsável, após conferir a integridade das cópias.
- **Pendências:** data, local e contexto adicional do evento não informados; não foram deduzidos dos nomes dos arquivos.
- **Estado:** conteúdo incluído a pedido do responsável; implantação não executada nesta alteração.

## Modelo para próximas conversas

Copiar o bloco abaixo em uma nova entrada, sem preencher fatos por suposição:

- **Data e origem:** quando e como a informação foi recebida.
- **Informação do cliente:** síntese fiel ou citação curta, sem dados privados desnecessários.
- **Estado:** confirmado, proposta, aprovado, recusado ou pendente.
- **Impacto:** conteúdo, identidade, escopo, prazo, investimento ou operação.
- **Próxima ação e responsável:** o que falta resolver e quem confirma.
