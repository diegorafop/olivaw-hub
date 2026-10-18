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

## 17/10/2026 — Backoffice de portfólio, issue 009

- **Origem:** história de usuário enviada pelo responsável pelo projeto.
- **Escopo solicitado:** gestão restrita e mobile, título/descrição/categoria, várias fotos, edição, visibilidade pública e exclusão confirmada.
- **Implementado:** `/gestao/entrar` e `/gestao`, autorização `portfolio.manage`, API CQS em inglês com leituras `Read`, multipart, persistência EF e armazenamento em volume. Nenhuma conta individual da equipe foi criada.
- **Decisão técnica desta entrega:** armazenamento local persistente atrás de `IPhotoStorage`; integração S3 continua como evolução. O volume deve ser copiado ao trocar o servidor.
- **Mural:** passa a ler a API, sem fallback fixo que possa republicar eventos ocultos. Importação manual do churrasco disponível no painel.
- **Pendência operacional:** migration autorizada, preparação do banco/identidade, backup e implantação. Nenhum desses passos foi executado nesta tarefa.
- **Referência:** [gestão de portfólio](gestao-portfolio.md).

## 17/10/2026 — Novas seleções de acervo e acesso da equipe

- **Origem:** responsável enviou dois JPEGs e três ZIPs em Downloads e pediu três ou quatro exemplos adicionais.
- **Recebido:** 91 fotos e 33 vídeos. Fotos preservadas e conferidas em `docs/acervo-recebido/2026-10-17`; vídeos mantidos fora do Git em Downloads. Origens de fotos removidas após a conferência, conforme autorizado.
- **Preparado:** quatro conjuntos demonstrativos com 14 fotos, além do churrasco inicial. Prévia em `/previa-portfolio#eventos` e carregamento individual no cadastro. Agrupamento/categoria pendentes de confirmação; não representam quatro eventos reais distintos comprovados.
- **Acesso solicitado:** usuário `ltmj` na identidade da própria API LTMJ, com `portfolio.manage`, sem root. Root de cada cliente é reservado à gestão Olivaw Hub.
- **Conferido:** chaves de assinatura JWT da LTMJ não são compartilhadas com outros serviços; os cinco ambientes têm chaves distintas. Nenhum valor foi registrado neste histórico.
- **Pendências do usuário:** e-mail obrigatório e definição de senha compatível com a política atual de 12 caracteres, ou autorização de ajuste específico da LTMJ. Nenhum usuário foi criado no servidor.

## 17/10/2026 — Política de senha e e-mail da equipe confirmados

- **Origem:** instrução explícita do responsável.
- **Decisão:** remover o mínimo de 12 caracteres na identidade compartilhada, incluindo cadastro, troca de senha e bootstrap, mantendo senha obrigatória, limite de 128 e preservação integral dos caracteres. A alteração também atende ao template limpo por referência ao mesmo módulo.
- **Conta planejada:** login `ltmj`, e-mail `ltmj@teste.com.br`, sem confirmação por e-mail. A senha fornecida não é reproduzida nos documentos.
- **Permissões:** somente gestão de portfólio; root continua reservado ao Olivaw Hub. Chaves JWT não foram alteradas.
- **Estado:** código e curl atualizados; usuário não criado remotamente.

## 17/10/2026 — Somente fotos e descarte dos vídeos recebidos

- **Origem:** responsável confirmou que a versão atual da LTMJ deve usar somente fotos e autorizou apagar os vídeos de Downloads.
- **Conferido:** não havia vídeos no código, assets públicos ou arquivos versionados da LTMJ; nenhuma referência a player de vídeo no site.
- **Executado:** 33 arquivos MP4 da pasta LTMJ em Downloads removidos conforme solicitado. Os 91 arquivos de fotos recebidos continuam preservados no projeto.
- **Escopo:** API e upload continuam aceitando somente JPEG, PNG e WebP. Não foram removidos arquivos de outros projetos.

## 17/10/2026 — Migration inicial e nomes de tabelas

- **Origem:** responsável solicitou gerar a migration e reafirmou lowercase sem hífens ou underscores nas tabelas.
- **Corrigido:** nomes de domínio `portfolioevent` e `portfoliophoto` no mapeamento; regra documentada no cliente e template limpo.
- **Gerado:** migration PostgreSQL `20261018014627_InitialIdentityAndPortfolio`, designer e snapshot. Contexto de design time permite gerar sem inicializar a API ou acessar a AWS.
- **Conferido:** build sem erros/avisos e geração de SQL idempotente para revisão. Nenhuma migration aplicada remotamente.
- **Conexão:** usuário PostgreSQL atual `postgres`, banco `ltmj_prd`, search path `ms_ltmj`. Não confundir o usuário PostgreSQL com as contas root e `ltmj` da aplicação.
- **Pendência:** confirmar se já há tabelas antigas no banco antes de aplicar a migration inicial, configurar aplicação no startup e bootstrap do root, então concluir cadastro e vínculos da equipe.

## 17/10/2026 — E-mail do root Olivaw confirmado

- **Origem:** responsável confirmou o e-mail da gestão Olivaw para root.
- **Configurado:** e-mail do root nos dotenv da API LTMJ; a conta da equipe continua separada, com login `ltmj` e sem root.
- **Estado:** segredo e chaves JWT preservados; aplicação de migrations e bootstrap continuam desabilitados enquanto a autorização solicitada está pendente. Cadastro e vínculo ainda não concluídos devido ao HTTP 500 no login.

## 17/10/2026 — Startup obrigatório de banco e root

- **Origem:** responsável determinou que migrations e bootstrap sempre rodem no startup, sem flags de habilitação.
- **Implementado:** API LTMJ e template limpo sempre aplicam migrations pendentes e garantem root antes de atender requisições; falhas impedem a inicialização com banco incompleto.
- **Configuração:** removidas opções de habilitação de migrations e bootstrap dos dois projetos. E-mail root confirmado e segredo existente preservados; bootstrap não redefine root existente.
- **Compatibilidade:** consumidores anteriores da biblioteca de identidade mantêm o comportamento do wrapper de inicialização já usado por eles. A LTMJ e o template usam diretamente o bootstrap obrigatório após as migrations.
- **Pendência operacional:** publicar a nova imagem; o deploy anterior não contém esse comportamento. Cadastro e vínculo da equipe aguardam o login root funcionar.

## Modelo para próximas conversas

Copiar o bloco abaixo em uma nova entrada, sem preencher fatos por suposição:

- **Data e origem:** quando e como a informação foi recebida.
- **Informação do cliente:** síntese fiel ou citação curta, sem dados privados desnecessários.
- **Estado:** confirmado, proposta, aprovado, recusado ou pendente.
- **Impacto:** conteúdo, identidade, escopo, prazo, investimento ou operação.
- **Próxima ação e responsável:** o que falta resolver e quem confirma.
