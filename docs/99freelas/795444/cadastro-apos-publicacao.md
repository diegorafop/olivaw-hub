# Registro operacional — 795444

Em 03/11/2026, o mantenedor autorizou cadastro por API direta. UUID estável: `78d8d842-7ad0-51f9-841e-4f75e32979c8`. Conferir referência `795444`, URL e UUID antes de criar; registro divergente exige revisão, sem duplicar.

Contrato consultado: `LeadCreateData` e `LeadProposal` em `ms-olivaw-ng/src/app/features/admin/leads/lead-proposal-model.ts`. Cadastro: `POST /leads`. Orçamento: `PUT /leads/<id>/proposal`, usando revisão de `GET /leads/<id>`. Status draft. Não substituir proposta divergente silenciosamente.

Após importação, revisão visual e exportação oficial pelo painel em `/admin/leads/78d8d842-7ad0-51f9-841e-4f75e32979c8/proposal`, botão **Baixar proposta HTML**. O HTML local inicialmente usa o mesmo gerador para revisão. Não indica publicação externa, envio ou aceite. Não registrar tokens ou credenciais.

## Execução confirmada em 03/11/2026

- Consulta por referência sem registro divergente identificado.
- `POST /leads` retornou HTTP 201.
- Orçamento persistido via API direta com HTTP 200.
- Leitura posterior confirmou igualdade com `orcamento-proposta.json`, revisão 2, status `draft`.
- Proposta comercial local com 2.501 caracteres; anúncio integral, cálculos e links conferidos.
- `proposta.html` gerado localmente pelo mesmo gerador do frontend. Não exportado pelo botão do painel nesta tarefa; o mantenedor solicitou cadastro via API direta.
- Nenhuma mensagem enviada ao anunciante, nenhum aceite ou acesso ao sistema sigiloso.
