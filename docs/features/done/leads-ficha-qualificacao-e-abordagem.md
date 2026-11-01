# Leads — Ficha completa, qualificação e abordagem de parceria

## user-history

Como operador, quero consultar um resumo na lista e trabalhar na ficha completa de cada Lead, para qualificar oportunidades e preparar conversas pertinentes sem enviar mensagens automaticamente.

## acceptance-criteria

- Painel lateral somente para consulta, com status, resumo e timeline e link Abrir ficha.
- Ficha protegida em `/admin/leads/<id>/details`, com breadcrumb, informações originais, orçamento e acompanhamento com nota/etapa.
- Qualificação editável com segmento, região, presença digital, contatos, fontes, fatos confirmados, observações, pontos positivos e perguntas; separar observação de hipótese.
- Rascunho de abordagem personalizado e revisável, sem envio automático, sem contato marcado como realizado e sem promessa de risco zero.
- API protegida para salvar qualificação com revisão otimista, autoria/data e preservação da jornada e proposta. Endpoint público não altera a qualificação interna.
- Manter compatibilidade dos Leads já existentes e preservar rascunhos em falha ou conflito.

## ai-prompt

Desenvolvimento concluído e validado pelo Codex na branch `feature/issue027-agent-prospector`, worktree `/home/makina/projects/olivaw`, issue 027 inferida do branch. Escopo: frontend Leads, contrato/handler e endpoint administrativo de qualificação no snapshot existente, testes e documentação. Não gerar migrations: não há mudança de schema prevista. Sem commit, push ou deploy.

A ficha permite preparo local e edição de rascunhos baseados nos campos registrados. A coleta e qualificação por IA são um recorte separado em [Trevize](trevize-coleta-agendada-de-leads.md); não houve chamada paga nem lote real nesta validação. A [discovery do agente](../discovery/trevize-prospeccao-e-qualificacao-de-leads.md) mantém as evoluções futuras.

Posicionamento: parceria, nova empresa de sistemas, atenção à necessidade real, apresentação positiva, pergunta simples. Infraestrutura, backups, domínio e gestão entram conforme o interesse, sem sobrecarregar a primeira mensagem. Não assumir infraestrutura deficiente, falta de backup ou ausência de site sem confirmação.

### Implementação e evidências

Painel lateral usa acompanhamento somente leitura. A ficha em `/admin/leads/:id/details` concentra perfil, fontes, contatos, prioridades, perguntas, rascunho e acompanhamento. A proposta e breadcrumb apontam para essa ficha. Tags e presença digital são independentes do status comercial. Lista projeta telefone, segmento e website do JSON no banco, sem carregar o snapshot completo; busca inclui esses campos. WhatsApp confirmado da qualificação complementa o contato original, sem transformar telefone em WhatsApp. Contatos não são enviados automaticamente.

`PUT /leads/{id}/qualification` exige `leads.manage`; snapshot preserva proposta e jornada, com revisão otimista e última autoria/data pelo servidor. O endpoint público rejeita e preserva os campos internos. Notas e qualificação têm revisões independentes; salvar uma não apaga o rascunho da outra. Limite de 32.000 caracteres no snapshot completo. Sem alteração de schema e sem migrations necessárias.

Validações: suíte API com 467 testes; frontend com 46 testes unitários, build e lint; 16 E2E de ficha, lista e orçamento em desktop/mobile, autorização, persistência e conflito. Revisão visual com dados simulados. Publicação pelo mantenedor pendente; não foram atualizados Leads reais nesta entrega.

## user-feedback

```text
a nossa area de leads deve oferecer um suporte maior para trabalhar com os novos tipos de leads.
acho que ao clicar no lead ao lado pode aparecer sim a lista de resumo com timeline e status e tals.

porem sem campos para nota interna ou trocar etapa mas algum button que navegue pra uma tela daquele lead tipo /leads/guid/details algo assim.

nessa tela teriamos mais informacoes daquele lead as quais vamos começar a captar no nosso site.
e voce tem razão n pode ser uma msg padrao para todo cliente, mas algo trabalhado que o trevize qualifique as oportunidades e tbm gere nessa pagina de leads textos sugestoes do approach inicial.

n vamos apenas criticar o site da pessoa, mesmo que seja ruim, teremos uma abordagem transparente nós somos uma nova empresa de desenvolvimento de sistemas e avaliamos sites da regiao de curitiba e regiao metropolitana. Gostariamos de trabalhar com voces, estao satisfeitos com seu site atual, gostariam de melhorar/trocar/ ter uma area de gestao no site, cadastro interno de despesas, receitas, controle finaneiro que hj talvez voce faça em planilhas e tals.

Enfim seremos não tao intrusivos na venda, mas deixar claro que buscamos parcerias.
como está sua sua infraestrtura hoje, voce tem o acesso, é do programador que fez ou vendeu o site para voce.

enfim, nós da Olivaw buscamos clientes do seu perfil para cuidar de tudo para voce, sem se preocupar com a segurança dos seus dados, backup de suas informacoes, dominio, custo de armazenamento em nuvem, custo de dominio, ip etc etc etc.

claro que cada caso é um caso mas podemos pensar em oferecer essa parceria com os clientes.

antes de codar me diga o que acha

ficou otimo! pode seguir
```
