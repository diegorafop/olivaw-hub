# Cadastro após publicação

Preparado em 31/10/2026, sem cadastro remoto ou envio de mensagem. Publicar API e frontend com as novas operações de Leads; não há alteração de schema ou migration nesta entrega.

## Arquivos

- [Lead](cadastro-lead.json): anúncio integral, origem 99Freelas e iniciativa outbound; nome, URL e contatos desconhecidos permanecem vazios.
- [Proposta](orcamento-proposta.json): rascunho com 27 horas base a R$ 100/h e reserva de 20%, total de R$ 3.240. Complemento personalizado ilustrativo de 16 horas/R$ 1.600 é opcional, fora do total, e precisa ser reestimado após diagnóstico.
- [Análise](analise.md), [estimativas](estimativa.md) e [abordagem](abordagem.md).

O cenário detalhado está dentro da faixa inicial de R$ 2.400–3.840. Não é preço aprovado; a sugestão comercial anterior de R$ 3.200 exige negociação própria e não substitui este cálculo. Desconto permanece em 0% e parcelamento em uma vez até haver condições definidas.

## Procedimento no painel

1. Conferir na lista de Leads a referência/título e o UUID `67ffbaec-579c-47db-b42b-5d1816bb3d89`. Se existir, abrir a ficha; não gerar outro UUID para repetir o cadastro.
2. Em **Leads → Cadastrar lead**, carregar `cadastro-lead.json`. Revisar campos e confirmar cadastro. O arquivo carregado sozinho não persiste nada.
3. Na página de orçamento, carregar `orcamento-proposta.json`, revisar as premissas e salvar. Se houver versão existente, conferir antes de substituir.
4. Na ficha, registrar a etapa **Em análise** e uma nota identificando a prospecção pelo anúncio. Acompanhar o contato na ficha. O rascunho não declara envio, aceite ou contratação.
5. Após aprovação comercial, usar **Ver apresentação** e **Baixar proposta HTML**. O arquivo é autônomo e pode ser compartilhado pelo canal permitido; não há link público nem publicação automática da proposta.

O UUID estável evita duplicação em reenvios deste arquivo (resposta 409). Não existe deduplicação universal por referência entre anúncios/canais. Nome, contato ou URL precisam ser confirmados antes de criar outro cadastro.

## Contratos para automação posterior

- `POST /leads`: body de `cadastro-lead.json`, autorizado por `leads.manage`; retorna 201 ou 409.
- `GET /leads/<uuid>`: obter a revisão atual de `snapshot.revision`.
- `PUT /leads/<uuid>/proposal`: body `{ "revision": <revisão atual>, "proposal": <conteúdo de orcamento-proposta.json> }`; retorna nova revisão ou 409 em concorrência.

Usar somente autenticação já estabelecida; não registrar tokens nos arquivos. Cadastro e envio ocorrerão depois que o mantenedor confirmar a publicação. Para este caso, não há necessidade de migration nem conexão direta ao banco.

[Apresentação HTML já gerada](proposta.html) para revisão do rascunho antes do cadastro. Gere novamente na aplicação se alterar as condições.
