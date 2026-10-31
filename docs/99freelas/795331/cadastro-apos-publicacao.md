# Cadastro e apresentação para revisão

Arquivos preparados localmente, sem cadastro remoto ou envio. A proposta JSON/HTML preserva valores do cenário inicial como referência histórica. Não importar ou enviar como orçamento da alternativa com agentes Python; reestimar e revisar o documento antes. UUID estável: `4970e265-b451-5b48-84a7-27d772ed8819`.

1. Verificar na lista de Leads a referência `795331`, título e UUID antes de importar; se já existir, abrir a ficha e comparar as informações.
2. Após confirmar disponibilidade das operações no ambiente, carregar `cadastro-lead.json` em Leads → Cadastrar lead; revisar e confirmar. Não criar contatos, nome comercial ou URL fictícios.
3. Abrir `/admin/leads/<id>/proposal`, carregar `orcamento-proposta.json`, revisar premissas e salvar. Obter a revisão atual antes de qualquer atualização automatizada.
4. Usar **Baixar proposta HTML** para gerar a apresentação oficial no painel após revisão. O `proposta.html` local usa o mesmo gerador do frontend para revisão prévia; não foi baixado de uma ficha persistida.
5. Registrar separadamente eventual envio, negociação ou aprovação; preparar esses arquivos não indica nenhuma dessas ações.

Contrato consultado: `LeadCreateData` e `LeadProposal` em `ms-olivaw-ng/src/app/features/admin/leads/lead-proposal-model.ts`. Não há autorização para publicar, importar ou enviar nesta tarefa.
