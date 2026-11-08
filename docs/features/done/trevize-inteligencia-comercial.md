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

Publicação e aceite no ambiente permanecem pendentes. API v1.1.3 e frontend devem ser publicados da feature pelo mantenedor, sem merge antecipado em develop. Exemplo fictício está preparado localmente; a aplicação na ficha real de teste ocorrerá após publicação compatível, antes de uma nova mensagem aprovada para o endereço do mantenedor. Chaves de envio permanecem desligadas. Nenhuma mensagem foi enviada nesta implementação. Nenhum contato autorizado por esta documentação. O mantenedor publica a feature após revisão; não fazer finish automaticamente.

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
