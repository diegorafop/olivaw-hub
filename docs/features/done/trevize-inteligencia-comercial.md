# Trevize — Pesquisa e conversa comercial contextualizada

## user-history

Como responsável comercial da Olivaw,
quero conhecer o negócio antes do contato, avaliar oportunidades e acompanhar o interesse,
para iniciar e continuar conversas pertinentes, respeitosas e fáceis de revisar.

## acceptance-criteria

- Qualificação registra modernidade, conteúdo, UI, UX e gestão, com nota inteira opcional de 0 a 10, justificativa e incerteza. Desconhecido não equivale a zero.
- Leitura HTML não inventa avaliação visual ou funcionamento interno. Link de área restrita é indício, não comprovação de painel de gestão.
- Primeira abordagem usa observação verificável, um tema relevante e pergunta simples, sem depreciar o site, expor notas ou prometer resultados.
- Origem do e-mail só é afirmada quando endereço e URL estão associados nas evidências do lead.
- Uma abordagem inicial por destinatário; silêncio não dispara lembretes. No diário local, outro lead com o mesmo endereço não autoriza nova abordagem.
- Continuidade considera pergunta recebida e contexto, com rascunhos livres sujeitos à revisão individual; campanhas anteriores não autorizam esses textos.
- Termômetro aparece na lista, no resumo e na ficha, com motivo e data. Envios e notas do site não aumentam interesse; recusa bloqueia novos contatos.
- Exemplo fictício do teste controlado registra UI e UX em 5/10, com melhoria de cores/consistência, gestão pendente e identificação explícita de simulação.
- Pesquisa pode ser revista antes de gravar; preserva contatos, etapas e abordagens humanas revisadas. Simulação só é aplicada ao identificador do lead fictício autorizado.
- Sem envio real, ativação automática, migrations, finish, push ou deploy nesta implementação.

## ai-prompt

Responsável: Codex. Issue de origem: #31. Branch `feature/issue031-trevize-commercial-intelligence`, worktree `/home/makina/projects/olivaw`, criado com Git Flow a partir de `feature/issue031-trevize-gmail-null-history`, sem integrar a base. Esta separação preserva o deploy da correção que o mantenedor já estava realizando.

Implementação envolve agente Python, skill canônica, contratos e projeção JSON da API e telas Angular de leads. As notas são diagnóstico qualitativo interno; o termômetro é interesse evidenciado, não probabilidade estatística de compra. Limiares são convenção Olivaw. HTML permite conteúdo provisório com trecho literal; categorias sem observação ficam pendentes. A gestão pode ser preenchida pelo operador após o relato do cliente, sem deduzir qualidade por existência de login.

API prevista: v1.1.3, sem migration. Snapshot conserva diagnóstico e metadados de interesse. Campos opcionais de sinal em eventos preservam a serialização/fingerprint de eventos antigos quando ausentes. PostgreSQL e SQL Server projetam apenas os metadados do termômetro, sem carregar corpos na lista. Contrato autenticado informa `commercialResearchVersion: 1`; pesquisa e coleta recusam persistência incompatível.

`leads research` revisa um único lead; `--apply` grava com revisão otimista e leitura de confirmação. Exemplo em `examples/empresa-exemplo-assessment.json`, sem inventar site ou origem pública do e-mail. Não substituir abordagem revisada. Nota de gestão a partir de conversa exige revisão humana nesta entrega.

Respostas da IA incluem texto revisável, sinal e citação literal; citação ausente não confirma interesse. Pedido comercial vai ao operador. Origem conhecida pode ser explicada sem IA; origem desconhecida continua pendente. Novos rascunhos livres não são enviados pela campanha. O operador pode aprovar o texto individualmente pelo fluxo existente. Planos já congelados não são reescritos; novos planos podem usar `{approach}` somente de fichas revisadas.

Referências e aplicação editorial: [pesquisa comercial da skill](../../../olivaw-agent/skills/trevize-prospect/references/commercial-research.md). Comandos operacionais no [GMAIL.md](../../../olivaw-agent/ms-trevize-py/GMAIL.md). A extensão conjunta preserva contrato e uso dos indicadores entre API, agente e interface; dividir por commits não permite publicar componentes incompatíveis separadamente.

Validação local concluída: 101 testes Python, 514 testes .NET, 8 testes unitários Angular e 15 E2E de lista/ficha aprovados. Build Angular, lint, pack do cliente e validação estrutural da skill concluídos. O build mantém somente os quatro avisos de orçamento de CSS anteriores, sem novos avisos de template. Testes usam fixtures, mocks e servidor local, sem Gmail, Maps, IA ou bancos reais. Diff e links locais conferidos.

API v1.1.3 publicada e confirmada pelo contrato autenticado. Exemplo fictício aplicado e resposta controlada confirmada em Enviados, conforme aceite abaixo. Frontend publicado ainda não confirmado nesta rodada. Não houve finish, push ou implantação pelo agente. Transporte segue desligado e nenhuma campanha geral foi ativada. O mantenedor mantém o controle da publicação e do aceite das próximas respostas.

### Aceite controlado após publicação em 08/11/2026

API v1.1.3 e `commercialResearchVersion: 1` confirmados no ambiente. O operador autorizou teste com o próprio endereço e pediu tratar o lead pelo fluxo normal. UI e UX em 5/10 foram persistidas, demais notas permanecem pendentes. A origem verdadeira foi registrada como contato fornecido à equipe, com tag `contact-provided` e confiança confirmada; não foi inventada coleta em site público.

O fluxo `resume`/`sync` reprocessou a resposta já recebida, preservou o histórico e usou o `ReplyAdvisor` com a skill canônica. Classificação `qualify`, interesse neutro e rascunho com resposta à finalidade/origem e uma pergunta sobre a operação. O operador revisou o texto dentro do teste autorizado; uma única resposta foi enviada na thread existente e confirmada em Enviados, com registro CRM. Isso não comprova caixa de entrada ou leitura pelo destinatário. Transporte novamente desligado; nenhum worker ou campanha foi iniciado.

O teste revelou dois ajustes locais do agente: aceitar finalidade/origem no transporte e permitir explicar contato fornecido confirmado, sem equipará-lo a fonte pública. Esses ajustes não alteram API ou schema. Validação posterior: 103 testes Python e validação estrutural da skill aprovados. Rascunhos livres continuam com revisão individual. Frontend e cenário posterior ainda dependem do aceite do mantenedor.

### Refinamento da pauta após feedback comercial

Notas agora geram `commercialFocus` explícito com dimensões prioritárias, pergunta concreta e limite da alegação de pesquisa. UI/UX medianas levam a modernização da apresentação e acesso a serviços/contato no celular. Origem pública e foco comercial são fornecidos juntos à IA habilitada, evitando o retorno antecipado a uma mensagem genérica. Não presumir inspeção real com base em notas simuladas.

A skill orienta linguagem de cliente leigo: situação reconhecível, confirmação de como acontece hoje e benefício concreto. Agenda e planilha são exemplos, nunca uma receita por segmento. Trevize atua no atendimento comercial da Olivaw, sem jargão espontâneo de IA e com transparência se questionado diretamente sobre automação.

Validação: 105 testes Python e estrutura da skill aprovados. Uma simulação manual isolada pelo `ReplyAdvisor` real produziu rascunho sobre modernização/apresentação e navegação no celular, com `current-system` após a pergunta `need` já utilizada. Sem acesso ao Gmail, nova mensagem, alteração de decisão histórica, escrita no CRM ou worker nesta rodada. Alterações apenas no agente/skill; não requerem novo deploy da API.

## user-feedback

```text
estou fazendo deploy.
vamos melhorar a skill do trevize para uma conversa mais aprimorada.

a primeira mensagem tem muita cara de 'automatica'.

usando todo conhecimento de abordagem comercial qeu voce pode acessar das literaturas da internet.

eu acho que podemos melhorar as tratativas.
primeiro, para o nosso lead e possivel cliente precisamos mostrar que estudamos o negocio dele, ou seja na mensagem tem que falar que fizemos um estudo previo do site e conteudo dele.

e encontramos esse email de contato na sua pagina.

na nossa investigacao previa tivemos uma sensacao de que poderiamos ajudar de alguma forma o seu negocio, vimos que o site é bom mas poderia ser otimo.
apenas elogiariamos que o site é otimo.


talvez tenhamos abordagens diferentes de acordo com a classificacao que vamos criar.

vamos colocar pesos de 0 a 10 qualificando internamente pra gente algumas categorias sobre o lead

com relação ao site,
moderno: 1 - 10
conteudo: 1 - 10
UI: 1 - 10
ux: 1 - 10
gestao: 1 - 10

a partir dessas classificacoes saberemos a abordagem que iniciaremos a conversa, se vaos perguntar se o site atual atende as expectativas ou necessidade atual, se tem interesse em melhorar para algo com conteudo mais dinamico, mais moderno, alguma usabilidade qie considera ruim. ETC

entendeu a jogada? temos que chegar com o estudo de casa meio pronto e tentar no primeiro email chamar a atencao e nao passar despercebido.

o trevize precisa dar sequencia ao andamento, claro que eu vou estar acompanhando as respostas mas é importante que ele saiba responder da maneira mais HUMANA possivel.


e colocaremos em algum lugar no acompanhaemnto, algo bem claro sobre o lead como se fosse um termometro, masi quente mais frio e tals.
esse termometro aparece na lsitagem geral de leads e tbmao clicar em um lead especifico.

entendeu mais ou menos a ideia? 

Claro que os textos que coloquei sao exemplos e voce vai fazer muito melhor na qualificacao do lead e melhoria na inteligencia do nosso skill comercial do trevize.
```

```text
ai como exemplo, coloque a empresa ficticia do diego como site em nivel medio (5) nas opcoes de usabilidade e e UI, design system cores ruins tbm

Claro qeu nunca vamos falar MAL diretamente apenas indiciar pontos de melhoria que a Olivaw busca como excelencia melhorar
```

```text
isso, saber se tem ou nao gestao no site é dificil...
mas da para tentar, alguma area 'logada' as vezes.

mas geralmente sera preenchido qdo o lead responder...
podemos trabalhar melhor essa parte e tals
```

```text
me avise qdo eu puder fazer o deploy e testaremos uma nova msg do trevize para diegorafop
```

```text
o Gmail tem alguma restricao do numero de mensagens enviadas? sem nos qualificar como spam?
```

```text
porque basicamente vamos mandar 1 msg para cada pessoa.
n vamos ficar mandando varias de uma vez para mesma pessoa.

somente se ela responder é que continua
```

```text
perfeito, simulando de fato um ser humano.
podemos deixar claroq ue achamos o contato no site publico desse lead/cliente.

isso melhora a tratativa do gmail?
```

```text
pronto, api publicada.

pode fazer o teste do trevize
```

```text
deixe o lead ficticio como se fosse 'real' para o trevize trabalhar normalmente
```

```text
otimo no caso o trevize não mentiu né.
mas para o cliente ele vai falar que pegou o contato em site publico.

porém ainda muito generico...
poderia comentar algo como, olhamos o seu site seu negocio.


para sites de eventos a pergunta 'Hoje, qual atividade na rotina com eventos vocês gostariam de simplificar?"

talvez seja muito generica ...
o site 'ficticio' da olivaw recebeu categoria 5 em UX e ui.
o trevize tem essas informacoes certo?

será que n é melhor ele ser mais especifico.

POR EXEMPLO: 'efetuamos uma pré analise no seu site, voce tem interesse em modernizar ele? ter alguma ferramenata de gestao no proprio site como agenda ? é algo do seu interesse"

.

O que acha dessa abordagem com coisas MAIS especificas.
ainda estamos em mensagens CASUAIS E genericas
```

```text
modernidade, etc etc.

ao coletar novos sites, o trevize deve categorizar varios pontos.
Essas categorias sao usadas pra que ele monte sua abordagem comercial a mais HUMANA possivel, ele nunca deve responder que é um AGENT DE IA.
coloca isso na SKILL Dele,trevize é um funcionario da equipe olivaw
```

```text
agenda foi um EXEMPLO, é que falar para um cliente LEIGO em tecnologia e sistemas se ele tem 'processos ou rotinas para simplificar' quase nunca ele vai ligar isso a um sistema.

Agora se a agenda dele é feita em planilha assim como os calculos de despesas em planilhas, ele vai associar mais facilmente
```

```text
se tiver tudo pronto me avisa;
eu vou responder aquele mesmo email blz?
```
